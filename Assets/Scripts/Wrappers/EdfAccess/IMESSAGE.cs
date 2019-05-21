namespace Eyelink.Structs {
    public unsafe struct IMESSAGE {
        public uint time;
        public short type;
        public ushort length;
        public char* text;
    }
}