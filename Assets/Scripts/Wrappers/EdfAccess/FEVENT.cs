namespace EdfAccess {
    //types differ from Eyelink documentation.
    //See https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/built-in-types-table
    
    public struct FEVENT {
        public uint time;
        public short type;
        public ushort read;
        public uint sttime;
        public uint entime;
        public float hstx;
        public float hsty;
        public float gstx;
        public float gsty;
        public float sta;
        public float henx;
        public float heny;
        public float genx;
        public float geny;
        public float ena;
        public float havx;
        public float havy;
        public float gavx;
        public float gavy;
        public float ava;
        public float avel;
        public float pvel;
        public float svel;
        public float evel;
        public float supd_x;
        public float eupd_x;
        public float supd_y;
        public float eupd_y;
        public short eye;
        public ushort status;
        public ushort flags;
        public ushort input;
        public ushort buttons;
        public ushort parsedby;
        //from documentation it is LSTRING * message but it has no equivalent in C#, trying to use char*
        public unsafe char* message;// LSTRING * message; 
    }
}
