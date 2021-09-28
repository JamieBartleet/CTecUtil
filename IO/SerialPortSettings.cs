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
