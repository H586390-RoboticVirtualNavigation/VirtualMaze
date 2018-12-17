using UnityEngine;
using UnityEditor;

public class ResetPlayerPrefEditor : Editor {

	[MenuItem("Edit/Reset Playerprefs")] public static void DeletePlayerPrefs() { 
		PlayerPrefs.DeleteAll(); 
	}
}
