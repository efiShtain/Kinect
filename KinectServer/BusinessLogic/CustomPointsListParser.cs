using KinectServer.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{
    public class CustomPointsListParser
    {
        Dictionary<string, Dictionary<string, List<PointRealWorld>>> _userEnemies;
        public CustomPointsListParser()
        {
            _userEnemies = null;
            
        }
        public bool Init()
        {
            var enemiesFilePath = ConfigurationManager.AppSettings["enemiesFile"];
            try
            {
                var str = File.ReadAllText(enemiesFilePath);
                _userEnemies = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<PointRealWorld>>>>(str);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public List<PointRealWorld> GetPointsForStage(string username, string stage)
        {
            if (_userEnemies.ContainsKey(username))
            {
                if (_userEnemies[username].ContainsKey(stage))
                {
                    return _userEnemies[username][stage];
                }
            }
            return null;
        }
        
    }
}
