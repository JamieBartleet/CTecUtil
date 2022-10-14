using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    internal class Command
    {
        internal Command() { }

        //internal Command(Command original)
        //{
        //    CommandData = (byte[])original.CommandData.Clone();
        //    Index = original.Index;
        //    Tries = original.Tries;
        //}


        internal int Index { get; set; }

        /// <summary>The command data</summary>
        internal byte[] CommandData { get; set; }

        internal int Tries { get; set; }


        /// <summary>Handler to which the response will be sent.</summary>
        internal SerialComms.ReceivedResponseDataHandler DataReceiver { get; set; }

        /// <summary>Returns the string representation of the CommandData in hexadecimal format</summary>
        public override string ToString() => ByteArrayProcessing.ByteArrayToHexString(CommandData);
    }
}
