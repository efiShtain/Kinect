﻿using KinectServer.DTO;
using KinectServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectServer.BusinessLogic
{
    public class CustomPointsListParser
    {
        GameInputFile _gameInputDetails;
        public CustomPointsListParser()
        {
            _gameInputDetails = null;

        }
        public bool Init()
        {
            var enemiesFilePath = ConfigurationManager.AppSettings["enemiesFile"];
            try
            {
                var str = File.ReadAllText(enemiesFilePath);
                _gameInputDetails = JsonConvert.DeserializeObject<GameInputFile>(str);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Json file is malformed");
                return false;
            }
        }

        public Dictionary<string,float> GetInnerBoundaryInflationRatios()
        {
            return _gameInputDetails.InnerBoundaryInflRatio;
        }

        public Dictionary<string, float> GetOuterBoundaryInflationRatios()
        {
            return _gameInputDetails.OuterBoundaryInflRatio;
        }
   

        public List<UserDefinedPoint> GetPointsForStage(string username, string stage)
        {
            if (_gameInputDetails.Players.ContainsKey(username))
            {
                if (_gameInputDetails.Players[username].ContainsKey(stage))
                {
                    
                    var requiredActionsIds =  _gameInputDetails.Players[username][stage];
                    var requiredPoints = new List<UserDefinedPoint>();
                    for (int i = 0; i < requiredActionsIds.Count; i++)
                    {
                        requiredPoints.Add(_gameInputDetails.Actions[requiredActionsIds[i]]);
                    }
                        return requiredPoints;
                }
            }
            return null;
        }

        public List<Instruction> GetStages(string playerName = "")
        {
            var stages = new List<Instruction>();
            var firstPlayerStages = _gameInputDetails.Players.FirstOrDefault().Value;
            if (playerName != string.Empty && _gameInputDetails.Players.ContainsKey(playerName))
            {
                firstPlayerStages = _gameInputDetails.Players[playerName];
            }
            
            if (firstPlayerStages != null)
            {
                foreach(var s in firstPlayerStages.Keys)
                {
                    var inst = new Instruction();
                    inst.State = s;
                    inst.Text = s;
                    inst.EnemyCount = firstPlayerStages[s].Count;
                    stages.Add(inst);
                }               
            }
            return stages;
        }

    }

}
