using HDF.PInvoke;
using System;

public abstract class AbstractHDF5File : IDisposable {
    public readonly long id;
    public bool IsOpened { get => id > -1; }

    public AbstractHDF5File(string filePath) {
        id = H5F.open(filePath, H5F.ACC_RDONLY);
    }

    public void Dispose() {
        H5F.close(id);
    }

    public override string ToString() {
        return id.ToString();
    }
}
