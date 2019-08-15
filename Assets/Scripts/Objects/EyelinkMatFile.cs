using sharpHDF.Library.Objects;

public class EyelinkMatFile : Hdf5File {
    public readonly double[,] trial_index;
    public readonly uint[,] timestamps;
    //public readonly double[,] indices;
    public readonly float[,] eyePos;
    public readonly uint[,] timeoutTimes;

    public EyelinkMatFile(string _filename) : base(_filename) {
        Hdf5Group grp = Groups["el"].Groups["data"];
        timeoutTimes = (uint[,])grp.Datasets["timeouts"].GetData();
        trial_index = (double[,])grp.Datasets["trial_timestamps"].GetData();
        //indices = (double[,])grp.Datasets["indices"].GetData();
        eyePos = (float[,])grp.Datasets["eye_pos"].GetData();
        timestamps = (uint[,])grp.Datasets["timestamps"].GetData();
    }
}
