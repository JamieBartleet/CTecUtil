using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil.IO
{
    public class SerialComms
    {
        private static SerialPort _serial = newSerialPort();
        public static SerialPort Port { get => _serial; }


        public static string PortName
        {
            get => _serial.PortName;
            set
            {
                if (_serial.PortName != value)
                    _serial.Close();
                _serial.PortName = value;
            }
        }


        public static int       BaudRate     { get => _serial.BaudRate;     set { _serial.BaudRate = value; } }
        public static Handshake Handshake    { get => _serial.Handshake;    set { _serial.Handshake = value; } }
        public static Parity    Parity       { get => _serial.Parity;       set { _serial.Parity = value; } }
        public static int       DataBits     { get => _serial.DataBits;     set { _serial.DataBits = value; } }
        public static StopBits  StopBits     { get => _serial.StopBits;     set { _serial.StopBits = value; } }
        public static int       ReadTimeout  { get => _serial.ReadTimeout;  set { _serial.ReadTimeout = value; } }
        public static int       WriteTimeout { get => _serial.WriteTimeout; set { _serial.WriteTimeout = value; } }


        public delegate void ReceivedDataHandler(byte[] incomingData);
        public static ReceivedDataHandler OnReceiveData;


        public static bool Open(out string errorMessage)
        {
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


        
        public static List<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();

        
//        public delegate void ReceivedDataHandler(string data);

        private class Command
        {
            public byte[] CommandData { get; set; }
            public ReceivedDataHandler DataReceiver { get; set; }
            public bool   Sent { get; set; }
        }

        private static Queue<Command> _commandQueue = new();

        public static void EnqueueCommand(byte[] commandData, ReceivedDataHandler dataReceiver)
        {
            _commandQueue.Enqueue(new() { CommandData = commandData, DataReceiver = dataReceiver });
        }


        public static void SendCommandQueue()
        {
            if (_commandQueue.Count == 0)
                return;

            string errorMessage;
            var cmd = _commandQueue.Peek();
            if (!SendData(cmd, out errorMessage))
                error(errorMessage);
        }


        public static byte CalcChecksum(byte[] data, bool outgoing = false)
        {
            int checksumCalc = 0;
            for (int i = outgoing ? 1 : 0; i < data.Length; i++)
                checksumCalc += data[i];
            return (byte)(checksumCalc & 0xff);
        }


        public static bool CheckChecksum(byte[] data)
        {
            int checksumCalc = 0;
            for (int i = 0; i < data.Length - 1; i++)
                checksumCalc += data[i];
            return (byte)(checksumCalc & 0xff) == data[data.Length - 1];
        }


        private static bool SendData(Command command, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                if (!_serial.IsOpen)
                    _serial.Open();

                OnReceiveData = command.DataReceiver;
                command.Sent = true;
                _serial.Write(command.CommandData, 0, command.CommandData.Length);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
            }

            return false;
        }


        private static void error(string message) => MessageBox.Show("Comms error:\n\n" + message, "Panel Comms");


        private static SerialPort newSerialPort()
        {
            var port = new SerialPort();
            CTecUtil.Registry.ReadSerialPortSettings(port);
            var available = GetAvailablePorts();
            if (available.Count > 0)
                if (!available.Contains(port.PortName))
                    port.PortName = available[0] ?? port.PortName;
            port.DataReceived += dataReceived;
            return port;
        }


        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var incoming = readIncomingData();

            if (_commandQueue.Count > 0)
            {
                if (incoming[0] == _commandQueue.Peek().CommandData[1] && CheckChecksum(incoming))
                {                    
                    OnReceiveData?.Invoke(incoming);
                    _commandQueue.Dequeue();
                }

                //send next in queue
                SendCommandQueue();
            }
        }


        private static byte[] readIncomingData()
        {
            //wait for buffer
            int oldBytes = _serial.BytesToRead, newBytes;
            do
            {
                Thread.Sleep(30);
                if ((newBytes = _serial.BytesToRead) == oldBytes)
                    break;
                oldBytes = newBytes;
            } while (true);

            byte[] buffer = new byte[_serial.BytesToRead];
            _serial.Read(buffer, 0, _serial.BytesToRead);
            return buffer;
        }

    }
}
