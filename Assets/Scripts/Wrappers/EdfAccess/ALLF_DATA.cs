using System.Runtime.InteropServices;
namespace EdfAccess {
    //this configuration mimics the union datatype found in C.
    //[StructLayout(LayoutKind.Explicit)]
    public struct ALLF_DATA {
        //[FieldOffset(0)]
        public FEVENT fe;

        //[FieldOffset(0)]
        public IMESSAGE im;

        //[FieldOffset(0)]
        public IOEVENT io;

        //[FieldOffset(0)]
        public FSAMPLE fs;

        //[FieldOffset(0)]
        public RECORDINGS rec;
    }
}
