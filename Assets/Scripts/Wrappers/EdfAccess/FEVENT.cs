using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Eyelink.Structs {
    public struct FEVENT {
        public uint time;
        public short type;

        /// <summary>
        /// flags of which items are included
        /// </summary>
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

        /// <summary>
        /// contains error or warning flags
        /// </summary>
        public ushort flags;

        public ushort input;
        public ushort buttons;
        public ushort parsedby;

        public unsafe LSTRING* message;

        /// <summary>
        /// Converts LSTRING to a proper string message.
        /// </summary>
        /// <returns></returns>
        public unsafe string GetMessage() {
            int len = message->len;
            byte* charPtr = &(message->c);

            StringBuilder builder = new StringBuilder(len);

            for (int i = 0; i < len - 1; i++) {
                //byte converted to char, charPtr increments to point to next char
                builder.Append((char)*(charPtr + i));
            }

            return builder.ToString();

        }

        /// <summary>
        /// Since all messages sent to Eyelink is appended by the SessionTrigger, 
        /// it is safe to get the second last char and convert it into a SessionTrigger.
        /// 
        /// eg.
        /// MSG	1686949 Trigger Version 84\0
        /// MSG	1689040	Start Trial 14\0
        /// 
        /// where '\0' is a null character
        /// </summary>
        /// <returns></returns>
        public unsafe SessionTrigger GetSessionTrigger() {
            //check that the message is a valid SessionTrigger.
            if (!IsSessionTrigger(GetMessage())) {
                return SessionTrigger.NoTrigger;
            }

            //get index of trigger
            int index = (message->len) - 3;

            //derive pointer of trigger
            byte* triggerPtr = (&(message->c)) + index;

            //convert byte to char and subtract with the char '0' to get actual number.
            int trigger = ((char)(*triggerPtr)) - '0';

            // multipled by 10 due to the trigger being in the tens place
            return (SessionTrigger)(trigger * 10);
        }


        public static bool IsSessionTrigger(string message) {
            //pattern describes only 2 words and a 2 digit number
            const string pattern = @"^(\w+\s){2}\d{2}$";

            return Regex.IsMatch(message.Trim(), pattern) || message.Contains("Timeout");
        }
    }

    public struct LSTRING {
        /// <summary>
        /// length of message
        /// </summary>
        public Int16 len;

        /// <summary>
        /// First character of the message,
        /// 
        /// In EDF file, char is stored as 8 bits ascii characters and in C#, 
        /// char is 16 bits so byte is used.
        /// </summary>
        public byte c;
    }

    public class FEvent : AllFloatData {
        public readonly string message;
        public readonly SessionTrigger trigger;

        public FEvent(FEVENT ev, DataTypes dataType) : base(dataType, ev.sttime) {
            message = ev.GetMessage();
            trigger = ev.GetSessionTrigger();
        }

        public FEvent(uint time, string message, DataTypes dataType) : base(dataType, time) {
            this.message = message;

            trigger = GetSessionTrigger(message);
        }

        /// <summary>
        /// Since all messages sent to Eyelink is appended by the SessionTrigger, 
        /// it is safe to get the second last char and convert it into a SessionTrigger.
        /// 
        /// eg.
        /// MSG	1686949 Trigger Version 84
        /// MSG	1689040	Start Trial 14
        /// 
        /// </summary>
        /// <returns></returns>
        private unsafe SessionTrigger GetSessionTrigger(string message) {
            //check that the message is a valid SessionTrigger.
            if (!FEVENT.IsSessionTrigger(message)) {
                return SessionTrigger.NoTrigger;
            }

            //get index of trigger
            int index = message.Length - 2;

            //convert byte to char and subtract with the char '0' to get actual number.
            int trigger = message[index] - '0';

            // multipled by 10 due to the trigger being in the tens place
            return (SessionTrigger)(trigger * 10);
        }

        public override string ToString() {
            return $"{dataType} @ {time} | {trigger} | {message}";
        }
    }
}
