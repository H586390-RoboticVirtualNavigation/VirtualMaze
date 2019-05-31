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

        public AllFloatData ConvertToAllFloatData(DataTypes type) {
            switch (type) {
                case DataTypes.MESSAGEEVENT:
                    return new FEvent(fe, type);
                case DataTypes.SAMPLE_TYPE:
                    return new Fsample(fs, type);
                default:
                    return new EmptyData(type);
            }
        }
    }

    public abstract class AllFloatData {
        public readonly DataTypes dataType;
        public abstract uint Time { get; }

        public AllFloatData(DataTypes type) {
            this.dataType = type;
        }

        public abstract override string ToString();
    }

    public class EmptyData : AllFloatData {
        public EmptyData(DataTypes type) : base(type) {
        }

        public override uint Time => 0;

        public override string ToString() {
            return dataType.ToString();
        }
    }
}
