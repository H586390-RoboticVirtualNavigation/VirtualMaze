public enum DataTypes {
    STARTBLINK = 3,     // pupil disappeared, time only
    ENDBLINK = 4,       // pupil reappeared, duration data
    STARTSACC = 5,      // start of saccade, time only
    ENDSACC = 6,        // end of saccade, summary data
    STARTFIX = 7,       // start of fixation, time only
    ENDFIX = 8,         // end of fixation, summary data
    FIXUPDATE = 9,      // update within fixation, summary data for interval
    MESSAGEEVENT = 24,  // user-definable text: IMESSAGE structure
    BUTTONEVENT = 25,   // button state change: IOEVENT structure
    INPUTEVENT = 28,    // change of input port: IOEVENT structure
    SAMPLE_TYPE = 200,  // type code for samples
    LOST_DATA_EVENT = 0x3F, // NEW: Event flags gap in data stream

    STARTPARSE = 1,     /* these only have time and eye data */
    ENDPARSE = 2,
    BREAKPARSE = 10,

    STARTSAMPLES = 15,  /* start of events in block */
    ENDSAMPLES = 16,    /* end of samples in block */
    STARTEVENTS = 17,   /* start of events in block */
    ENDEVENTS = 18,     /* end of events in block */

    RECORDING_INFO = 30,  /* recording struct is returned */
    NO_PENDING_ITEMS = 0  /*no more data left.*/
}



