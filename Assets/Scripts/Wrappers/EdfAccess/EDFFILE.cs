namespace EdfAccess {
    // EDFFILE is a dummy structure that holds an EDF file handle.
    public struct EDFFILE {
    }

    //wrapper to make the pointer easily used in safe code.
    public unsafe class EdfFilePointer {
        public readonly EDFFILE* value;

        public EdfFilePointer(EDFFILE* pointer) {
            value = pointer;
        }
    }
}
