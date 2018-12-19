using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ParallelPort {
	[DllImport("inpoutx64.dll", EntryPoint = "Out32")]
	private static extern void Out32(int address, int value);

    public static void TryOut32(int address, int value)
    {
        try
        {
            Out32(address, value);
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogException(e);
        }
    }
}
