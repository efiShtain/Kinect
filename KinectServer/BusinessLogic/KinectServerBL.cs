using KinectServer.Common;
using KinectServer.Communication;
using KinectServer.DTO;
using KinectServer.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{

    public class KinectServerBL
    {
        private IKinectBusinessLogic _kinect;
        private GameLogic _game;
        private WebsocketBL _server;
        private Dictionary<string, List<List<PointRealWorld>>> _recordedStages;
        private Hits _recordedHits;
        private TrajectoriesData _trajectories;
        private bool _waitingForPoint;
        private CustomPointsListParser _pointsParser;
        private string _currentPlayer;
        public KinectServerBL(IKinectBusinessLogic kbl, int port)
        {
            _game = new GameLogic();
            _waitingForPoint = false;
            Initialize(kbl, port);
            _trajectories = new TrajectoriesData();
            _pointsParser = new CustomPointsListParser();
            
        }

        #region Public Methods
        public void Initialize(IKinectBusinessLogic kbl, int port)
        {
            if (_server != null)
            {
                _server.NewSessionStarted -= _server_NewSessionStarted;
                _server.NewMessageRecived -= _server_NewMessageRecived;
                _server.SessionClosed -= _server_SessionClosed;
                _server.StopServer();
            }

            _server = new WebsocketBL(port);
            _server.NewSessionStarted += _server_NewSessionStarted;
            _server.NewMessageRecived += _server_NewMessageRecived;
            _server.SessionClosed += _server_SessionClosed;

            if (_kinect != null)
            {
                _kinect.KinectAvailabletyChanged -= _kinect_KinectAvailabletyChanged;
                _kinect.NewJointsDataReady -= _kinect_NewJointsDataReady;
            }
            
            _kinect = kbl;
            _kinect.KinectAvailabletyChanged += _kinect_KinectAvailabletyChanged;
            _kinect.NewJointsDataReady += _kinect_NewJointsDataReady;

            _recordedStages = new Dictionary<string, List<List<PointRealWorld>>>();
            _recordedHits = new Hits();
            _server.StartServer();
        }

        #endregion

        #region Public Properties

        #endregion

        #region Events
        public event Action<List<PointRealWorld>> NewKinectDataReady;
        public event Action<KinectState> KinectStateChanged;
        public event Action<string> GameStateChanged;
        public event Action SessionStarted;
        public event Action SessionClosed;
        #endregion

        #region Private Methods

        private void _server_SessionClosed()
        {
            if (_kinect != null)
            {
                _kinect.StopKinect();
                if (KinectStateChanged != null)
                {
                    KinectStateChanged(KinectState.STOPPED);
                }

                GenerateMatlabFiles();
            }
            if (SessionClosed != null)
            {
                SessionClosed();
            }
        }

        private void _server_NewMessageRecived(Communication.ClientMessage message)
        {
            switch (message.Type)
            {
                case Protocol.PLAYER_NAME:
                    _currentPlayer = message.Data;
                    _pointsParser.Init();
                    break;
                case Protocol.KINECT_START:
                    if (!_kinect.IsOpen)
                    {
                        _server.PostMessage<string>("Starting Kinect", Protocol.KINECT_START);
                        _kinect.IsGetNextPoint = false;
                        _kinect.IsNextPointMoving = false;
                        _kinect.StartKinect();
                    }
                    break;
                case Protocol.KINECT_STOP:
                    if (_kinect.IsOpen)
                    {
                        _server.PostMessage<string>("Stopping Kinect", Protocol.KINECT_STOP);
                        _kinect.StopKinect();
                    }
                    break;
                case Protocol.GET_NEXT_INSTRUCTIONS:
                    var instruction = _game.NextInstruction;
                    if (instruction.State == "random")
                    {
                        _trajectories.Random = new List<PointRealWorld>();
                        var enemiesList = _pointsParser.GetPointsForStage(_currentPlayer, "random");
                        _kinect.SetEnemiesList(enemiesList);
                        instruction.EnemyCount = enemiesList.Count;
                    }
                    else if (instruction.State == "moving")
                    {
                        _trajectories.Moving = new List<PointRealWorld>();
                        var enemiesList = _pointsParser.GetPointsForStage(_currentPlayer, "moving");
                        _kinect.SetEnemiesList(enemiesList);
                        instruction.EnemyCount = enemiesList.Count;
                    }
                    _server.PostMessage<Instruction>(instruction, Protocol.GET_NEXT_INSTRUCTIONS);
                    if (GameStateChanged != null)
                    {
                        GameStateChanged(instruction.State);
                    }
                    break;
                case Protocol.GET_NEXT_STAR_RANDOM_POSITION:
                    _waitingForPoint = true;
                    _kinect.IsGetNextPoint = true;
                    _kinect.IsNextPointMoving = false;
                    break;
                case Protocol.GET_NEXT_STAR_MOVING_POSITION:
                    _waitingForPoint = true;
                    _kinect.IsGetNextPoint = true;
                    _kinect.IsNextPointMoving = true;

                    break;
                case Protocol.MOVING_HITS_DATA:
                    var hitDataMoving = JsonConvert.DeserializeObject<HitData>(message.Data);
                    if (hitDataMoving != null)
                    {
                        var hit = _recordedHits.Moving.LastOrDefault();
                        if (hit != null)
                        {
                            hit.T2 = hitDataMoving.skeletonIndex;
                            hit.HitJointIndex = hitDataMoving.jointIndex;
                        }
                        else
                        {
                            throw new FieldAccessException("Missing hit");
                        }
                    }
                    break;
                case Protocol.RANDOM_HITS_DATA:
                    var hitDataRandom = JsonConvert.DeserializeObject<HitData>(message.Data);
                    if (hitDataRandom != null)
                    {
                        var hit = _recordedHits.Random.LastOrDefault();
                        if (hit != null)
                        {
                            hit.T2 = hitDataRandom.skeletonIndex;
                            hit.HitJointIndex = hitDataRandom.jointIndex;
                        }
                        else
                        {
                            throw new FieldAccessException("Missing hit");
                        }
                    }
                    break;
                case Protocol.GAME_DONE:
                    _kinect.StopKinect();
                    GenerateMatlabFiles();
                    break;
            }
        }

        private void _server_NewSessionStarted()
        {
            _game.ResetInstructionsCounter();
            if (SessionStarted != null)
            {
                SessionStarted();
            }
        }

        private void _kinect_NewJointsDataReady(DisplayData screenData, List<PointRealWorld> matlabData, PointRealWorld traj)
        {
            _server.PostMessage<DisplayData>(screenData, Communication.Protocol.SKELETON_DATA);
            if (_waitingForPoint)
            {
                if (_game.CurrentState == "random")
                {
                    _recordedHits.Random.Add(new Hit() { T1 = screenData.SkeletonIndex });
                    _server.PostMessage<ScreenPoint>(screenData.NextEnemyPoint, Communication.Protocol.GET_NEXT_STAR_RANDOM_POSITION);
                }
                else if (_game.CurrentState == "moving")
                {
                    _recordedHits.Moving.Add(new Hit()
                    {
                        T1 = screenData.SkeletonIndex,
                        //Trajectory = new TrajectoriesData()
                    });
                    _server.PostMessage<ScreenPoint>(screenData.NextEnemyPoint, Communication.Protocol.GET_NEXT_STAR_MOVING_POSITION);
                }
                _waitingForPoint = false;
            }
            if (_game.CurrentState == "random")
            {
                if (traj != null)
                {
                    _trajectories.Random.Add(traj);                         //This is a real location of a moving object
                }
                else
                {
                    _trajectories.Random.Add(new PointRealWorld(0, 0, 0));  //This is padding so indecis will match skeleton indecis
                }
            }
            else if (_game.CurrentState == "moving")
            {
                if (traj != null)
                {
                    _trajectories.Moving.Add(traj);                         //This is a real location of a moving object
                }
                else
                {
                    _trajectories.Moving.Add(new PointRealWorld(0, 0, 0));  //This is padding so indecis will match skeleton indecis
                }
            }
            if (_game.CurrentState != null && _game.CurrentState != "win")
            {
                if (!_recordedStages.ContainsKey(_game.CurrentState))
                {
                    _recordedStages.Add(_game.CurrentState, new List<List<PointRealWorld>>());
                }
                _recordedStages[_game.CurrentState].Add(matlabData);

                //if (_game.CurrentState == "moving")
                //{
                //    var lp = _recordedHits.Moving.LastOrDefault();
                //    if (lp != null)
                //    {
                //        lp.Trajectory.Moving.Add(new PointRealWorld(screenData.NextEnemyPoint.X, screenData.NextEnemyPoint.Y, 0));
                //    }
                //}
            }

            if (NewKinectDataReady != null)
            {
                NewKinectDataReady(matlabData);
            }
        }

        private void _kinect_KinectAvailabletyChanged(bool state)
        {
            _server.PostMessage<bool>(state, Communication.Protocol.KINECT_CHANGED_AVAILABILITY);
            if (KinectStateChanged != null)
            {
                if (state)
                {
                    KinectStateChanged(KinectState.READY);
                }
                else
                {
                    KinectStateChanged(KinectState.NOT_READY);
                }
            }
        }

        private void GenerateMatlabFiles()
        {
            MatlabDriver.SetFolder();
            if (_recordedStages.Count != 0)
            {
                foreach (var data in _recordedStages)
                {
                    //MatlabDriver.ToMat(data.Value, data.Key);
                    if (data.Key == "random")
                    {
                        MatlabDriver.ToMatAll(data.Value, _recordedHits.Random,_trajectories.Random,_currentPlayer);
                    }
                    else if (data.Key == "moving")
                    {
                        MatlabDriver.ToMatAll(data.Value, _recordedHits.Moving, _trajectories.Moving,_currentPlayer);
                    }
                }
                _recordedStages.Clear();
            }
            //if (_recordedHits.Random.Count > 0)
            //{
            //    MatlabDriver.ToMat(_recordedHits.Random, "randomHits");
            //    _recordedHits.Random = new List<Hit>();
            //}
            //if (_recordedHits.Moving.Count > 0)
            //{
            //    MatlabDriver.ToMatMoving(_recordedHits.Moving,_movingTrajectories,"movingHits");
            //    _recordedHits.Moving = new List<Hit>();
            //}
            //TODO: handle hits saving in matlab 
        }
        #endregion
    }
}
