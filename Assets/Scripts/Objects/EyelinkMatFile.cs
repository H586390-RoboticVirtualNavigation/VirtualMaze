using System;
using UnityEngine;
using sharpHDF.Library.Objects;

public class EyelinkMatFile : Hdf5File {
    public readonly double[,] trial_index;
    public readonly int[,] trial_codes;
    public readonly uint[,] timestamps;
    //public readonly double[,] indices;
    public readonly float[,] eyePos;

    public EyelinkMatFile(string _filename) : base(_filename) {
        Hdf5Group grp = Groups["el"].Groups["data"];
        trial_index = (double[,])grp.Datasets["trial_timestamps"].GetData();
        //indices = (double[,])grp.Datasets["indices"].GetData();
        eyePos = (float[,])grp.Datasets["eye_pos"].GetData();
        timestamps = (uint[,])grp.Datasets["timestamps"].GetData();

        Array a = grp.Datasets["trial_codes"].GetData();

        Debug.Log(a);

        trial_codes = (int[,])grp.Datasets["trial_codes"].GetData();
    }
}
