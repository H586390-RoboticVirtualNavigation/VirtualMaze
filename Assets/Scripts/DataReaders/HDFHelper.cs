using HDF.PInvoke;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HDFHelper {

    const string MAT_REF = "#refs#";

    /// <summary>
    /// under the assumption that all data needed by this program is stored as 
    /// "/*/data" with reference to the root group
    /// </summary>
    public static HDF5Group GetMyDataGroup(AbstractHDF5File file) {
        using (HDF5Group root = new HDF5Group(file)) {
            ulong idx = 0;

            string groupName = null;

            // callback for H5L.iterate
            H5L.iterate_t findFirstGroup = new H5L.iterate_t((long group, IntPtr name, ref H5L.info_t info, IntPtr op_data) =>
            {
                string temp = Marshal.PtrToStringAnsi(name);
                Debug.LogWarning($"temp: {temp}");
                if (!temp.Contains(MAT_REF)) {
                    groupName = temp;
                    return 1; //short circuit success
                }
                return 0; //continue
            });

            H5L.iterate(root.id, H5.index_t.NAME, H5.iter_order_t.NATIVE, ref idx, findFirstGroup, IntPtr.Zero);

            if (groupName != null) {
                return new HDF5Group(file, $"/{groupName}/data");
            }
        }

        return null;
    }

    public static T[,] GetDataMatrix<T>(HDF5Group group, string dsetName) {
        long dset_id = 0, space_id = 0, type_id = 0;
        long result = 0;
        T[,] data = null;
        try {
            result = dset_id = H5D.open(group.id, dsetName);
            if (result < 0L) {
                return null;
            }

            result = space_id = H5D.get_space(dset_id);
            if (result < 0L) {
                return null;
            }

            result = H5S.is_simple(space_id);
            if (result <= 0L) {
                return null;
            }

            int ndim = H5S.get_simple_extent_ndims(space_id);//number of dimensions

            ulong[] dims = new ulong[ndim];

            if (H5S.get_simple_extent_dims(space_id, dims, null) < 0) {
                return null; //return null if fail
            }

            type_id = H5D.get_type(dset_id);

            data = new T[dims[0], dims[1]];

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            H5D.read(dset_id, type_id, H5S.ALL, H5S.ALL, H5P.DEFAULT, handle.AddrOfPinnedObject());
            handle.Free();
        }
        catch (Exception e) {
            throw e;
        }
        finally {
            H5T.close(type_id);
            H5S.close(space_id);
            H5D.close(dset_id);
        }

        return data;
    }

    public static void RefreshDataSpace(long dataset_id, ref long space_id) {
        H5S.close(space_id);
        space_id = H5D.get_space(dataset_id);
    }
}
