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
using System.Windows;
using System.Windows.Media.Media3D;

namespace KinectServer.BusinessLogic
{

    public class KinectServerBL
    {
        private IKinectBusinessLogic _kinect;
        private GameLogic _game;
        private WebsocketBL _server;
        private Dictionary<string, List<List<Point3D>>> _kinectSkeletonCoordinatePerStage;
        private Dictionary<string, List<List<ScreenPoint>>> _2dCoordinatePerStage;
        private Dictionary<string, List<ScreenPoint>> _trajectoriesDataIn2D;
        private Dictionary<string, List<Point3D>> _trajectoriesData;
        private Dictionary<string, List<Hit>> _recordedHitsData;
        private bool _waitingForPoint;
        private CustomPointsListParser _pointsParser;
        private string _currentPlayer;
        public KinectServerBL(IKinectBusinessLogic kbl, int port)
        {
            _waitingForPoint = false;
            Initialize(kbl, port);
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

            _kinectSkeletonCoordinatePerStage = new Dictionary<string, List<List<Point3D>>>();
            _2dCoordinatePerStage = new Dictionary<string, List<List<ScreenPoint>>>();
            _recordedHitsData = new Dictionary<string, List<Hit>>();
            _trajectoriesData = new Dictionary<string, List<Point3D>>();
            _trajectoriesDataIn2D = new Dictionary<string, List<ScreenPoint>>();
            _pointsParser = new CustomPointsListParser();
            _server.StartServer();
        }

        #endregion

        #region Public Properties

        #endregion

        #region Events
        public event Action<List<Point3D>> NewKinectDataReady;
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
                _recordedHitsData.Clear();
                _trajectoriesData.Clear();
                _trajectoriesDataIn2D.Clear();
                _kinectSkeletonCoordinatePerStage.Clear();
                _2dCoordinatePerStage.Clear();
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
                    if (instruction.State != "win")
                    {
                        _trajectoriesData.Add(instruction.State, new List<Point3D>());
                        _trajectoriesDataIn2D.Add(instruction.State, new List<ScreenPoint>());
                        _recordedHitsData.Add(instruction.State, new List<Hit>());
                        var enemiesList = _pointsParser.GetPointsForStage(_currentPlayer, instruction.State);
                        _kinect.SetEnemiesList(enemiesList);
                        instruction.EnemyCount = enemiesList.Count;
                    }
                    _server.PostMessage<Instruction>(instruction, Protocol.GET_NEXT_INSTRUCTIONS);
                    if (GameStateChanged != null)
                    {
                        GameStateChanged(instruction.State);
                    }
                    break;
                case Protocol.GET_NEXT_STAR_POSITION:
                    _waitingForPoint = true;
                    _kinect.IsGetNextPoint = true;
                    break;
                case Protocol.HITS_DATA:
                    var hitData = JsonConvert.DeserializeObject<HitData>(message.Data);
                    if (hitData != null)
                    {
                        var hit = _recordedHitsData[_game.CurrentState].LastOrDefault();
                        if (hit != null)
                        {
                            hit.T2 = hitData.skeletonIndex;
                            hit.HitJointIndex = hitData.jointIndex;
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

            _pointsParser.Init();
            _game = new GameLogic(_pointsParser.GetStages());
            _kinect.SetInflationRatios(_pointsParser.GetInnerBoundaryInflationRatios(), _pointsParser.GetOuterBoundaryInflationRatios());
            _game.ResetInstructionsCounter();
            if (SessionStarted != null)
            {
                SessionStarted();
            }
        }

        private void _kinect_NewJointsDataReady(DisplayData screenData, List<Point3D> kinectCoordinatesSkeleton, Point3D enemyObject)
        {
            _server.PostMessage<DisplayData>(screenData, Communication.Protocol.SKELETON_DATA);
            if (_waitingForPoint)
            {
                _recordedHitsData[_game.CurrentState].Add(new Hit() { T1 = screenData.SkeletonIndex });
                _server.PostMessage<ScreenPoint>(screenData.NextEnemyPoint, Communication.Protocol.GET_NEXT_STAR_POSITION);

                _waitingForPoint = false;
            }

            if (_game.CurrentState != "win")
            {
                if (enemyObject != null)
                {
                    _trajectoriesData[_game.CurrentState].Add(enemyObject);                        //This is a real location of a moving object
                    if (screenData.NextEnemyPoint != null)
                    {
                        _trajectoriesDataIn2D[_game.CurrentState].Add(screenData.NextEnemyPoint);
                    }
                    else
                    {
                        _trajectoriesDataIn2D[_game.CurrentState].Add(new ScreenPoint());
                    }
                }
                else
                {
                    _trajectoriesData[_game.CurrentState].Add(new Point3D(0, 0, 0));  //This is padding so indecis will match skeleton indecis
                    var sp = new ScreenPoint();
                    _trajectoriesDataIn2D[_game.CurrentState].Add(sp);
                }
            }

            if (_game.CurrentState != null && _game.CurrentState != "win")
            {
                if (!_kinectSkeletonCoordinatePerStage.ContainsKey(_game.CurrentState))
                {
                    _kinectSkeletonCoordinatePerStage.Add(_game.CurrentState, new List<List<Point3D>>());
                    _2dCoordinatePerStage.Add(_game.CurrentState, new List<List<ScreenPoint>>());
                }
                _kinectSkeletonCoordinatePerStage[_game.CurrentState].Add(kinectCoordinatesSkeleton);
                _2dCoordinatePerStage[_game.CurrentState].Add(screenData.Joints);

            }

            if (NewKinectDataReady != null)
            {
                NewKinectDataReady(kinectCoordinatesSkeleton);
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
            if (_kinectSkeletonCoordinatePerStage.Count != 0)
            {

                foreach (var stage in _kinectSkeletonCoordinatePerStage.Keys)
                {
                    MatlabDriver.ToMatAll(_kinectSkeletonCoordinatePerStage[stage], _recordedHitsData[stage], _trajectoriesData[stage], _2dCoordinatePerStage[stage], _trajectoriesDataIn2D[stage], _currentPlayer);
                }

                //foreach (var data in _kinectSkeletonCoordinatePerStage)
                //{
                //    MatlabDriver.ToMatAll(data.Value, _recordedHitsData[data.Key], _trajectoriesData[data.Key], _currentPlayer);
                //}

            }
        }
        #endregion
    }
}
