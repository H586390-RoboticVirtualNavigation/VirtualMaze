using System.Runtime.InteropServices;

namespace Eyelink.Structs {
    //this configuration mimics the union datatype found in C.
    [StructLayout(LayoutKind.Explicit)]
    public struct ALLF_DATA {
        [FieldOffset(0)]
        public FEVENT fe;

        [FieldOffset(0)]
        public IMESSAGE im;

        [FieldOffset(0)]
        public IOEVENT io;

        [FieldOffset(0)]
        public FSAMPLE fs;

        [FieldOffset(0)]
        public RECORDINGS rec;

        public uint getTime(DataTypes type) {
            switch (type) {
                case DataTypes.SAMPLE_TYPE:
                    return fs.time;

                case DataTypes.MESSAGEEVENT:
                    return fe.sttime;

                default:
                    return fe.sttime;
            }
        }
    }
}
