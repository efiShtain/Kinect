﻿using csmatio.io;
using csmatio.types;
using KinectServer.Common;
using KinectServer.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectServer.BusinessLogic
{
    public static class MatlabDriver
    {
        private const int _jointsTimesCoordinate = 25*3;
        private static string _folder;

        public static void SetFolder()
        {
            var path = @"c:\MatlabResults\" + DateTime.Now.ToString("dd-MM-yyyy h_mm_ss");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _folder = path;
        }
        public static event Action<string> WritingActionStateChanged;
        public static void ToMat(List<List<PointRealWorld>> skeleton, string name = null)
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("Skeletons", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                sequenceStruct["Skeletons"] = jointsMat;
                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }


        public static void ToMatRandom(List<List<PointRealWorld>> skeleton, List<Hit> hits, string name = null)
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("RandomGame", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                sequenceStruct["Skeletons"] = jointsMat;
                if (hits.Count > 0)
                {
                    MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                    int hitsCounter = 0;
                    foreach (var hit in hits)
                    {
                        hitsMat.Set(hit.T1, 0, hitsCounter);
                        hitsMat.Set(hit.T2, 1, hitsCounter);
                        hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                    }
                    sequenceStruct["hits"] = hitsMat;
                }
                

                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }








        public static void ToMat(List<Hit> hits, string name = null)
        {
            Task.Factory.StartNew(() =>
            {
               
                MLStructure sequenceStruct = new MLStructure("hits", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                int hitsCounter = 0;
                foreach (var hit in hits)
                {
                    hitsMat.Set(hit.T1,0, hitsCounter);
                    hitsMat.Set(hit.T2,1, hitsCounter);
                    hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                }

                sequenceStruct["hits"] = hitsMat;
                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "hitsData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }


                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });


        }


        public static void ToMatAll(List<List<PointRealWorld>> skeleton, List<Hit> hits, List<PointRealWorld> trajectories, string name = "")
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("Game", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", name);

                MLSingle jointsMat = new MLSingle("", new int[] { _jointsTimesCoordinate, skeleton.Count });
                int frameCount = 0;
                foreach (var frame in skeleton)
                {
                    int jointCount = 0;
                    foreach (var point in frame)
                    {
                        jointsMat.Set(point.X, jointCount * 3, frameCount);
                        jointsMat.Set(point.Y, jointCount * 3 + 1, frameCount);
                        jointsMat.Set(point.Z, jointCount * 3 + 2, frameCount);
                        jointCount++;
                    }
                    frameCount++;
                }
                sequenceStruct["Skeletons"] = jointsMat;

                if (hits.Count > 0)
                {
                    int maxTraj = 0;

                    MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                    int hitsCounter = 0;
                    foreach (var hit in hits)
                    {
                        hitsMat.Set(hit.T1, 0, hitsCounter);
                        hitsMat.Set(hit.T2, 1, hitsCounter);
                        hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                    }

                    sequenceStruct["hits"] = hitsMat;
                }

                if (trajectories.Count > 0)
                {
                    MLSingle trajMat = new MLSingle("", new int[] { 3, trajectories.Count });
                    int trajCounter = 0;
                    foreach (var t in trajectories)
                    {
                        trajMat.Set(t.X, 0, trajCounter);
                        trajMat.Set(t.Y, 1, trajCounter);
                        trajMat.Set(t.Z, 2, trajCounter++);
                    }

                    sequenceStruct["trajectories"] = trajMat;
                }

                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);

                string filename = "skeletonData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }

                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });
        }

        public static void ToMatMoving(List<Hit> hits,TrajectoriesData trajectories, string name = null)
        {
            Task.Factory.StartNew(() =>
            {

                MLStructure sequenceStruct = new MLStructure("hits", new int[] { 1, 1 });
                sequenceStruct["UserName", 0] = new MLChar("", "igork");

                int maxTraj = 0;

                MLSingle hitsMat = new MLSingle("", new int[] { 3, hits.Count });
                int hitsCounter = 0;
                foreach (var hit in hits)
                {
                    hitsMat.Set(hit.T1, 0, hitsCounter);
                    hitsMat.Set(hit.T2, 1, hitsCounter);
                    hitsMat.Set(hit.HitJointIndex, 2, hitsCounter++);
                }

                sequenceStruct["hits"] = hitsMat;

                MLStructure trajectoryStruct = new MLStructure("hits", new int[] { 1, 1 });
                trajectoryStruct["UserName", 0] = new MLChar("", "igork");

                
                MLSingle trajMat = new MLSingle("", new int[] { 3, trajectories.Moving.Count });
                int trajCounter = 0;
                foreach (var t in trajectories.Moving)
                {
                    trajMat.Set(t.X, 0, trajCounter);
                    trajMat.Set(t.Y, 1, trajCounter);
                    trajMat.Set(t.Z, 2, trajCounter++);
                }

                sequenceStruct["trajectories"] = trajMat;



                List<MLArray> matData = new List<MLArray>();
                matData.Add(sequenceStruct);
                
                string filename = "hitsData_" + DateTime.Now.Ticks + ".mat";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = name + "_" + DateTime.Now.Ticks + ".mat";
                }
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Started writing file: " + filename);
                }


                MatFileWriter mfw = new MatFileWriter(Path.Combine(_folder, filename), matData, false);
                if (WritingActionStateChanged != null)
                {
                    WritingActionStateChanged("Done writing file: " + filename);
                }


            });


        }
    }
}