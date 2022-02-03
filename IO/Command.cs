using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    public class Command
    {
        public Command() { }

        public Command(Command original)
        {
            CommandData = (byte[])original.CommandData.Clone();
            Tries = original.Tries;
        }


        /// <summary>The command data</summary>
        public byte[] CommandData { get; set; }

        public int Tries { get; set; }

        /// <summary>Handler to which the response will be sent.</summary>
        public SerialComms.ReceivedResponseDataHandler DataReceiver { get; set; }
        //public SerialComms.ReceivedResponseDataHandlerWithValidation DataReceiverWithValidation { get; set; }

        /// <summary>Returns the string representation of the CommandData in hexadecimal format</summary>
        public override string ToString() => Utils.ByteArrayToHexString(CommandData);
    }
}
