using System;
using System.Collections.Generic;

public class Trial {
    private List<Frame> frames = new List<Frame>();
    public Dictionary<int, SessionTrigger> triggerMap = new Dictionary<int, SessionTrigger>();

    private Frame currentFrame = null;
    public int RewardIndex { get; private set; }
    private string _trialName;

    public string TrialName {
        get => _trialName;
        set {
            _trialName = value;
            RewardIndex = (_trialName[_trialName.Length - 1] - '0') - 1;
        }
    }

    public Trial(string trialName) {
        TrialName = trialName;
    }

    public void AddData(PlaybackData playBackData) {
        if (currentFrame == null) {
            NextFrame(null);
        }
        //event data not added into trial as they have the same time as the sample before or after it
        if (playBackData is PlaybackEvent ev) {
            //0 based indexing
            triggerMap.Add(frames.Count - 1, ev.trigger);
            currentFrame.AddData(ev);
        }
        else {
            currentFrame.AddData(playBackData);
        }
    }

    public void NextFrame(RobotConfiguration config = null) {
        if (currentFrame != null) {
            currentFrame.Config = config;
        }

        currentFrame = new Frame();
        frames.Add(currentFrame);
    }

    public Frame GetFrameAt(int i) {
        //clamp values
        int index = Math.Min(frames.Count - 1, i);
        index = Math.Max(i, 0);

        return frames[index];
    }

    public int GetFrameCount() {
        return frames.Count;
    }

    public int GetFrameNumAtTrigger(SessionTrigger trigger) {
        foreach (int triggerIndex in triggerMap.Keys) {
            triggerMap.TryGetValue(triggerIndex, out SessionTrigger sTrigger);
            if (sTrigger == trigger) {
                return triggerIndex;
            }
        }
        return -1;
    }

    public SessionTrigger GetLatestTriggerAtFrame(int frameNum) {
        int latestIndex = -1;
        foreach (int triggerIndex in triggerMap.Keys) {
            if (frameNum >= triggerIndex) {
                latestIndex = Math.Max(latestIndex, triggerIndex);
            }
        }

        if (triggerMap.TryGetValue(latestIndex, out SessionTrigger value)) {
            return value;
        }
        else {
            return SessionTrigger.NoTrigger;
        }
    }

    public uint GetDurationOf(int frameIndex) {
        Frame f = frames[frameIndex];

        return f.endTime - frames[0].startTime;
    }
}
