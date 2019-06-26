using Eyelink.Structs;
using System.IO;

public class EyeCsvReader : EyeDataReader {
    StreamReader r;
    AllFloatData current = null;

    public EyeCsvReader(string filePath) : base(filePath) {
        r = new StreamReader(filePath);
    }

    public override AllFloatData GetCurrentData(DataTypes dataType) {
        return current;
    }

    public override AllFloatData GetNextData() {
        string data = r.ReadLine();
        if (string.IsNullOrEmpty(data)) {
            current = null;    
        }
        else {
            string[] dataArr = data.Split(',');

            uint time = uint.Parse(dataArr[0]);
            DataTypes type = (DataTypes)int.Parse(dataArr[1]);

            switch (type) {
                case DataTypes.MESSAGEEVENT:
                    current =  new FEvent(time, dataArr[4], type);
                    break;

                case DataTypes.SAMPLE_TYPE:
                    float gx = float.Parse(dataArr[2]);
                    float gy = float.Parse(dataArr[3]);
                    current = new Fsample(time, gx, gy, type);
                    break;
                    
                default:
                    current = null;
                    break;
            }
        }

        return current;
    }

    public override void Close() {
        r.Close();
        r.Dispose();
        base.Close();
    }
}
