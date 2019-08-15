using sharpHDF.Library.Objects;

public class UnityMazeMatFile : Hdf5File {
    public readonly double[,] unityData;

    // Row is trial, Column is event (Start, CueOffset, End), 1 based
    public readonly double[,] unityTriggersIndex;


    public UnityMazeMatFile(string _filename) : base(_filename) {
        //base constructor always run first
        Hdf5Group dataGroup = Groups["um"].Groups["data"];

        unityData = (double[,])dataGroup.Datasets["unityData"].GetData();
        unityTriggersIndex = (double[,])dataGroup.Datasets["unityTriggers"].GetData();
    }
}
