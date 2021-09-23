using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static SerialComms() => _settings = CTecUtil.Registry.ReadSerialPortSettings();


        private static SerialPort _port;


        public static byte AckByte { get; set; }
        public static byte NakByte { get; set; }


        public class SerialPortSettings
        {
            public string PortName { get; set; }
            public int BaudRate { get; set; }
            public Handshake Handshake { get; set; }
            public Parity Parity { get; set; }
            public int DataBits { get; set; }
            public StopBits StopBits { get; set; }
            public int ReadTimeout { get; set; }
            public int WriteTimeout { get; set; }
        }


        private static SerialPortSettings _settings = new();
        public static SerialPortSettings Settings { get => _settings; }


        public static string PortName { get => Settings.PortName; set => Settings.PortName = value; }
        public static int BaudRate { get => Settings.BaudRate; set => Settings.BaudRate = value; }
        public static Handshake Handshake { get => Settings.Handshake; set => Settings.Handshake = value; }
        public static Parity Parity { get => Settings.Parity; set => Settings.Parity = value; }
        public static int DataBits { get => Settings.DataBits; set => Settings.DataBits = value; }
        public static StopBits StopBits { get => Settings.StopBits; set => Settings.StopBits = value; }
        public static int ReadTimeout { get => Settings.ReadTimeout; set => Settings.ReadTimeout = value; }
        public static int WriteTimeout { get => Settings.WriteTimeout; set => Settings.WriteTimeout = value; }


        public delegate void ReceivedDataHandler(byte[] incomingData);


        public static bool Close()
        {
            try
            {
                if (_port?.IsOpen == true)
                    _port?.Close();
                _port?.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        ~SerialComms()
        {
            _port?.Close();
            _port?.Dispose();
        }



        public static List<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();


        private class Command
        {
            /// <summary>The command data.</summary>
            public byte[] CommandData { get; set; }

            /// <summary>Handler to which the response will be sent.</summary>
            public ReceivedDataHandler DataReceiver { get; set; }
            
            public bool Sent { get; set; }
        }


        private static Queue<Command> _commandQueue = new();


        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="dataReceiver">Handler to which the response will be sent.</param>
        public static void EnqueueCommand(byte[] commandData, ReceivedDataHandler dataReceiver)
            => _commandQueue.Enqueue(new() { CommandData = commandData, DataReceiver = dataReceiver });


        public static void StartSendingCommandQueue() => SendNextCommandInQueue();


        private static void SendNextCommandInQueue()
        {
            if (_commandQueue.Count > 0)
                SendData(_commandQueue.Peek());
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
            if (data.Length == 0) 
                return false;
            int checksumCalc = 0;
            for (int i = 0; i < data.Length - 1; i++)
                checksumCalc += data[i];
            return (byte)(checksumCalc & 0xff) == data[data.Length - 1];
        }


        private static bool SendData(Command command)
        {
            try
            {
                if (_port is null)
                    _port = newSerialPort();

                if (!_port.IsOpen)
                    _port.Open();

                _port.Write(command.CommandData, 0, command.CommandData.Length);

                return command.Sent = true;
            }
            catch (Exception ex)
            {
                error(Cultures.Resources.Error_Serial_Port, ex);
            }

            return false;
        }


        /// <summary>Use of this delegate allows house style message box to be used</summary>
        public delegate void ErrorMessageHandler(string message);

        /// <summary>Set this to provide house style message box for any error messages generated during serial comms</summary>
        public static ErrorMessageHandler ShowErrorMessage;


        private static void error(string message, Exception ex) => ShowErrorMessage?.Invoke(message + "\n\n'" + ex.Message + "'");


        /// <summary>
        /// Returns a new serial port Initialised with the current PortName, BaudRate, etc. properties.
        /// </summary>
        private static SerialPort newSerialPort()
        {
            try
            {
                var port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                port.ReadTimeout  = ReadTimeout;
                port.WriteTimeout = WriteTimeout;

                var available = GetAvailablePorts();
                if (available.Count > 0 && !available.Contains(port.PortName))
                    port.PortName = available[0];

                port.DataReceived += dataReceived;
                return port;
            }
            catch (Exception ex)
            {
                error(Cultures.Resources.Error_Serial_Port, ex);
            }
            return null;
        }


        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var incoming = readIncomingData(sender as SerialPort);

            if (incoming is not null && _commandQueue.Count > 0)
            {
//                OnReceiveData?.Invoke(incoming);
                _commandQueue.Peek().DataReceiver?.Invoke(incoming);

                //remove just-handled command from queue
                _commandQueue.Dequeue();
            }

            //send next command or finish
            if (_commandQueue.Count > 0)
            {
                SendNextCommandInQueue();
            }
            else
            {
                _port?.Close();
                _port?.Dispose();
            }

            //}
        }


        private static byte[] readIncomingData(SerialPort sender)
        {
            try
            {
                //wait for buffer
                Thread.Sleep(30);
                int bytes = sender.BytesToRead, newBytes;
                do
                {
                    Thread.Sleep(30);
                    if ((newBytes = sender.BytesToRead) > 0 && newBytes == bytes)
                        break;
                    bytes = newBytes;
                } while (true);

                byte[] buffer = new byte[sender.BytesToRead];
                sender.Read(buffer, 0, sender.BytesToRead);
                //return buffer;
                return CheckChecksum(buffer) ? buffer : null;



                //Thread.Sleep(30);
                //if (sender.BytesToRead == 0)
                //    return null;

                //byte[] header = new byte[2];

                ////read first byte & check for ack/nak
                //sender.Read(header, 0, 1);
                //if (header[0] == AckByte) return new byte[] { AckByte };
                //if (header[0] == NakByte) return new byte[] { NakByte };

                ////first byte is command code; next read payload length
                //while (sender.BytesToRead == 0) Thread.Sleep(5);
                //sender.Read(header, 1, 1);
                //var payloadLength = header[1];
                //var checkSumLength = 1;

                //int bytesRead = header.Length;
                //var buffer = new byte[payloadLength + bytesRead + checkSumLength];
                //buffer[0] = header[0];
                //buffer[1] = header[1];

                //while (bytesRead < payloadLength + checkSumLength)
                //{
                //    while (sender.BytesToRead == 0) Thread.Sleep(5);
                //    int newBytes = Math.Min(sender.BytesToRead, header.Length + payloadLength + checkSumLength - bytesRead);
                //    sender.Read(buffer, bytesRead, newBytes);
                //    bytesRead += newBytes;
                //}

                //int r;
                //while ((r = sender.BytesToRead) > 0) { Thread.Sleep(5); var discard = new byte[r]; sender.Read(discard, 0, r); }

                //return CheckChecksum(buffer) ? buffer : null;
            }
            catch (Exception ex)
            {
                error(Cultures.Resources.Error_Reading_Incoming_Data, ex);
                return null;
            }
        }

    }
}
