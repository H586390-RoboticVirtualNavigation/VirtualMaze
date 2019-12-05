using System;
using System.IO;

public class SpikeTimeParser : IDisposable {
    private StreamReader reader;
    private string[] spikeTimings = null;

    public bool HasNext { get => reader.Peek() > -1; }

    public SpikeTimeParser(string filePath) {
        reader = new StreamReader(filePath);
        spikeTimings = reader.ReadLine().Trim().Split(',');
    }

    public void Dispose() {
        reader.Dispose();
    }

    public int Length { get => spikeTimings.Length; }

    public decimal this[int index] {
        get => decimal.Parse(spikeTimings[index], System.Globalization.NumberStyles.Float);
    }
}
