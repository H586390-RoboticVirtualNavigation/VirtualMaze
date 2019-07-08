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

    public int LineNumber { get; protected set; } = 0;

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
            LineNumber++;
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
            NextData();
        }

        //search for next trigger/flag
        while (currData != null && currData.flag == 0) {
            NextData();
        }
    }

    public void MoveToNextTrigger(SessionTrigger trigger) {
        //if current is already pointing to a trigger move forward first
        if (currData != null && currData.flag != 0) {
            NextData();
        }

        //search for next trigger/flag
        while (currData != null && currData.trigger != trigger) {
            NextData();
        }
    }

    protected virtual SessionData ParseData(string data) {
        string[] dataArr = data.Trim().Split(' ');

        int flag = int.Parse(dataArr[0]);
        decimal timeDelta = decimal.Parse(dataArr[1]);
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
            LineNumber++;
        }
        else {
            //parse as older header
            context = new SessionContext(currLine, r);
            LineNumber += 14;
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
        return reader.Peek() > -1;
    }
}
