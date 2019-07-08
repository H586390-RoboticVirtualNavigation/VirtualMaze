using System;
using System.Collections.Generic;
using System.IO;

public class CsvReader<T> : IDisposable {
    private StreamReader reader;
    private T currentData = default;
    private string[] currentRawData = null;
    private ICsvLineParser<T> parser;

    public bool HasNext { get => reader.Peek() > -1; }

    public CsvReader(string filePath, ICsvLineParser<T> parser) {
        reader = new StreamReader(filePath);

        this.parser = parser;
        parser.ParseHeader(reader);
    }

    public void Dispose() {
        reader.Dispose();
    }

    public T GetCurrentData() {
        return currentData;
    }

    public IList<string> GetCurrentRawData() {
        return Array.AsReadOnly(currentRawData);
    }

    public T GetNextData() {
        string str = reader.ReadLine();

        if (!string.IsNullOrEmpty(str)) {
            currentRawData = str.Split(',');
            currentData = parser.Parse(currentRawData);
        }
        else {
            currentRawData = null;
            currentData = default;
        }

        return currentData;
    }

    public void SetLineParser(ICsvLineParser<T> parser) {
        this.parser = parser;
    }
}

public interface ICsvLineParser<T1> {
    void ParseHeader(StreamReader reader);
    T1 Parse(string[] data);
}
