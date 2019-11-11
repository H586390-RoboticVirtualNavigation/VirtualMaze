using Eyelink.Structs;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class EyeMatReader : EyeDataReader {
    /// <summary>
    /// Importing hdf5.dll here so that unity will include it in the build
    /// This function does not exist do not use this
    /// </summary>
    [DllImport("hdf5")]
    private static extern void test();

    [DllImport("hdf5_hl")]
    private static extern void test2();

    private AllFloatData currentData = null;

    private EyelinkMatFile file;
    private int currentTime = -1;
    private int stateIndex = 0;

    private double lastTriggerTime = -1;

    //SessionTrigger state = SessionTrigger.TrialStartedTrigger;
    double stateTime;

    public EyeMatReader(string filePath) {
        file = new EyelinkMatFile(filePath);
        // -1 since matlab is 1 based array, -1 again to act as a null value

        currentTime = (int)GetStateTime(stateIndex);
        stateTime = GetStateTime(stateIndex + 1);

        //additional 1 second for fadeout
        lastTriggerTime = GetStateTime(file.trial_index.GetLength(1) * 3 - 1) + 1000;
    }

    //datatype is unused for .mat file as they do not contain thee information
    public AllFloatData GetCurrentData(DataTypes dataType) {
        return currentData;
    }

    public AllFloatData GetNextData() {
        if (currentData == null) {
            //do not need to decrement because the decrement is done in the constuctor
            currentData = new MessageEvent(file.timestamps[0, currentTime], parseTrialCode(GetStateCode(stateIndex)), DataTypes.MESSAGEEVENT);

            //undo the increment of index to simulate a message event within the data
            currentTime--;
        }
        else {

            currentTime++;

            if (currentTime >= stateTime) {
                stateIndex++;

                if (stateIndex < file.trial_index.GetLength(1) * 3 - 1) {
                    stateTime = GetStateTime(stateIndex + 1);
                }
                else {
                    stateTime = float.MaxValue;
                }

                currentData = new MessageEvent(file.timestamps[0, currentTime], parseTrialCode(GetStateCode(stateIndex)), DataTypes.MESSAGEEVENT);

                //undo the increment of index to simulate a message event within the data
                currentTime--;
            }
            else {
                if (currentTime >= lastTriggerTime) {
                    currentData = new FEvent(1, DataTypes.NO_PENDING_ITEMS);
                }
                else {
                    float gx = file.eyePos[0, currentTime];
                    float gy = file.eyePos[1, currentTime];
                    if (float.IsNaN(gx)) {
                        gx = 100_000_000f;
                    }

                    if (float.IsNaN(gy)) {
                        gy = 100_000_000f;
                    }

                    currentData = new Fsample(file.timestamps[0, currentTime], gx, gy, DataTypes.SAMPLE_TYPE);
                }
            }
        }
        return currentData;
    }

    private double GetStateTime(int stateIndex) {
        // -1 since matlab is 1 based array
        Debug.Log($"trialindex: {stateIndex % 3}, {stateIndex / 3} from {stateIndex}");
        return file.trial_index[stateIndex % 3, stateIndex / 3] - 1;
    }

    private int GetStateCode(int stateIndex) {
        return file.trial_codes[stateIndex % 3, stateIndex / 3];
    }

    private string parseTrialCode(int code) {
        Debug.Log(code);
        SessionTrigger trigger = (SessionTrigger)(code - (code % 10));
        switch (trigger) {
            case SessionTrigger.CueOffsetTrigger:
                return $"Cue Offset {code}";
            case SessionTrigger.TrialStartedTrigger:
                return $"Start Trial {code}";
            case SessionTrigger.TrialEndedTrigger:
                return $"End Trial {code}";
            case SessionTrigger.TimeoutTrigger:
                return $"Timeout {code}";
            case SessionTrigger.ExperimentVersionTrigger:
                return $"Trigger Version {(int)trigger + GameController.versionNum}";
            default:
                throw new NotSupportedException($"EyeMatReader::Unknown code {code}");
        }
    }

    public void Dispose() {
        file.Dispose();
    }
}
