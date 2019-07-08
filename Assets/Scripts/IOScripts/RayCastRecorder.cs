using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class RayCastRecorder {
    //index reference
    public const int Type = 0;
    public const int Time = 1;
    public const int ObjName_Message = 2;
    public const int Gx = 3;
    public const int Gy = 4;
    public const int PosX = 5;
    public const int PosY = 6;
    public const int PosZ = 7;
    public const int RotY = 8;
    public const int GazeWorldX = 9;
    public const int GazeWorldY = 10;
    public const int GazeWorldZ = 11;
    public const int GazeObjLocX = 12;
    public const int GazeObjLocY = 13;
    public const int GazeObjLocZ = 14;
    public const int X2d = 15;
    public const int Y2d = 16;
    public const int EndOfFrame = 17;

    public const string EndOfFrameFlag = "F";

    public const char delimiter = ',';
    private StreamWriter s;

    public RayCastRecorder(string saveLocation) : this(saveLocation, "defaultTest.csv") {
    }

    public RayCastRecorder(string saveLocation, string fileName) {
        s = new StreamWriter(Path.Combine(saveLocation, fileName));
    }

    public void WriteSample(DataTypes type,
                            uint time,
                            string objName,
                            Vector2 centerOffset,
                            Vector3 hitObjLocation,
                            Vector3 pointHitLocation,
                            Vector2 rawGaze,
                            Vector3 subjectLoc,
                            float subjectRotation,
                            bool isLastSampleInFrame) {
        s.Write($"{type}{delimiter}");
        s.Write($"{time}{delimiter}");
        s.Write($"{objName}{delimiter}");
        s.Write(Vector2ToString(rawGaze)); //1 delimiter
        s.Write(delimiter);
        s.Write(Vector3ToString(subjectLoc)); //2 delimiters
        s.Write(delimiter);
        s.Write(subjectRotation);
        s.Write(delimiter);
        s.Write(Vector3ToString(pointHitLocation)); //2 delimiters
        s.Write(delimiter);
        s.Write(Vector3ToString(hitObjLocation));//2 delimiters
        s.Write(delimiter);
        s.Write(Vector2ToString(centerOffset));//1 delimiter

        s.Write(delimiter);
        if (isLastSampleInFrame) {
            s.Write(EndOfFrameFlag);
        }
        s.WriteLine(); //total 17 delimiters
        s.Flush();
    }

    public void IgnoreEvent(DataTypes type, uint time, Vector2 gazePos, bool isLastSampleInFrame) {
        if (gazePos != null) {
            IgnoreEvent(type, time, $"(x:{gazePos.x} y:{gazePos.y})", isLastSampleInFrame);
        }
        else {
            IgnoreEvent(type, time, "Null", isLastSampleInFrame);
        }
    }

    public void IgnoreEvent(DataTypes type, uint time, string message, bool isLastSampleInFrame) {
        WriteEvent(type, time, $"Data ignored {message}", isLastSampleInFrame);
    }

    public void WriteEvent(DataTypes type, uint time, string message, bool isLastSampleInFrame) {
        if (message.Contains(delimiter.ToString())) {
            throw new InvalidDataException("Message contains delimiter");
        }

        s.Write($"{type}{delimiter}");
        s.Write($"{time}{delimiter}");
        s.Write($"{message}{delimiter}");
        //for CSV to be parsed properly, empty columns are needed.
        for (int i = 0; i < 17 - 3; i++) {
            s.Write($"{delimiter}");
        }

        if (isLastSampleInFrame) {
            s.Write(EndOfFrameFlag);
        }

        s.WriteLine();
        s.Flush();
    }

    public void Close() {
        s.Flush();
        s.Dispose();
        s.Close();
    }

    public string Vector3ToString(Vector3 v) {
        if (v == null) {
            return $"{delimiter}{delimiter}";
        }
        return $"{v.x}{delimiter}{v.y}{delimiter}{v.z}";
    }

    public string Vector2ToString(Vector2 v) {
        if (v == null) {
            return $"{delimiter}";
        }
        return $"{v.x}{delimiter}{v.y}";
    }
}
