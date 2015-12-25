using KinectServer.DTO;
using KinectServer.Model;
using Microsoft.Kinect;
using Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
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

        private List<UserDefinedPoint> _predefinedEnemiesList;
        private float _innerRectInflationRatioX = 1.0f;
        private float _innerRectInflationRatioY = 1.0f;
        private float _outterRectInflationRatioX = 1.0f;
        private float _outterRectInflationRatioY = 1.0f;
        private int _enemyCounter;
        private Moveable _currentEnemyObject;

        public KinectBL()
        {
            SetKinect();
            _skeletonIndex = 1;
            IsGetNextPoint = false;
            _predefinedEnemiesList = null;
            _enemyCounter = 0;
            _currentEnemyObject = null;
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


        private DepthSpacePoint ConverEnemyLocationToDepthSpace(Point3D enemyLocation)
        {
            CameraSpacePoint enemy = new CameraSpacePoint();
            enemy.X = (float)enemyLocation.X;
            enemy.Y = (float)enemyLocation.Y;
            enemy.Z = (float)enemyLocation.Z;

            return _mapper.MapCameraPointToDepthSpace(enemy);

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
                        var kinectPoints = new List<Point3D>();
                        float zPlane = 0;
                        //Save points from current body joints
                        foreach (JointType jointType in joints.Keys)
                        {

                            CameraSpacePoint position = joints[jointType].Position;
                            if (position.Z < 0)
                            {
                                position.Z = 0.1f;
                            }
                            kinectPoints.Add(new Point3D(position.X, position.Y, position.Z));

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
                                
                                var leftHand = body.Joints[JointType.HandLeft].Position;
                                var rightHand = body.Joints[JointType.HandRight].Position;
                                var head = body.Joints[JointType.Head].Position;
                                var leftFoot = body.Joints[JointType.FootLeft].Position;
                                var rightFoot = body.Joints[JointType.FootRight].Position;
                                zPlane = body.Joints[JointType.SpineBase].Position.Z;

                                var topY = head.Y;
                                var bottomY = Math.Min(leftFoot.Y, rightFoot.Y);
                                var minX = Math.Min(leftHand.X, rightHand.X);
                                var maxX = Math.Max(leftHand.X, rightHand.X);

                                BoundingRect bodyRect = new BoundingRect(minX, bottomY, zPlane, maxX - minX, topY - bottomY, 1.0f);
                                var innerRect = bodyRect.Inflate(_innerRectInflationRatioX, _innerRectInflationRatioY, 1.0f);
                                var outterRect = bodyRect.Inflate(_outterRectInflationRatioX, _outterRectInflationRatioY, 1.0f);
                                var slices = Slicer.SliceRect(innerRect, outterRect, 3);

                                var nextDefinedPoint = _predefinedEnemiesList[_enemyCounter];
                                var slicePoint = slices[nextDefinedPoint.SliceId].ConvertPoint(nextDefinedPoint.X, nextDefinedPoint.Y, nextDefinedPoint.Z);
                                _currentEnemyObject = new Moveable(
                                    (float)slicePoint.X, (float)slicePoint.Y, (float)slicePoint.Z,
                                    nextDefinedPoint.V0X, nextDefinedPoint.V0Y, nextDefinedPoint.V0Z,
                                    nextDefinedPoint.AX, nextDefinedPoint.AY, nextDefinedPoint.AZ);

                                _currentEnemyObject.Init(); //Start time of object for trajectory calculations, t0
                                IsGetNextPoint = false;
                                _enemyCounter++;
                            }
                           

                            var mappedEnemy = ConverEnemyLocationToDepthSpace(_currentEnemyObject.GetNextPosition());
                            displayData.NextEnemyPoint = new ScreenPoint();
                            displayData.NextEnemyPoint.X = (int)(mappedEnemy.X * ScaleFactorX);
                            displayData.NextEnemyPoint.Y = (int)(mappedEnemy.Y * ScaleFactorY);
                            
                            displayData.Joints = screenPoints;
                            displayData.SkeletonIndex = _skeletonIndex++;

                            NewJointsDataReady(displayData, kinectPoints, _currentEnemyObject.GetLastPosition());
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
        public event Action<DisplayData, List<Point3D>, Point3D> NewJointsDataReady;

        public bool StartKinect()
        {
            if (!_sensor.IsOpen)
            {
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


        public void SetEnemiesList(List<UserDefinedPoint> enemies)
        {
            _predefinedEnemiesList = enemies;

        }

        public void SetInflationRatios(Dictionary<string, float> inner, Dictionary<string, float> outter)
        {
            _innerRectInflationRatioX = inner["RatioX"];
            _innerRectInflationRatioY = inner["RatioY"];
            _outterRectInflationRatioX = inner["RatioX"];
            _outterRectInflationRatioY = inner["RatioY"];
        }
    }
}
