using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    public class SerialPortSettings
    {
        public SerialPortSettings()
        {
            //defaults
            BaudRate     = 9600;
            Handshake    = Handshake.None;
            Parity       = Parity.None;
            DataBits     = 8;
            StopBits     = StopBits.One;
            ReadTimeout  = 5000;
            WriteTimeout = 500;
        }

        public string    PortName { get; set; }
        public int       BaudRate { get; set; }
        public Handshake Handshake { get; set; }
        public Parity    Parity { get; set; }
        public int       DataBits { get; set; }
        public StopBits  StopBits { get; set; }
        public int       ReadTimeout { get; set; }
        public int       WriteTimeout { get; set; }
    }
}
