using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class RayCastRecorder
{
    private const string delimiter = ",";
    private StreamWriter s;

    public RayCastRecorder(string saveLocation) : this(saveLocation, "defaultTest.csv"){
    }

    public RayCastRecorder(string saveLocation, string fileName) {
        s = new StreamWriter(Path.Combine(saveLocation, fileName));
    }

    public void WriteSample(DataTypes type, uint time, string objName, Vector2 centerOffset, Vector3 hitObjLocation, Vector3 pointHitLocation, Vector2 gaze, Vector3 subjectLoc) {
        s.Write($"{type}{delimiter}");
        s.Write($"{time}{delimiter}");
        s.Write($"{objName}{delimiter}");
        s.Write(VectorToString(centerOffset));//1 delimiter
        s.Write(delimiter);
        s.Write(VectorToString(hitObjLocation));//2 delimiter
        s.Write(delimiter);
        s.Write(VectorToString(pointHitLocation)); //2 delimiter
        s.Write(delimiter);
        s.Write(VectorToString(gaze)); //1 delimiter
        s.Write(delimiter);
        s.Write(VectorToString(subjectLoc)); //2 delimiter
        s.WriteLine(); //total 12 delimiter
        s.Flush();
    }

    public void WriteEvent(DataTypes type, uint time, string message) {
        s.Write($"{type}{delimiter}");
        s.Write($"{time}{delimiter}");
        s.Write($"{message}{delimiter}");
        for(int i = 0; i < 15-3; i++) {
            s.Write($"{delimiter}");
        }
        s.WriteLine();
        s.Flush();
    }

    public void Close() {
        s.Flush();
        s.Dispose();
        s.Close();
    }

    public string VectorToString(Vector3 v) {
        if(v == null) {
            return $"{delimiter}{delimiter}";
        }
        return $"{v.x}{delimiter}{v.y}{delimiter}{v.z}";
    }

    public string VectorToString(Vector2 v) {
        if (v == null) {
            return $"{delimiter}";
        }
        return $"{v.x}{delimiter}{v.y}";
    }
}
