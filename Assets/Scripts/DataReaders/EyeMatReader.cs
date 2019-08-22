using Eyelink.Structs;
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
    private int index = -1;
    private int stateIndex = 1;

    SessionTrigger state = SessionTrigger.TrialStartedTrigger;
    double stateTime;
    private int timeoutIndex = 0;

    public EyeMatReader(string filePath) : base(filePath) {
        file = new EyelinkMatFile(filePath);
        // -1 since matlab is 1 based array, -1 again to act as a null value

        index = (int)(file.trial_index[0, 0] - 2);
        stateTime = GetStateTime(stateIndex);
    }

    //datatype is unused for .mat file as they do not contain thee information
    public override AllFloatData GetCurrentData(DataTypes dataType) {
        return currentData;
    }

    public override AllFloatData GetNextData() {
        index++;

        if (currentData == null) {
            //do not need to decrement because the decrement is done in the constuctor
            currentData = new MessageEvent(file.timestamps[0, index], $"Trial Start {(int)state}", DataTypes.MESSAGEEVENT);
        }
        else if (index >= stateTime) {
            stateIndex++;

            if (stateIndex / 3 >= file.trial_index.GetLength(1)) {
                stateTime = float.MaxValue;
            }
            else {
                stateTime = GetStateTime(stateIndex);
            }

            uint timeStamp = file.timestamps[0, index];

            if (timeStamp == file.timeoutTimes[0, timeoutIndex]) {
                timeoutIndex++;
                Debug.LogError(timeoutIndex);
                state = state.NextTrigger(false);
            }
            else {
                state = state.NextTrigger();
            }

            currentData = new MessageEvent(file.timestamps[0, index], $"Trigger Here {(int)state}", DataTypes.MESSAGEEVENT);

            //undo the increment of index to simulate a message event within the data
            index--;
        }
        else {
            if (index >= file.eyePos.GetLength(1)) {
                currentData = null;
            }
            else {
                float gx = file.eyePos[0, index];
                float gy = file.eyePos[1, index];
                if (float.IsNaN(gx)) {
                    gx = 100_000_000f;
                }

                if (float.IsNaN(gy)) {
                    gy = 100_000_000f;
                }

                currentData = new Fsample(file.timestamps[0, index], gx, gy, DataTypes.SAMPLE_TYPE);
            }
        }

        return currentData;
    }

    private double GetStateTime(int stateIndex) {
        // -1 since matlab is 1 based array
        Debug.Log($"trialindex: {stateIndex % 3}, {stateIndex / 3} from {stateIndex}");
        return file.trial_index[stateIndex % 3, stateIndex / 3] - 1;
    }
}
