using UnityEngine;
using System;
using System.IO;

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
        parseHeader();
    }

    public bool NextData() {
        string currentData = reader.ReadLine();
        if (!string.IsNullOrEmpty(currentData)) {
            currData = new SessionData(currentData);
            return true;
        }
        else {
            return false;
        }
    }

    public void Close() {
        reader.Close();
        reader.Dispose();
    }

    private void parseHeader() {
        string currLine = reader.ReadLine();
        // check if first line is a JsonObject
        if (currLine[0] == '{') { // newest version
            context = JsonUtility.FromJson<SessionContext>(currLine);
        }
        else {
            //parse as older header
            context = new SessionContext(currLine, reader);
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
