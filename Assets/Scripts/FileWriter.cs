using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class FileWriter {

	public static StreamWriter CreateFileInFolder(string folderPath, string filename){
		try{
			if(Directory.Exists(folderPath)){
				return new StreamWriter(folderPath + "/" + filename);
			}
		}catch{
		}
		return null;
	}
}
