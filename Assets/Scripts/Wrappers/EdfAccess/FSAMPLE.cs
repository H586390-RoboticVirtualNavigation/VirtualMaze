//the arrays may need to be set as unsafe | fixed float gxvel[3];
namespace EdfAccess {
    public unsafe struct FSAMPLE {
        public short time;
        public fixed float px[2];
        public fixed float py[2];
        public fixed float hx[2];
        public fixed float hy[2];
        public fixed float pa[2];
        public fixed float gx[2];
        public fixed float gy[2];
        public float rx;
        public float ry;
        public fixed float gxvel[2];
        public fixed float gyvel[2];
        public fixed float hxvel[2];
        public fixed float hyvel[2];
        public fixed float rxvel[2];
        public fixed float ryvel[2];
        public fixed float fgxvel[2];
        public fixed float fgyvel[2];
        public fixed float fhxvel[2];
        public fixed float fhyvel[2];
        public fixed float frxvel[2];
        public fixed float fryvel[2];
        public fixed short hdata[8];
        public ushort flags;
        public ushort input;
        public ushort buttons;
        public short htype;
        public ushort errors;
    }
}