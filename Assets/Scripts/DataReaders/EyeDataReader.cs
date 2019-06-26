using Eyelink.Structs;

public abstract class EyeDataReader {
    public readonly string filePath;

    public EyeDataReader(string filePath) {
        this.filePath = filePath;
    }

    public abstract AllFloatData GetNextData();
    public abstract AllFloatData GetCurrentData(DataTypes dataType);

    public virtual void Close() { }
}
