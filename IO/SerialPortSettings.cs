using CTecUtil.Config;
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
            ////defaults
            //BaudRate     = 9600;
            //Handshake    = Handshake.None;
            //Parity       = Parity.None;
            //DataBits     = 8;
            //StopBits     = StopBits.One;
            //ReadTimeout  = 5000;
            //WriteTimeout = 500;
        }

        private string   _portName;

        public string    PortName { get => _portName; set { _portName = value; ApplicationConfig.Save = true; } }

        // vvv the below settings are essentially readonly vvv

        public int       BaudRate { get; set; } = 9600;
        public Handshake Handshake { get; set; } = Handshake.None;
        public Parity    Parity { get; set; } = Parity.None;
        public int       DataBits { get; set; } = 8;
        public StopBits  StopBits { get; set; } = StopBits.One;
        public int       ReadTimeout { get; set; } = 5000;
        public int       WriteTimeout { get; set; } = 500;
    }
}
