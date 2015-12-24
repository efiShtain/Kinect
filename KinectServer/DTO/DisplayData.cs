using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.DTO
{
    public class DisplayData
    {
        public List<ScreenPoint> Joints { get; set; }
        //public SkeletonCoordinateSystem SCS { get; set; }
        public int SkeletonIndex { get; set; }
        public ScreenPoint NextEnemyPoint { get; set; }

        
        
    }
}
