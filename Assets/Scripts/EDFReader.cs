using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

namespace EDFUtil {
    public class EDFReader {
        private StreamReader reader;

        public SessionContext context;

        public EDFReader(string filePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException();
            }
            reader = new StreamReader(filePath);
        }

        public EdfData getNextData() {
            string currentData = reader.ReadLine();

            if (string.IsNullOrEmpty(currentData)) {
                return null;
            }

            if (currentData.EndsWith("...")) {
                return parseSample(currentData);
            }

            return parseMessage(currentData);
        }

        private EdfMessage parseMessage(string currentData) {
            if (currentData.StartsWith("MSG")) {
                //MSG	1686949	Trigger Version 84
                string[] data = currentData.Trim().Split('\t');

            }
            return null; //for now focus on MSG events, ignore the others
        }

        private EdfSampleData parseSample(string currentData) {
            string[] data = currentData.Trim().Split('\t');

            if (data.Length == 0) { return null; }

            float.TryParse(data[0], out float timeStamp);
            float.TryParse(data[1], out float gazeX);
            float.TryParse(data[2], out float gazeY);

            return new EdfSampleData(timeStamp, gazeX, gazeY);
        }

        public void Close() {
            reader.Close();
            reader.Dispose();
        }
    }

    public class EdfMessage : EdfData {
        public readonly float timeStamp;
        public readonly string message;
        public readonly int flag;

        public EdfMessage(float timeStamp, string message, int flag) {
            this.timeStamp = timeStamp;
            this.message = message;
            this.flag = flag;
        }
    }

    public class EdfSampleData : EdfData {
        public readonly float timeStamp;
        public readonly float gazeX;
        public readonly float gazeY;

        public EdfSampleData(float timeStamp, float gazeX, float gazeY) {
            this.timeStamp = timeStamp;
            this.gazeX = gazeX;
            this.gazeY = gazeY;
        }
    }

    public abstract class EdfData {
    }
}