using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads a text file line by line. 
/// </summary>
[Serializable]
public class SessionReader : ISessionDataReader {
    private StreamReader reader;
    public SessionContext context;

    private SessionData currData;
    public SessionData CurrentData => currData;

    private int lineNumber = 0;
    public int CurrentIndex => lineNumber;

    public bool HasNext => reader.Peek() > -1;

    private readonly string filePath;

    public SessionReader(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException();
        }

        this.filePath = filePath;

        reader = new StreamReader(filePath);
        ParseHeader(reader);
    }

    public bool Next() {
        string currentData = reader.ReadLine();
        if (!string.IsNullOrEmpty(currentData)) {
            lineNumber++;
            currData = ParseData(currentData);
            return true;
        }
        else {
            return false;
        }
    }

    public void MoveToNextTrigger() {
        //if current is already pointing to a trigger move forward first
        if (currData != null && currData.flag != 0) {
            Next();
        }

        //search for next trigger/flag
        while (currData != null && currData.flag == 0) {
            Next();
        }
    }

    public void MoveToNextTrigger(SessionTrigger trigger) {
        //if current is already pointing to a trigger move forward first
        if (currData != null && currData.flag != 0) {
            Next();
        }

        //search for next trigger/flag
        while (currData != null && currData.trigger != trigger) {
            Next();
        }
    }

    public void ParseHeader(StreamReader r) {
        string currLine = r.ReadLine();
        // check if first line is a JsonObject
        if (currLine[0] == '{') { // newest version
            context = JsonUtility.FromJson<SessionContext>(currLine);
            lineNumber++;
        }
        else {
            //parse as older header
            context = new SessionContext(currLine, r);
            lineNumber += 14;
        }
    }

    //helper method to extract the Context from the file
    public static void ExtractInfo(string filePath, out SessionContext context, out int numFrames) {
        using (SessionReader r = new SessionReader(filePath)) {
            context = r.context;

            //get number of lines
            int lineCount = 0;
            while (r.reader.ReadLine() != null) {
                lineCount++;
            }

            numFrames = lineCount;
        }
    }

    public void Dispose() {
        reader.Close();
        reader.Dispose();
    }

    public SessionData ParseData(string rawData) {
        string[] dataArr = rawData.Trim().Split(' ');

        int flag = int.Parse(dataArr[0]);
        decimal timeDelta = decimal.Parse(dataArr[1]);
        float posX = float.Parse(dataArr[2]);
        float posZ = float.Parse(dataArr[3]);
        float rotY = float.Parse(dataArr[4]);

        return new SessionData(flag, timeDelta, posX, posZ, rotY);
    }
}
