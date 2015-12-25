using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.Communication
{
    public enum Protocol
    {
        STATES = 1,
        KINECT_START = 10,
        KINECT_STOP = 11,
        KINECT_CHANGED_AVAILABILITY = 12,

        GET_NEXT_INSTRUCTIONS = 100,
        GET_NEXT_STAR_POSITION = 101,
        
        SKELETON_DATA = 200,
        HITS_DATA = 201,

        GAME_DONE=301,

        PLAYER_NAME=400
    }
}
