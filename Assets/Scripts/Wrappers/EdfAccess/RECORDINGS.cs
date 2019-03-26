namespace EdfAccess {
    public struct RECORDINGS {
        public uint time;
        public float sample_rate;
        public ushort eflags;
        public ushort sflags;
        public byte state;
        public byte record_type;
        public byte pupil_type;
        public byte recording_mode;
        public byte filter_type;
        public byte pos_type;
        public byte eye;
    }
}
