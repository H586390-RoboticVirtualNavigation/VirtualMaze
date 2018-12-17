using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ParallelPort {
	[DllImport("InpOutx64.dll", EntryPoint = "Out32")]
	public static extern void Out32(int address, int value);
}
