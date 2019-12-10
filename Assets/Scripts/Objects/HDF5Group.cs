using HDF.PInvoke;
using System;
public class HDF5Group : IDisposable {
    public readonly long id;
    public bool IsOpened { get => id > -1; }

    /// <summary>
    /// Opens group at root of the file
    /// </summary>
    /// <param name="file">File where group is to be opened</param>
    public HDF5Group(AbstractHDF5File file) : this(file, "/") {
        //calls the other constructor
    }

    /// <summary>
    /// Opens group at the specified subgroup
    /// </summary>
    /// <param name="file">File where group is to be opened</param>
    /// <param name="grpPath">Full path of the subgroup starting from root "\"</param>
    public HDF5Group(AbstractHDF5File file, string grpPath) {
        id = H5G.open(file.id, grpPath);
    }

    public void Dispose() {
        H5G.close(id);
    }

    public override string ToString() {
        return id.ToString();
    }
}
