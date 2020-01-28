using Eyelink.Structs;
using System.Runtime.InteropServices;

namespace Eyelink.EdfAccess {
    public class EdfAccessWrapper {
        //see https://docs.unity3d.com/Manual/PlatformDependentCompilation.html 
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
        const string dll = "edfapi"; //name of dll file. not including extension
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
        const string dll = "edfapiMac"; //name of bundle file. not including extension
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

        /// <summary>
        /// Opens the EDF file passed in by edf_file_name and preprocesses the EDF file.
        /// </summary>
        /// <param name="fname">Name of the EDF file to be opened.</param>
        /// <param name="consistency">Consistency check control (for the time stamps of the start and end events, etc). 
        ///                             0, no consistency check. 
        ///                             1, check consistency and report. 
        ///                             2, check consistency and fix.</param>
        /// <param name="loadevents">load/skip loading events 
        ///                             0, do not load events. 
        ///                             1, load events.</param>
        /// <param name="loadsamples">load/skip loading of samples 
        ///                             0, do not load samples. 
        ///                             1, load samples.</param>
        /// <param name="errval">This parameter is used for returning error value. The pointer should be a valid pointer to an
        ///                             integer. If the returned value is not 0 then an error occurred.</param>
        /// <returns>An EdfFilePointer to access the edf file</returns>
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
            return (DataTypes)edf_get_next_data(pointer.value);
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
