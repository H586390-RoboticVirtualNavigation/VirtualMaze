using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

[assembly: AssemblyVersionAttribute("4.1.*")]
public class VersionInfo {
    public static string Version { get => Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
    public static int MajorVersion { get => Assembly.GetExecutingAssembly().GetName().Version.Major; }
    
}
