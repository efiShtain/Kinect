using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{
    public interface IKinectBusinessLogic
    {
        event Action<bool> KinectAvailabletyChanged;
        //event Action<List<ScreenPoint>,List<KinectPoint>> NewJointsDataReady;
        event Action<DisplayData, List<PointRealWorld>, PointRealWorld> NewJointsDataReady;

        bool StartKinect();
        bool StopKinect();

        float ScaleFactorX { get; set; }
        float ScaleFactorY { get; set; }
        bool IsOpen { get; }
        bool IsNextPointMoving { get; set; }
        bool IsGetNextPoint { get; set; }
        void SetEnemiesList (List<PointRealWorld> enemies);
    }
}
