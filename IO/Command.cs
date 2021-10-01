using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    public class Command
    {
        public delegate void ReceivedDataHandler(byte[] incomingData, int index = -1);


        /// <summary>The command data</summary>
        public byte[] CommandData { get; set; }

        public int? Index { get; set; }

        /// <summary>Handler to which the response will be sent.</summary>
        public ReceivedDataHandler DataReceiver { get; set; }


        public bool Complete { get; set; }
    }
}
