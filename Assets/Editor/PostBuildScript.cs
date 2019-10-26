using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;

public class PostBuildScript
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject) {
        switch (target) {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                CopyLibs(Path.GetDirectoryName(pathToBuiltProject));
                break;
            
            default:
                break;
        }
        Debug.Log(pathToBuiltProject);
    }

    private static void CopyLibs(string pathToBuiltProject) {
        string dataPath = Path.Combine(pathToBuiltProject,"VirtualMaze_Data");
        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string MonoPath = Path.Combine(dataPath, "Mono");
        Directory.CreateDirectory(MonoPath);

        DirectoryInfo source = new DirectoryInfo(pluginsPath);
        DirectoryInfo dest = new DirectoryInfo(MonoPath);

        Debug.Log(dest.FullName);

        foreach (FileInfo lib in source.GetFiles()) {
            lib.CopyTo(Path.Combine(dest.FullName, lib.Name), true);
        }
    }
}
