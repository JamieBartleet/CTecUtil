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
    public partial class SerialComms
    {
        static SerialComms() => _settings = CTecUtil.Registry.ReadSerialPortSettings();


        private static SerialPort     _port;
        private static Queue<Command> _commandQueue = new();

        
        private static int _totalCommandsToSend;
        public delegate void ProgressMaxSetter(int maxValue);
        public delegate void ProgressValueUpdater(int value);

        public static SerialComms.ProgressMaxSetter    SetProgressMaxValue;
        public static SerialComms.ProgressValueUpdater UpdateProgressValue;


        public delegate void ReceivedDataHandler(byte[] incomingData, int index = -1);


        public static byte AckByte { get; set; }
        public static byte NakByte { get; set; }


        private static SerialPortSettings _settings = new();
        public static SerialPortSettings Settings { get => _settings; }


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

        
        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="dataReceiver">Handler to which the response will be sent.</param>
        /// <param name="index">(Optional) the index of the item requested - for the case where the index is not included in the response data (e.g. devices).</param>
        public static void EnqueueCommand(byte[] commandData, ReceivedDataHandler dataReceiver, int? index = null)
            => _commandQueue.Enqueue(new() { CommandData = commandData, DataReceiver = dataReceiver, Index = index });


        public static void StartSendingCommandQueue()
        {
            SetProgressMaxValue?.Invoke(_totalCommandsToSend = _commandQueue.Count);
            ShowProgressBarWindow();
            UpdateProgressValue?.Invoke(0);
            SendNextCommandInQueue();
        }

        private static void SendNextCommandInQueue() => SendData(_commandQueue.Peek());


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
            if (command != null)
            {
                try
                {
                    if (_port is null)
                        _port = newSerialPort();

                    if (!_port.IsOpen)
                        _port.Open();

                    _port.DiscardOutBuffer();
                    _port.Write(command.CommandData, 0, command.CommandData.Length);

                    return true;
                }
                catch (Exception ex)
                {
                    error(Cultures.Resources.Error_Serial_Port, ex);
                }
            }

            return false;
        }


        /// <summary>Use of this delegate allows house style message box to be used</summary>
        public delegate void ErrorMessageHandler(string message);

        /// <summary>Set this to provide house style message box for any error messages generated during serial comms</summary>
        public static ErrorMessageHandler ShowErrorMessage;


        public static void error(string message, Exception ex) => ShowErrorMessage?.Invoke(message + "\n\n'" + ex.Message + "'");


        /// <summary>
        /// Returns a new serial port Initialised with the current PortName, BaudRate, etc. properties.
        /// </summary>
        private static SerialPort newSerialPort()
        {
            try
            {
                var port = new SerialPort(Settings.PortName, Settings.BaudRate, Settings.Parity, Settings.DataBits, Settings.StopBits);
                port.ReadTimeout  = Settings.ReadTimeout;
                port.WriteTimeout = Settings.WriteTimeout;

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


        private static async void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            if (port == null || port.BytesToRead == 0)
                return;

            var incoming = readIncomingData(port);

            if (incoming is not null && _commandQueue.Count > 0)
            {
                var cmd = _commandQueue.Peek();
                if (cmd != null)
                {
                    cmd.Complete = true;

                    if (_commandQueue.Count > 0)
                        _commandQueue.Dequeue();

                    //send response to data receiver
                    await Task.Run(new Action(() =>
                    {
                        if (cmd.Index != null)
                            cmd.DataReceiver?.Invoke(incoming, cmd.Index.Value);
                        else
                            cmd.DataReceiver?.Invoke(incoming);
                    }));

                    UpdateProgressValue?.Invoke(_totalCommandsToSend - _commandQueue.Count);
                }
            }

            //send next command, if any
            if (_commandQueue.Count > 0)
                SendNextCommandInQueue();
        }


        private static byte[] readIncomingData(SerialPort sender)
        {
            CommsTimer timer = new();
            
            try
            {
                ////wait for buffering [sometimes dataReceived() is called by the port when BytesToRead is still zero]
                //Thread.Sleep(30);

                
                timer.Start(5000);

                //wait for buffering [sometimes dataReceived() is called by the port when BytesToRead is still zero]
                while (sender.BytesToRead == 0)
                {
                    Thread.Sleep(40);
                    if (timer.TimedOut)
                        throw new TimeoutException();
                }

                //read first byte: either Ack/Nak or the command ID
                byte[] header = new byte[2];
                sender.Read(header, 0, 1);
                if (header[0] == AckByte || header[0] == NakByte)
                    return new byte[] { header[0] };

                //read payload length byte
                while (sender.BytesToRead == 0)
                {
                    Thread.Sleep(40);
                    if (timer.TimedOut)
                        throw new TimeoutException();
                }

                sender.Read(header, 1, 1);
                var payloadLength = header[1];

                //now we know how many more bytes to expect - i.e. header + payloadLength + 1 byte for checksum
                byte[] buffer = new byte[header.Length + payloadLength + 1];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);

                int offset = header.Length;
                while (offset < buffer.Length)
                {
                    while (sender.BytesToRead == 0)
                    {
                        Thread.Sleep(40);

                        if (timer.TimedOut)
                            throw new TimeoutException();
                    }

                    //Read payload & checksum
                    var bytes = Math.Min(sender.BytesToRead, buffer.Length - offset);
                    sender.Read(buffer, offset, bytes);
                    offset += bytes;
                }


                //int bytes = sender.BytesToRead, newBytes;
                //do
                //{
                //    Thread.Sleep(30);
                //    if ((newBytes = sender.BytesToRead) > 0 && newBytes == bytes)
                //        break;
                //    bytes = newBytes;

                //    if (timer.TimedOut)
                //        throw new TimeoutException();

                //} while (true);

                //sender.Read(buffer, 0, sender.BytesToRead);

                if (!CheckChecksum(buffer))
                    throw new Exception(Cultures.Resources.Error_Checksum_Fail);

                return buffer;
            }
            catch (TimeoutException ex)
            {
                error(Cultures.Resources.Error_Comms_Timeout, ex);
                return null;
            }
            catch (Exception ex)
            {
                error(Cultures.Resources.Error_Reading_Incoming_Data, ex);
                return null;
            }
            finally
            {
                timer.Stop();
                //timer.Dispose();
                timer = null;
                _port.DiscardInBuffer();
            }
        }

    }
}
