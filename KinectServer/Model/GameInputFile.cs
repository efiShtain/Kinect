using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Model
{
    public class GameInputFile
    {
        public Dictionary<string, float> InnerBoundryInflRatio { get; set; }
        public Dictionary<string, float> OuterBoundryInflRatio { get; set; }
        public Dictionary<string, Dictionary<string, List<UserDefinedPoint>>> Players { get; set; }
    }
}
