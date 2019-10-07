using sharpHDF.Library.Objects;

public class UnityMazeMatFile : Hdf5File {
    public readonly double[,] unityData;

    // Row is trial, Column is event (Start, CueOffset, End), 1 based
    public readonly double[,] unityTriggersIndex;


    public UnityMazeMatFile(string _filename) : base(_filename) {
        //base constructor always run first

        Hdf5Group dataGroup = null;
        foreach (Hdf5Group a in Groups) {
            if (a.Name.Equals("um") || a.Name.Equals("uf")) {
                dataGroup = Groups[a.Name];
                break;
            }
        }

        if (dataGroup == null) {
            return;
        }

        dataGroup = dataGroup.Groups["data"];

        unityData = (double[,])dataGroup.Datasets["unityData"].GetData();
        unityTriggersIndex = (double[,])dataGroup.Datasets["unityTriggers"].GetData();
    }
}
