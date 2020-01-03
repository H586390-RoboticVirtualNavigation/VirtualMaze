using HDF.PInvoke;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BinRecorder : IDisposable {
    const int RANK = 2; // number of dimensions in dataset
    private readonly string DATASETNAME = "values";

    private long dataspace;
    private long file;
    private int status;
    private long dataset;

    public BinRecorder() {
        ulong[] dims = { 0, 0 }; // row, col
        ulong[] maxDims = { H5S.UNLIMITED, H5S.UNLIMITED };
        ulong[] chunk_dims = { 1, 10 };

        /* Create the data space with unlimited dimensions. */
        dataspace = H5S.create_simple(RANK, dims, maxDims);

        /* Create a new file. If file exists its contents will be overwritten. */
        file = H5F.create("binData.hdf", H5F.ACC_TRUNC, H5P.DEFAULT, H5P.DEFAULT);

        /* Modify dataset creation properties, i.e. enable chunking  */
        long prop = H5P.create(H5P.DATASET_CREATE);
        status = H5P.set_chunk(prop, RANK, chunk_dims);

        /* Set fill value*/
        double fillValue = 0;

        GCHandle h1 = GCHandle.Alloc(fillValue, GCHandleType.Pinned);

        status = H5P.set_fill_value(prop, H5T.NATIVE_DOUBLE, h1.AddrOfPinnedObject());

        h1.Free();

        /* Create a new dataset within the file using chunk
       creation properties.  */
        dataset = H5D.create(file, DATASETNAME, H5T.NATIVE_DOUBLE, dataspace,
                             H5P.DEFAULT, prop, H5P.DEFAULT);

        H5P.close(prop);
    }



    public void Dispose() {
        H5D.close(dataset);
        H5S.close(dataspace);
        H5F.close(file);
    }

    public void RecordMovement(uint timestamp, Vector2 gaze, HashSet<int> bin_ids) {
        HDFHelper.RefreshDataSpace(dataset, ref dataspace);

        ulong[] currDatasetDim = new ulong[RANK];
        H5S.get_simple_extent_dims(dataspace, currDatasetDim, null);

        ulong dataLen = (ulong)(3 + bin_ids.Count);

        ulong[] newDatasetDim = { currDatasetDim[0] + 1, Math.Max(dataLen, currDatasetDim[1]) };
        H5D.set_extent(dataset, newDatasetDim);

        HDFHelper.RefreshDataSpace(dataset, ref dataspace);

        /* Prepare the data to be saved */
        double[,] dataArr = new double[1, dataLen];

        dataArr[0, 0] = timestamp;
        dataArr[0, 1] = gaze.x;
        dataArr[0, 2] = gaze.y;

        int i = 0;
        foreach (int id in bin_ids) {
            dataArr[0, 3 + i] = id;
            i++;
        }

        ulong[] dataDim = { 1, dataLen };

        ulong[] offset = new ulong[RANK] { currDatasetDim[0], 0 };
        status = H5S.select_hyperslab(dataspace, H5S.seloper_t.SET, offset, null, dataDim, null);
        long newDataMemspace = H5S.create_simple(RANK, dataDim, null);

        GCHandle h = GCHandle.Alloc(dataArr, GCHandleType.Pinned);

        /* Write data to dataset */
        status = H5D.write(dataset, H5T.NATIVE_DOUBLE, newDataMemspace, dataspace,
                           H5P.DEFAULT, h.AddrOfPinnedObject());

        h.Free();

        H5S.close(newDataMemspace);
    }
}
