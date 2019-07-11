using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : IEnumerable<PlaybackData> {
    List<PlaybackData> fixations = new List<PlaybackData>();
    public RobotConfiguration Config { get; set; }

    public int DataCount { get => fixations.Count; }

    public uint startTime { get; private set; }
    private uint endTime;

    private AudioClip spikeTrain = null;

    private const int Sampling_Rate = 48000;
    private const int Samples_Per_Millis = Sampling_Rate / 1000;

    private static float[] tone = null;
    private static float[] negTone = null;


    public Frame() {
        if (tone == null) {
            tone = new float[Samples_Per_Millis];
            negTone = new float[Samples_Per_Millis];
            for (int i = 0; i < Samples_Per_Millis; i++) {
                tone[i] = 1f;
                negTone[i] = -1f;
            }
        }
    }

    public void AddData(PlaybackData playBackData) {
        if (fixations.Count == 0) {
            startTime = playBackData.timestamp;
        }
        fixations.Add(playBackData);

        endTime = playBackData.timestamp;
    }

    public AudioClip GetAudioClip() {
        if (spikeTrain == null) {
            if (fixations.Count != 0) {
                spikeTrain = AudioClip.Create(ToString(), fixations.Count * Samples_Per_Millis, 1, Sampling_Rate, false);
            }

            for (int i = 0; i < fixations.Count; i++) {
                if (fixations[i].HasSpike) {
                    spikeTrain.SetData(tone, Samples_Per_Millis * i);
                }

            }
        }
        return spikeTrain;
    }

    IEnumerator<PlaybackData> IEnumerable<PlaybackData>.GetEnumerator() {
        return ((IEnumerable<PlaybackData>)fixations).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<PlaybackData>)fixations).GetEnumerator();
    }
}
