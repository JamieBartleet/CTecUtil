using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    public class SerialComms
    {
        private static bool _initialised;
        private static SerialPort _serial = new();

        private static void init()
        {
            _serial = new();
            _serial.Handshake = System.IO.Ports.Handshake.None;
            _serial.Parity = Parity.None;
            _serial.DataBits = 8;
            _serial.StopBits = StopBits.One;
            _serial.ReadTimeout = 5000;
            _serial.WriteTimeout = 500;
            _serial.DataReceived += new((s,e) => { OnReceiveData?.Invoke(s, e); });
            _initialised = true;
        }


        public delegate void ReceivedDataHandler(object sender, SerialDataReceivedEventArgs e);
        public static ReceivedDataHandler OnReceiveData;


        public static bool Open(string portName, int baud, out string errorMessage)
        {
            if (!_initialised)
                init();

            errorMessage = "";

            if (_serial.IsOpen)
                return true;

            _serial.PortName = portName;
            _serial.BaudRate = baud;

            try
            {
                _serial.Open();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }

            return false;
        }


        public static bool IsOpen { get => _serial?.IsOpen ?? false; }


        //public static bool SendData(string data, out string errorMessage) => SendData(Encoding.ASCII.GetBytes(data), out errorMessage);

        public static bool SendData(byte[] command, out string errorMessage)
        {
            errorMessage = "";

            if (!_serial.IsOpen)
                _serial.Open();

            try
            {
                _serial.Write(command, 0, command.Length);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }

            return false;
        }


        public static byte[] Read()
        {
            byte[] buffer = new byte[_serial.BytesToRead];
            _serial.Read(buffer, 0, _serial.BytesToRead);
            return buffer;
        }


        public static bool Close(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                if (_serial.IsOpen)
                    _serial?.Close();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }
            return false;
        }

    }
}
