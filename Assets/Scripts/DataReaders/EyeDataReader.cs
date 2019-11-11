using Eyelink.Structs;
using System;

public interface EyeDataReader : IDisposable{
      AllFloatData GetNextData();
      AllFloatData GetCurrentData(DataTypes dataType);
}
