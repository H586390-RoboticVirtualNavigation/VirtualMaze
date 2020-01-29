using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// Refer to https://github.com/surban/HDF.PInvoke/blob/master/HDF5/H5DLLImporter.cs
/// for the locations where the HDF.PInvoke searchs for HDF5 libraries.
/// </summary>
public class PostBuildScript {
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

            case BuildTarget.StandaloneLinux64:
                LinuxCopyLibs(Path.GetDirectoryName(pathToBuiltProject));
                break;

            default:
                break;
        }
        Debug.Log(pathToBuiltProject);
    }

    private static void MacCopyLibs(string pathToBuiltProject) {
        //change the app name as required. "*.app"
        string dataPath = Path.Combine(pathToBuiltProject, "VirtualMaze.app/Contents");

        if (!Directory.Exists(dataPath)) {
            Debug.LogError("Save the build as \"VirtualMaze\"! Build the game from File > Build Settings > Build to rename the Build!");
            throw new System.Exception("Unsupported buildName. Rename to VirtualMaze or change the PostBuildScript.cs");
        }

        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string destPath = Path.Combine(dataPath, @"Resources/runtimes/osx-x64/native/");
        Directory.CreateDirectory(destPath);

        CopyFolder(pluginsPath, destPath);
    }

    private static void WindowsCopyLibs(string pathToBuiltProject) {
        string dataPath = Path.Combine(pathToBuiltProject, "VirtualMaze_Data");
        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string monoPath = Path.Combine(dataPath, "Mono");
        Directory.CreateDirectory(monoPath);

        CopyFolder(pluginsPath, monoPath);
    }

    private static void LinuxCopyLibs(string pathToBuiltProject) {
        string dataPath = Path.Combine(pathToBuiltProject, "VirtualMaze_Data");

        if (!Directory.Exists(dataPath)) {
            Debug.LogError("Save the build as \"VirtualMaze\"! Build the game from File > Build Settings > Build to rename the Build!");
            throw new System.Exception("Unsupported buildName. Rename to VirtualMaze or change the PostBuildScript.cs");
        }

        /* Copy libhdf5.so.103 and libhdf5hl.so.100 into built plugin folder first */

        /* \<Project Directory>\VirtualMaze\Assets\Plugins\runtimes\linux - x64\native */
        string assetPluginsPath = Path.Combine(Application.dataPath, @"Plugins/runtimes/linux-x64/native");
        string pluginsPath = Path.Combine(dataPath, "Plugins");

        string so103 = "libhdf5.so.103";
        string hlso103 = "libhdf5_hl.so.100";

        File.Copy(Path.Combine(assetPluginsPath, so103), Path.Combine(pluginsPath, so103));
        File.Copy(Path.Combine(assetPluginsPath, hlso103), Path.Combine(pluginsPath, hlso103));


        string destPath = Path.Combine(pathToBuiltProject, @"runtimes/linux-x64/native/");

        Directory.CreateDirectory(destPath);

        CopyFolder(pluginsPath, destPath);
    }

    private static void CopyFolder(string from, string to) {
        DirectoryInfo source = new DirectoryInfo(from);
        DirectoryInfo dest = new DirectoryInfo(to);

        Debug.Log(dest.FullName);

        foreach (FileInfo lib in source.GetFiles()) {
            lib.CopyTo(Path.Combine(dest.FullName, lib.Name), true);
        }
    }
}
