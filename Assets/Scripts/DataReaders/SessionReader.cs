using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads a text file line by line. 
/// </summary>
[Serializable]
public class SessionReader {
    private StreamReader reader;

    public SessionData currData;

    public SessionContext context;

    public SessionReader(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException();
        }
        reader = new StreamReader(filePath);
        ParseHeader(reader);
    }

    public bool NextData() {
        string currentData = reader.ReadLine();
        if (!string.IsNullOrEmpty(currentData)) {
            currData = ParseData(currentData);
            return true;
        }
        else {
            return false;
        }
    }

    protected virtual SessionData ParseData(string data) {
        string[] dataArr = data.Trim().Split(' ');

        int flag = int.Parse(dataArr[0]);
        float timeDelta = float.Parse(dataArr[1]);
        float posX = float.Parse(dataArr[2]);
        float posZ = float.Parse(dataArr[3]);
        float rotY = float.Parse(dataArr[4]);

        return new SessionData(flag, timeDelta, posX, posZ, rotY);
    }

    public void Close() {
        reader.Close();
        reader.Dispose();
    }

    protected virtual void ParseHeader(StreamReader r) {
        string currLine = r.ReadLine();
        // check if first line is a JsonObject
        if (currLine[0] == '{') { // newest version
            context = JsonUtility.FromJson<SessionContext>(currLine);
        }
        else {
            //parse as older header
            context = new SessionContext(currLine, r);
        }
    }

    //helper method to extract the Context from the file
    public static void ExtractInfo(string filePath, out SessionContext context, out int numFrames) {
        SessionReader r = new SessionReader(filePath);
        context = r.context;

        //get number of lines
        int lineCount = 0;
        while (r.reader.ReadLine() != null) {
            lineCount++;
        }

        numFrames = lineCount;

        r.Close();
    }

    public bool HasNext() {
        return reader.Peek() > 0;
    }
}
