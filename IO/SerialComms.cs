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
        private static SerialPort _serial = new();


        public static string    PortName     { get => _serial.PortName;     set { _serial.PortName = value; } }
        public static int       BaudRate     { get => _serial.BaudRate;     set { _serial.BaudRate = value; } }
        public static Handshake Handshake    { get => _serial.Handshake;    set { _serial.Handshake = value; } }
        public static Parity    Parity       { get => _serial.Parity;       set { _serial.Parity = value; } }
        public static int       DataBits     { get => _serial.DataBits;     set { _serial.DataBits = value; } }
        public static StopBits  StopBits     { get => _serial.StopBits;     set { _serial.StopBits = value; } }
        public static int       ReadTimeout  { get => _serial.ReadTimeout;  set { _serial.ReadTimeout = value; } }
        public static int       WriteTimeout { get => _serial.WriteTimeout; set { _serial.WriteTimeout = value; } }


        public delegate void ReceivedDataHandler(object sender, SerialDataReceivedEventArgs e);
        public static ReceivedDataHandler OnReceiveData;


        public static bool Open(out string errorMessage)
        {
            //if (!_initialised)
            //    init();

            errorMessage = "";

            if (_serial.IsOpen)
                return true;

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
