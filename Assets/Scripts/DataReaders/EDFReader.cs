using Eyelink.EdfAccess;
using Eyelink.Structs;

public class EDFReader : EyeDataReader {
    private EdfFilePointer pointer;

    public EDFReader(string filePath, out int errVal) {
        pointer = EdfAccessWrapper.EdfOpenFile(filePath, 0, 1, 1, out errVal);
    }

    public  AllFloatData GetNextData() {
        DataTypes type = EdfAccessWrapper.EdfGetNextData(pointer);
        return GetCurrentData(type);
    }

    public  AllFloatData GetCurrentData(DataTypes dataType) {
        return EdfAccessWrapper.EdfGetFloatData(pointer).ConvertToAllFloatData(dataType);
    }

    public void Dispose() {
        //nothing to dispose
    }
}
