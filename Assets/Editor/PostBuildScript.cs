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
                WindowsCopyLibs(Path.GetDirectoryName(pathToBuiltProject));
                break;

            case BuildTarget.StandaloneOSX:
                MacCopyLibs(Path.GetDirectoryName(pathToBuiltProject));
                break;

            default:
                break;
        }
        Debug.Log(pathToBuiltProject);
    }

    private static void MacCopyLibs(string pathToBuiltProject)
    {
        //change the app name as required. "*.app"
        string dataPath = Path.Combine(pathToBuiltProject, "VirtualMaze.app/Contents");

        if (!Directory.Exists(dataPath))
        {
            Debug.LogError("Save the build as \"VirtualMaze\"! Build the game from File > Build Settings > Build to rename the Build!");
            throw new System.Exception("Unsupported buildName. Rename to VirtualMaze or change the PostBuildScript.cs");
        }

        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string destPath = Path.Combine(dataPath, @"Resources/Runtimes/osx-x64/native/");
        Directory.CreateDirectory(destPath);

        CopyFolder(pluginsPath, destPath);
    }

    private static void WindowsCopyLibs(string pathToBuiltProject) {
        string dataPath = Path.Combine(pathToBuiltProject,"VirtualMaze_Data");
        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string monoPath = Path.Combine(dataPath, "Mono");
        Directory.CreateDirectory(monoPath);

        CopyFolder(pluginsPath, monoPath);
    }

    private static void CopyFolder(string from, string to)
    {
        DirectoryInfo source = new DirectoryInfo(from);
        DirectoryInfo dest = new DirectoryInfo(to);

        Debug.Log(dest.FullName);

        foreach (FileInfo lib in source.GetFiles())
        {
            lib.CopyTo(Path.Combine(dest.FullName, lib.Name), true);
        }
    }
}
