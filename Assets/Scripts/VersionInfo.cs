using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[assembly: AssemblyVersion("4.1.*")]
public class VersionInfo {
    /// <summary>
    /// returns Major.Minor.Build formatted Version
    /// </summary>
    public static string VersionString { get => Assembly.GetExecutingAssembly().GetName().Version.ToString(3); }
    public static Version Version { get => Assembly.GetExecutingAssembly().GetName().Version; }
    public static int MajorVersion { get => Assembly.GetExecutingAssembly().GetName().Version.Major; }
    public static int MinorVersion { get => Assembly.GetExecutingAssembly().GetName().Version.Minor; }
}
