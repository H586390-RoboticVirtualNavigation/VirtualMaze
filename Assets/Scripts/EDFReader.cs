using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EDFReader : MonoBehaviour
{
    private StreamReader reader;
    private string currentLine;

    public string currentData { get; private set; }

    public int flag { get; private set; } = -1;
    public float timeDelta { get; private set; } = -1;
    public float posX { get; private set; } = -1;
    public float posZ { get; private set; } = -1;
    public float rotY { get; private set; } = -1;

    public SessionContext context;

    public EDFReader(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException();
        }
        reader = new StreamReader(filePath);
        
    }

    public void Close() {
        reader.Close();
    }

    public struct Data {

    }
}
