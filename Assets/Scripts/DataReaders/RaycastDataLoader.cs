﻿using System;

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RaycastDataLoader : ICsvLineParser<PlaybackData> {

    public static void Load(string path, List<Trial> trials, SpikeTimeParser spikeTrainReader = null) {
        RaycastDataLoader loader = new RaycastDataLoader();
        using (CsvReader<PlaybackData> reader = new CsvReader<PlaybackData>(path, loader)) {

            Trial t = null;
            //initilize spiketrain reader
            int spikeIndexer = 0;

            IList<string> rawData = null, prevRawData;

            decimal initTime = -1;

            while (reader.HasNext) {
                PlaybackData data = reader.GetNextData();
                if (initTime == -1) {
                    initTime = data.timestamp;
                }

                if (data is PlaybackEvent ev && ev.trigger == SessionTrigger.TrialStartedTrigger) {
                    t = new Trial(ev.message);
                    trials.Add(t);
                }
                else if (spikeTrainReader != null && spikeIndexer < spikeTrainReader.Length && data is PlaybackSample sam) {
                    decimal spikeTime = spikeTrainReader[spikeIndexer] + initTime;

                    if (t.GetFrameCount() < 5) {
                        Debug.Log($"{spikeTime}|{sam.timestamp}");
                    }

                    if (spikeTime != default && spikeTime <= sam.timestamp) {
                        sam.HasSpike = true;
                        spikeIndexer++;
                    }
                }

                prevRawData = rawData;
                rawData = reader.GetCurrentRawData();

                if (rawData[RayCastRecorder.EndOfFrame].Contains(RayCastRecorder.EndOfFrameFlag)) {
                    if (data is PlaybackSample sam) {
                        t.NextFrame(GetRobotConfig(rawData));
                    }
                    else {
                        t.NextFrame(GetRobotConfig(prevRawData));
                    }
                }

                t.AddData(data);
            }
        }
    }

    private static RobotConfiguration GetRobotConfig(IList<string> dataArr) {
        if (dataArr != null) {
            if (dataArr[RayCastRecorder.ObjName_Message].Contains("ignored")) {
                return null;
            }

            float posX, posZ, rotY;
            try {

                posX = float.Parse(dataArr[RayCastRecorder.PosX]);
                posZ = float.Parse(dataArr[RayCastRecorder.PosZ]);
                rotY = float.Parse(dataArr[RayCastRecorder.RotY]);

            }
            catch (Exception) {
                foreach (string data in dataArr) {
                    Debug.Log(data);
                }
                throw;
            }
            return new RobotConfiguration(posX, posZ, rotY);
        }
        else {
            return null;
        }
    }

    private static DataTypes StringToType(string s) {
        switch (s) {
            case "MESSAGEEVENT":
                return DataTypes.MESSAGEEVENT;
            case "SAMPLE_TYPE":
                return DataTypes.SAMPLE_TYPE;
            default:
                throw new Exception($"unknown type {s}");
        }
    }

    public void ParseHeader(StreamReader reader) {
        //do nothing because no header to parse.
    }

    public PlaybackData Parse(string[] data) {
        string flags = data[RayCastRecorder.EndOfFrame];
        uint timestamp = uint.Parse(data[RayCastRecorder.Time]);

        if (!string.IsNullOrEmpty(flags) && flags.ContainsNumbers()) {

            string message = flags;
            SessionTrigger trigger = (SessionTrigger)((flags[flags.Length - 2] - '0') * 10);

            return new PlaybackEvent(message, trigger, timestamp);
        }
        try {
            string msg = data[RayCastRecorder.ObjName_Message];
            Vector3 pos = new Vector3(float.Parse(data[RayCastRecorder.PosX]), float.Parse(data[RayCastRecorder.PosY]), float.Parse(data[RayCastRecorder.PosZ]));
            float rotY = float.Parse(data[RayCastRecorder.RotY]);

            if (msg.Contains("Ignored")) {
                return new PlaybackSample(new Vector2(float.NaN, float.NaN), pos, rotY, timestamp);
            }

            Vector2 gaze = new Vector2(float.Parse(data[RayCastRecorder.Gx]), float.Parse(data[RayCastRecorder.Gy]));

            return new PlaybackSample(gaze, pos, rotY, timestamp);
        }
        catch (Exception) {
            Debug.LogError(data[RayCastRecorder.Time]);
            throw;
        }
    }
}
