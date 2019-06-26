using Eyelink.EdfAccess;
using Eyelink.Structs;

public class EDFReader : EyeDataReader {
    private EdfFilePointer pointer;

    public EDFReader(string filePath, out int errVal) : base(filePath) {
        pointer = EdfAccessWrapper.EdfOpenFile(filePath, 0, 1, 1, out errVal);
    }

    public override AllFloatData GetNextData() {
        DataTypes type = EdfAccessWrapper.EdfGetNextData(pointer);
        return GetCurrentData(type);
    }

    public override AllFloatData GetCurrentData(DataTypes dataType) {
        return EdfAccessWrapper.EdfGetFloatData(pointer).ConvertToAllFloatData(dataType);
    }
}
