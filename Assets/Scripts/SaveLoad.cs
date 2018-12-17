using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


public class SaveLoad{

	private static List<Settings> _settingsList;
	public static List<Settings> settingsList{
		get{
			BinaryFormatter bf = new BinaryFormatter ();
			if(File.Exists(Application.persistentDataPath + "/settingslist.gd")) {
				FileStream file = File.Open (Application.persistentDataPath + "/settingslist.gd", FileMode.Open);
				try{
					_settingsList = bf.Deserialize (file) as List<Settings>;
					file.Close ();
				}catch{
					file.Close ();
					File.Delete(Application.persistentDataPath + "/settingslist.gd");
					_settingsList = new List<Settings>();
				}
			}else{
				_settingsList = new List<Settings>();
			}
			return _settingsList;
		}

		set{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (Application.persistentDataPath + "/settingslist.gd");
			bf.Serialize (file, value);
			file.Close ();
			_settingsList = value;
		}
	}
}
