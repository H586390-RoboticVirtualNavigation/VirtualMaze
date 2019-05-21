using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class SessionReader {
    private StreamReader reader;
    private string currentLine;

    public string currentData { get; private set; }

    public int flag { get; private set; } = -1;
    public float timeDelta { get; private set; } = -1;
    public float posX { get; private set; } = -1;
    public float posZ { get; private set; } = -1;
    public float rotY { get; private set; } = -1;

    public SessionTrigger trigger { get { return (SessionTrigger)((flag / 10) * 10); } }

    public SessionContext context;

    public SessionReader(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException();
        }
        reader = new StreamReader(filePath);
        parseHeader();
    }

    public bool NextData() {
        currentData = reader.ReadLine();
        if (!string.IsNullOrEmpty(currentData)) {
            ParseData(currentData);
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

    // 16 0.01696440 0.0000 -10.0000 0.0000 
    private void ParseData(string data) {
        if (data == null) {
            flag = -1;
            timeDelta = -1;
            posX = -1;
            posZ = -1;
            rotY = -1;
        }

        string[] dataArr = data.Trim().Split(' ');

        flag = int.Parse(dataArr[0]);
        timeDelta = float.Parse(dataArr[1]);
        posX = float.Parse(dataArr[2]);
        posZ = float.Parse(dataArr[3]);
        rotY = float.Parse(dataArr[4]);
    }

    private void parseHeader() {
        string currLine = reader.ReadLine();
        // check if first line is a JsonObject
        if (currLine[0] == '{') {
            context = JsonUtility.FromJson<SessionContext>(currLine);
        }
        else {
            //parse as per older
            context = parseHeaderOld(currLine);
        }

        //ignore session settings for now
        while (currLine != null && currLine[0] != ' ') {
            currLine = reader.ReadLine();
        }
    }

    /// <summary>
    /// Parses the old version of the header where information have to be manually parsed line by line.
    /// </summary>
    /// <returns></returns>
    private SessionContext parseHeaderOld(string currentLine) {
        SessionContext context = new SessionContext();

        string line = currentLine;
        context.version = GetValue(line);

        line = reader.ReadLine();
        context.triggerVersion = GetValue(line);

        line = reader.ReadLine();
        context.taskType = GetValue(line);

        line = reader.ReadLine();//ignore parsing of poster location for now
        //context.posterLocations = GetValue(lin;

        line = reader.ReadLine();
        context.trialName = GetValue(line);

        line = reader.ReadLine();
        int.TryParse(GetValue(line), out context.rewardsNumber);

        line = reader.ReadLine();
        int.TryParse(GetValue(line), out context.completionWindow);

        line = reader.ReadLine();
        int.TryParse(GetValue(line), out context.timeoutDuration);

        line = reader.ReadLine();
        int.TryParse(GetValue(line), out context.intersessionInterval);

        line = reader.ReadLine();
        int.TryParse(GetValue(line), out context.rewardTime);

        line = reader.ReadLine();
        float.TryParse(GetValue(line), out context.rotationSpeed);

        line = reader.ReadLine();
        float.TryParse(GetValue(line), out context.movementSpeed);

        line = reader.ReadLine();
        float.TryParse(GetValue(line), out context.joystickDeadzone);

        line = reader.ReadLine();
        float.TryParse(GetValue(line), out context.rewardViewCriteria);

        return context;
    }

    private string GetValue(string line) {
        return line.Split(':')[1].Trim();
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
}
