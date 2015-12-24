using KinectServer.DTO;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace KinectServer.BusinessLogic
{
    public class KinectBL : IKinectBusinessLogic
    {
        private KinectSensor _sensor;
        private Body[] _bodies;
        private BodyFrameReader _bodyFrameReader;
        private CoordinateMapper _mapper;
        private int _skeletonIndex;
        private int _randomSeed;
        private Random _randomPoint;
        private Random _randomDirection;
        private ScreenPoint _nextEnemyPoint;
        private PointRealWorld _nextEnemyPointInRealWorld;
        private List<PointRealWorld> _predefinedEnemiesList;
        private int _enemyCounter;
        public KinectBL()
        {
            SetKinect();
            _skeletonIndex = 1;
            IsGetNextPoint = false;
            _predefinedEnemiesList=null;
            _predefinedEnemiesList = null;
            _enemyCounter = 0;
        }

        private void SetKinect()
        {
            ScaleFactorX = (float)(1600.0 / 512.0);
            ScaleFactorY = (float)(1000.0 / 424.0);
            _sensor = KinectSensor.GetDefault();
            _mapper = _sensor.CoordinateMapper;

            _sensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            _bodies = null;
            _bodyFrameReader = _sensor.BodyFrameSource.OpenReader();
            _bodyFrameReader.FrameArrived += _bodyFrameReader_FrameArrived;

        }


        RectF inflateRect(RectF f, float xInfRation, float yInfRatio)
        {
            RectF rect = new RectF();
            rect.X = f.X - f.Width * xInfRation;
            rect.Y = f.Y + f.Height * yInfRatio;
            rect.Height = f.Height;
            rect.Width = f.Width * (1+2*xInfRation);
            return rect;
        }

        private void _bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataRecived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (_bodies == null)
                    {
                        _bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    dataRecived = true;
                }
            }
            if (dataRecived)
            {
                foreach (Body body in _bodies)
                {
                    if (body.IsTracked)
                    {
                        IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                        var screenPoints = new List<ScreenPoint>();
                        var kinectPoints = new List<PointRealWorld>();
                        float zPlane = 0;
                        //Save points from current body joints
                        foreach (JointType jointType in joints.Keys)
                        {

                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            kinectPoints.Add(new PointRealWorld(position.X, position.Y, position.Z));
                            
                            DepthSpacePoint depthPoint = _mapper.MapCameraPointToDepthSpace(position);
                            ScreenPoint p = new ScreenPoint()
                            {
                                X = (int)(depthPoint.X * ScaleFactorX),
                                Y = (int)(depthPoint.Y * ScaleFactorY),

                            };

                            screenPoints.Add(p);
                        }

                        if (NewJointsDataReady != null)
                        {
                            var displayData = new DisplayData();
                            if (IsGetNextPoint)
                            {
                                _nextEnemyPoint = new ScreenPoint();
                                _nextEnemyPointInRealWorld = new PointRealWorld();
                                /*
                                  *Get bounding rectangle coordinates in camera space 
                                  *               TopY=Head
                                  *                 _____
                                  *                 | 0  |
                                  *                 |/|\ |
                                  *   MinX=Left hand| |  | MaxX=Right Hand
                                  *                 |/ \ |
                                  *                 |___\|
                                  * BottomY= Min(Left Foot, Right Foot)
                                  * 
                                 */
                                PointRealWorld _enemy = new PointRealWorld();

                                var leftHand = body.Joints[JointType.HandLeft].Position;
                                var rightHand =body.Joints[JointType.HandRight].Position;
                                var head = body.Joints[JointType.Head].Position;
                                var leftFoot = body.Joints[JointType.FootLeft].Position;
                                var rightFoot = body.Joints[JointType.FootRight].Position;
                                zPlane = body.Joints[JointType.SpineBase].Position.Z ;

                                var innerInflationX = 0.05;
                                var innerInflationY = 0.05;
                                var outerInflationX = 0.2;
                                var outerInflationY = 0.2;
                                

                                var topY = head.Y;
                                var bottomY = Math.Min(leftFoot.Y, rightFoot.Y);
                                bool isRightHandOnRightSide = rightHand.X > leftHand.X;
                                var minX = Math.Min(leftHand.X, rightHand.X);
                                var maxX = Math.Max(leftHand.X, rightHand.X);

                                RectF skeletonBoundryRect = new RectF();
                                skeletonBoundryRect.X = (float)minX;
                                skeletonBoundryRect.Y = (float)topY;
                                skeletonBoundryRect.Width = maxX - minX;
                                skeletonBoundryRect.Height = topY - bottomY;




                                var nextObjectX = _randomPoint.NextDouble() * (maxX - minX) + minX;
                                var nextObjectY = _randomPoint.NextDouble() * (topY - bottomY) + bottomY;

                                if (_predefinedEnemiesList != null)
                                {
                                    nextObjectX = _predefinedEnemiesList[_enemyCounter].X*(maxX-minX)+minX;
                                    nextObjectY = _predefinedEnemiesList[_enemyCounter].Y*(topY - bottomY) + bottomY;
                                }

                                

                                CameraSpacePoint enemy = new CameraSpacePoint();
                                /*
                                 * Check if nextObjectX is on on the right side or left side of the body
                                 * If it is on right side, calculate the right bound (right hand + inflation) 
                                 * for now it is xInflation exactly due to the addition
                                 * if it is on the left, calculate the left bound in the same way
                                 * check scenario where right hand and left hand are crossed by isRightHandOnRightSide flag
                                 */
                                
                                if (nextObjectX > ((maxX + minX) / 2))    //minX------(Max+Min)/2-----MaxX
                                {
                                    
                                    if (isRightHandOnRightSide)
                                    {
                                        var rightBound = Math.Abs(maxX - rightHand.X);
                                        enemy.X = (float)(maxX - Math.Abs((nextObjectX % rightBound)));
                                    }
                                    else
                                    {
                                        var rightBound = Math.Abs(maxX - leftHand.X);
                                        enemy.X = (float)(maxX - Math.Abs((nextObjectX % rightBound)));
                                    }

                                }
                                else
                                {
                                    
                                    if (isRightHandOnRightSide)
                                    {
                                        var leftBound = Math.Abs(minX - leftHand.X);
                                        enemy.X = (float)(minX + Math.Abs((nextObjectX % leftBound)));
                                    }
                                    else
                                    {
                                        var leftBound = Math.Abs(minX - rightHand.X);
                                        enemy.X = (float)(minX + Math.Abs((nextObjectX % leftBound)));
                                    }
                                }

                                enemy.Y = (float)nextObjectY;
                                enemy.Z = zPlane;

                                var mappedEnemy = _mapper.MapCameraPointToDepthSpace(enemy);
                                _nextEnemyPoint.X = (int)(mappedEnemy.X * ScaleFactorX);
                                _nextEnemyPoint.Y = (int)(mappedEnemy.Y * ScaleFactorY);

                                _nextEnemyPointInRealWorld.X = enemy.X;
                                _nextEnemyPointInRealWorld.Y = enemy.Y;
                                _nextEnemyPointInRealWorld.Z = enemy.Z;

                                if (IsNextPointMoving)
                                {   
                                    //_nextEnemyPoint.floatXDelta = (float)(_randomDirection.NextDouble() * 0.02 - 0.01); //get random number between -0.01 and 0.01;
                                    //_nextEnemyPoint.floatYDelta = (float)(_randomDirection.NextDouble() * 0.02 - 0.01); //get random number between -0.01 and 0.01;
                                    if (_predefinedEnemiesList != null)
                                    {
                                        _nextEnemyPointInRealWorld.XDelta = (float)(_predefinedEnemiesList[_enemyCounter].XDelta*0.02-0.01);
                                        _nextEnemyPointInRealWorld.YDelta = (float)(_predefinedEnemiesList[_enemyCounter].YDelta*0.02-0.01);
                                    }
                                    else
                                    {
                                        _nextEnemyPointInRealWorld.XDelta = (float)(_randomDirection.NextDouble() * 0.02 - 0.01); //get random number between -0.01 and 0.01;
                                        _nextEnemyPointInRealWorld.YDelta = (float)(_randomDirection.NextDouble() * 0.02 - 0.01); //get random number between -0.01 and 0.01;
                                    }
                                    
                                }
                                IsGetNextPoint = false;
                                displayData.NextEnemyPoint = _nextEnemyPoint;
                                _enemyCounter++;
                            }
                            else
                            {
                                if (IsNextPointMoving)
                                {
                                    CameraSpacePoint enemy = new CameraSpacePoint();
                                    enemy.X = _nextEnemyPointInRealWorld.X + _nextEnemyPointInRealWorld.XDelta;
                                    enemy.Y = _nextEnemyPointInRealWorld.Y + _nextEnemyPointInRealWorld.YDelta;
                                    enemy.Z = _nextEnemyPointInRealWorld.Z;
                                    
                                    var nextEnemyPoint = new PointRealWorld(enemy.X, enemy.Y, enemy.Z);
                                    nextEnemyPoint.XDelta = _nextEnemyPointInRealWorld.XDelta;
                                    nextEnemyPoint.YDelta = _nextEnemyPointInRealWorld.YDelta;
                                    _nextEnemyPointInRealWorld = nextEnemyPoint;
                                    
                                    var mappedEnemy = _mapper.MapCameraPointToDepthSpace(enemy);

                                    displayData.NextEnemyPoint = new ScreenPoint();
                                    displayData.NextEnemyPoint.X = (int)(mappedEnemy.X * ScaleFactorX);
                                    displayData.NextEnemyPoint.Y = (int)(mappedEnemy.Y * ScaleFactorY);
                                }
                                else
                                {
                                    _nextEnemyPoint = null;
                                    //_nextEnemyPointInRealWorld = null;
                                    displayData.NextEnemyPoint = null;
                                }
                            }
                            displayData.Joints = screenPoints;
                            displayData.SkeletonIndex = _skeletonIndex++;

                            NewJointsDataReady(displayData, kinectPoints, _nextEnemyPointInRealWorld);
                        }

                    }
                }
            }
        }

        

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (KinectAvailabletyChanged != null)
            {
                KinectAvailabletyChanged(_sensor.IsAvailable);
            }
        }

        public float ScaleFactorX { get; set; }
        public float ScaleFactorY { get; set; }
        public bool IsOpen { get { return _sensor.IsOpen; } }
        public bool IsNextPointMoving { get; set; }
        public bool IsGetNextPoint { get; set; }

        public event Action<bool> KinectAvailabletyChanged;
        /// <summary>
        /// DisplayData - Screen points of body in pixels, next enemy location in pixels, skeleton index
        /// List<PointRealWorld>Body skeleton coordinates in camera space</PointRealWorld>
        /// PointRealWorld  - Next enemy point in camera space
        /// </summary>
        public event Action<DisplayData, List<PointRealWorld>, PointRealWorld> NewJointsDataReady;

        public bool StartKinect()
        {
            if (!_sensor.IsOpen)
            {
                _randomSeed = 2;
                _randomPoint = new Random(_randomSeed);
                _randomDirection = new Random(_randomSeed + 2);
                _sensor.Open();
                _skeletonIndex = 1;
                _predefinedEnemiesList = null;
                _enemyCounter = 0;
                return true;
            }
            return false;
        }

        public bool StopKinect()
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
                return true;
            }
            return false;
        }


        public void SetEnemiesList(List<PointRealWorld> enemies)
        {
            _predefinedEnemiesList = enemies;
            
        }
    }
}
