using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class BatchModeLogger : IDisposable {
    StreamWriter writer;

    public BatchModeLogger(string location) {
        writer = new StreamWriter($"{location}{Path.DirectorySeparatorChar}VirtualMazeBatchLog.txt");

        writer.WriteLine($"The screen resolution is {Screen.width}x{Screen.height}.");
        writer.WriteLine($"Add \"-screen-height 1080 -screen-width 1920\" if data is incorrect due to resolution.");
        writer.WriteLine($"Add \"-logfile <log file location>.txt\" to see unity logs during the data generation");
        writer.WriteLine($"There may be a need to copy the libraries found in the directory 'Plugins' to a new folder called 'Mono'");
        writer.WriteLine($"\nMore command line arguments can be found at https://docs.unity3d.com/Manual/CommandLineArguments.html");
        writer.Flush();
    }

    public void Dispose() {
        writer?.Dispose();
    }

    internal void Print(string msg) {
        writer.WriteLine(msg);
        writer.Flush();
    }
}
