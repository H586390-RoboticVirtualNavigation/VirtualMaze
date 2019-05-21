using Eyelink.Structs;
using System.Runtime.InteropServices;

namespace Eyelink.EdfAccess {
    public class EdfAccessWrapper {
        //see https://docs.unity3d.com/Manual/PlatformDependentCompilation.html 
#if (UNITY_STANDALONE_WIN)
        const string dll = "edfapi"; //name of dll file. not including extension
#elif (UNITY_STANDALONE_OSX)
        const string dll = "edf2asc"; //name of dll file. not including extension
#else
        const string dll = ""; //nothing such that it will "safely fail"
#endif
        //EDF data access functions
        [DllImport(dll)]
        private static extern unsafe EDFFILE* edf_open_file(
            string fname,
            int consistency,
            int loadevents,
            int loadsamples,
            out int errval
            );

        public static unsafe EdfFilePointer EdfOpenFile(
            string fname,
            int consistency,
            int loadevents,
            int loadsamples,
            out int errval
            ) {
            EDFFILE* filePointer = edf_open_file(fname, consistency, loadevents, loadevents, out errval);
            return new EdfFilePointer(filePointer);
        }

        [DllImport(dll)]
        private static extern unsafe int edf_close_file(EDFFILE* ef);

        public static unsafe int EdfCloseFile(EdfFilePointer pointer) {
            return edf_close_file(pointer.value);
        }

        [DllImport(dll)]
        private static extern unsafe int edf_get_next_data(EDFFILE* ef);

        public static unsafe DataTypes EdfGetNextData(EdfFilePointer pointer) {
            return (DataTypes) edf_get_next_data(pointer.value);
        }

        [DllImport(dll)]
        private static extern unsafe ALLF_DATA* edf_get_float_data(EDFFILE* ef);

        public static unsafe ALLF_DATA EdfGetFloatData(EdfFilePointer ef) {
            ALLF_DATA* data = edf_get_float_data(ef.value);

            return *data;
        }

        [DllImport(dll)]
        private static extern unsafe uint edf_get_element_count(EDFFILE* ef);

        public static unsafe uint EdfGetElementCount(EdfFilePointer filePointer) {
            return edf_get_element_count(filePointer.value);
        }

        [DllImport(dll)]
        public static extern int edf_get_preamble_text(EDFFILE ef, char[] buffer, int length);

        [DllImport(dll)]
        public static extern int edf_get_preamble_text_length(EDFFILE edf);
    }

    //wrapper to make the pointer easily used in safe code.
    public unsafe class EdfFilePointer {
        public readonly EDFFILE* value;

        public EdfFilePointer(EDFFILE* pointer) {
            value = pointer;
        }
    }
}
