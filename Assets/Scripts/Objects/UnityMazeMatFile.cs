public class UnityMazeMatFile : AbstractHDF5File {
    public readonly double[,] unityData;

    // Row is trial, Column is event (Start, CueOffset, End), 1 based
    public readonly double[,] unityTriggersIndex;

    public UnityMazeMatFile(string _filename) : base(_filename) {
        using (HDF5Group data = HDFHelper.GetMyDataGroup(this)) {
            unityData = HDFHelper.GetDataMatrix<double>(data, "unityData");
            unityTriggersIndex = HDFHelper.GetDataMatrix<double>(data, "unityTriggers");
        }
    }
}
