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
                case DataTypes.ENDEVENTS:
                case DataTypes.ENDBLINK:
                case DataTypes.ENDSACC:
                case DataTypes.ENDSAMPLES:
                case DataTypes.ENDFIX:
                case DataTypes.ENDPARSE:
                    return new FEventEnd(fe, type);

                case DataTypes.FIXUPDATE:
                case DataTypes.BUTTONEVENT:
                case DataTypes.INPUTEVENT:
                case DataTypes.LOST_DATA_EVENT:
                case DataTypes.STARTBLINK:
                case DataTypes.STARTSACC:
                case DataTypes.STARTFIX:
                case DataTypes.STARTPARSE:
                case DataTypes.STARTSAMPLES:
                case DataTypes.STARTEVENTS:
                    return new FEvent(fe, type);
                case DataTypes.MESSAGEEVENT:
                    return new MessageEvent(fe, type);

                case DataTypes.SAMPLE_TYPE:
                    return new Fsample(fs, type);
                
                case DataTypes.BREAKPARSE:
                    //fill if needed
                case DataTypes.RECORDING_INFO:
                    //fill if needed
                case DataTypes.NO_PENDING_ITEMS:
                case DataTypes.NULL:
                default:
                    return new EmptyData(type);
            }
        }
    }

    public abstract class AllFloatData {
        public readonly DataTypes dataType;
        public readonly uint time;

        public AllFloatData(DataTypes dataType, uint time) {
            this.dataType = dataType;
            this.time = time;
        }

        public abstract override string ToString();
    }

    public class EmptyData : AllFloatData {
        public EmptyData(DataTypes type) : base(type, 0) {
        }

        public override string ToString() {
            return dataType.ToString();
        }
    }
}
