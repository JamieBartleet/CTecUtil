using CTecUtil.Config;
using CTecUtil.UI;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace CTecUtil.IO
{
    public partial class SerialComms
    {
        static SerialComms()
        {
            //_progressBarWindow.OnCancel = CancelCommandQueue;
        }


        /// <summary>Status of the serial connection</summary>
        public enum ConnectionStatus
        {
            /// <summary>Comms status is unknown</summary>
            Unknown,

            /// <summary>Comms is disconnected</summary>
            Disconnected,

            /// <summary>Comms is in a listening state</summary>
            Listening,

            /// <summary>Comms is actively connected but panel data is read-only</summary>
            ConnectedReadOnly,

            /// <summary>Comms is actively connected, panel is writeable</summary>
            ConnectedWriteable,

            /// <summary>Comms is actively connected but panel firmware is not supported</summary>
            FirmwareNotSupported
        }


        /// <summary>Direction of the current data transfer, if any</summary>
        //public enum Direction
        //{
        //    /// <summary>No data transfer in progress</summary>
        //    Idle,
            
        //    /// <summary>Uploading to panel</summary>
        //    Upload,
        
        //    /// <summary>Downloading from panel</summary>
        //    Download
        //}


        /// <summary>Panel polling interval (ms)</summary>
        public static int PingTimerPeriod         { get; set; } = 2000;

        /// <summary>Timeout period (ms) during receipt of successive incoming data packets</summary>
        public static int IncomingDataTimerPeriod { get; set; } = 1250;
        
        /// <summary>Wait timeout (ms) for a response to a sent command</summary>
        public static int ResponseTimerPeriod     { get; set; } = 5500;


        public static List<string> GetPortNames()
        {
            var names = SerialPort.GetPortNames().ToList();
            names.Sort(COMparer);
            return names;
        }
        

        /// <summary>
        /// Sort COM port names sensibly, not alphabetically - i.e. so that COM3 is listed before COM20
        /// </summary>
        internal static int COMparer(string portName1, string portName2)
        {
            int n1, n2;
            if (portName1.StartsWith("COM") && int.TryParse(portName1.Substring(3), out n1)
             && portName2.StartsWith("COM") && int.TryParse(portName2.Substring(3), out n2))
                return n1.CompareTo(n2);
            
            return portName1.CompareTo(portName2);
        }


        public static string PortName
        {
            get => _port?.PortName;
            set
            {
                if (!string.IsNullOrEmpty(value) && Settings.PortName != value)
                {
                    Settings.PortName = value;

                    if (_port is null || _port.PortName != value)
                    {
                        if (_port is not null && _port.IsOpen)
                            _port.Close();
                        getNewSerialPort();
                        ApplicationConfig.SerialPortName = _port.PortName = value;
                    }
                }
            }
        }


        private static SerialPort _port;
        private static CommandQueue _commandQueue = new();

        /// <summary>Timer on response to SendData; the OnTimedOut event is used to notify the connection status</summary>
        private static CommsTimer _responseTimer = new(nameof(_responseTimer));

        private static Exception _lastException = null;

        
        public delegate bool ReceivedResponseDataHandler(byte[] incomingData, int? index);
        public delegate bool MultiIndexResponseDataHandler(byte[] incomingData, int? index1, int? index2);
        public delegate void ReceivedListenerDataHandler(byte[] incomingData);
        public delegate void ProgressMaxSetter(int maxValue);
        public delegate void ProgressValueUpdater(int value);

        public static ReceivedListenerDataHandler ListenerDataReceiver { get; set; }


        public static Window OwnerWindow { get; set; }


        private static SerialPortSettings _settings = null;
        public static SerialPortSettings Settings
        {
            get
            {
                //if (_settings is null) throw new NullReferenceException("SerialComms.Settings has not been initialised.");
                return _settings;
            }
            set
            {
                _settings = value;
                _responseTimer.OnTimedOut = responseTimerTimeout;
                _responseTimer.Start(ResponseTimerPeriod);
            }
        }


        /// <summary>Use of this delegate allows house style message box to be used</summary>
        public delegate void MessageHandler(string message);
        public delegate void Message2Handler(string message, string message2);

        /// <summary>Delegate called when a command subqueue's requests have been completed</summary>
        public delegate void SubqueueCompletedHandler();


        /// <summary>Set this to provide house style message box for any messages generated during serial comms</summary>
        public static MessageHandler ShowMessage { get; set; }

        /// <summary>Set this to provide house style message box for any error messages generated during serial comms</summary>
        public static MessageHandler ShowErrorMessage { get; set; }
        public static Message2Handler ShowErrorMessage2 { get; set; }


        public static byte AckByte { get; set; }
        public static byte NakByte { get; set; }


        //private static bool _listenerMode;

        /// <summary>
        /// Set to true when the EventLogViewer page is active so that the correct ping command is sent to the panel
        /// </summary>
        //public static bool ListenerMode
        //{
        //    get => _listenerMode;
        //    set
        //    {
        //        var changedMode = _listenerMode != value;
        //        _listenerMode = value;
        //        if (changedMode)
        //        {
        //            if (ListenerMode)
        //                _prevConnectionStatus = _connectionStatus;
        //            NotifyConnectionStatus?.Invoke(setConnectionStatus(_listenerMode ? ConnectionStatus.Listening : _prevConnectionStatus));
        //        }
        //    }
        //}


        public static bool TransferInProgress() => _commandQueue.TotalCommandCount > 0;


        #region ping
        public enum PingModes
        {
            NoPing,
            Polling,
            Listening
        }


        private static PingModes _pingMode;
        private static bool _pingStarted;

        public static PingModes PingMode
        {
            get => _pingMode;
            set
            {
                if (PingCommand is null)            throw new NotImplementedException("SerialComms.PingCommand has not been initialised");
                if (LoggingModePingCommand is null) throw new NotImplementedException("SerialComms.LoggingModePingCommand has not been initialised");
                if (CheckFirmwareCommand is null)   throw new NotImplementedException("SerialComms.CheckFirmwareCommand has not been initialised");

                var changedMode = _pingMode != value;

                if ((_pingMode = value) != PingModes.NoPing)
                {
                    if (!_pingStarted)
                    {
                        _pingTimer = new()
                        {
                            AutoReset = true,
                            Enabled = true,
                            Interval = PingTimerPeriod
                        };
                        _pingTimer.Elapsed += sendPing;
                        _pingStarted = true;
                    }
                }
                else
                {
                    _pingTimer?.Stop();
                    _pingStarted = false;
                }

                if (changedMode)
                {
                    if (_pingMode == PingModes.Listening)
                        _prevConnectionStatus = _connectionStatus;
                    notifyConnectionStatus(value == PingModes.Listening ? ConnectionStatus.Listening : _prevConnectionStatus);
                }
            }
        }

        public delegate bool FirmwareVersionNotifier(byte[] firmwareResponse);
        public delegate void ConnectionStatusNotifier(ConnectionStatus status);
        public delegate void DeviceProtocolNotifier(byte[] protocol);
        public static FirmwareVersionNotifier  NotifyFirmwareVersion { get; set; }
        public static ConnectionStatusNotifier NotifyConnectionStatus { get; set; }
        public static DeviceProtocolNotifier   NotifyDeviceProtocol { get; set; }

        /// <summary>Timer for pinging the panel every few seconds</summary>
        private static System.Timers.Timer _pingTimer;

        private static int              _disconnected         = 0;
        private static ConnectionStatus _connectionStatus     = ConnectionStatus.Unknown;
        private static ConnectionStatus _prevConnectionStatus = ConnectionStatus.Unknown;

        public delegate byte[] PingCommandGetter();
        public static PingCommandGetter PingCommand;
        public static PingCommandGetter LoggingModePingCommand;
        public static PingCommandGetter CheckFirmwareCommand;
        public static PingCommandGetter CheckWriteableCommand;
        public static PingCommandGetter CheckProtocolCommand;
            
        public static ConnectionStatus Status => _connectionStatus;
        
        /// <summary>True if the port is available</summary>
        public static bool IsConnected    => _connectionStatus switch { ConnectionStatus.Listening or ConnectionStatus.ConnectedWriteable or ConnectionStatus.ConnectedReadOnly => true, _ => false };

        /// <summary>True if the port has been explicitly disconnected, i.e. via Disconnect()</summary>
        public static bool IsDisconnected => _disconnected > 0;

        private static void notifyConnectionStatus(ConnectionStatus status) => NotifyConnectionStatus?.Invoke(_connectionStatus = status);


        private static void sendPing(object sender, ElapsedEventArgs e)
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            //only ping the panel if there is no active upload/download
            if (_commandQueue.TotalCommandCount == 0)
            {
                switch (PingMode)
                {
                    case PingModes.Polling:   SendData(new Command() { CommandData = PingCommand?.Invoke() }); break;
                    case PingModes.Listening: SendData(new Command() { CommandData = LoggingModePingCommand?.Invoke() }); break;
                }
            }
        }

        private static void sendFirmwareVersionCheck()
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            //only ping the panel if there is no active upload/download
            if (_commandQueue.SubqueueCount == 0)
                SendData(new Command() { CommandData = CheckFirmwareCommand() });
        }

        private static void sendWriteableCheck()
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            //only ping the panel if there is no active upload/download
            if (_commandQueue.SubqueueCount == 0)
                SendData(new Command() { CommandData = CheckWriteableCommand() });
        }

        private static void sendProtocolCheck()
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (CheckProtocolCommand is null)
                return;

            if (IsDisconnected)
                return;

            //only ping the panel if there is no active upload/download
            if (_commandQueue.SubqueueCount == 0)
                SendData(new Command() { CommandData = CheckProtocolCommand() });
        }
        #endregion

        
        public static bool IsIdle => _commandQueue is null || _commandQueue.TotalCommandCount == 0;


        #region command queue
        public delegate void OnFinishedHandler(bool wasCompleted);
        public static OnFinishedHandler OnFinish { get; set; }
        //private static bool _transferInProgress;
        private static bool _queueWasCompleted;


        /// <summary>
        /// Initialise new set of command command queues with the given process name.
        /// </summary>
        /// <param name="operationName">Name of the process to be displayed in the progress bar window.<br/>
        /// E.g. 'Downloading from panel...'</param>
        public static void InitCommandQueue(string operationName) => _commandQueue = new() { OperationDesc = operationName };


        /// <summary>
        /// Add a new command subqueue to the set
        /// </summary>
        /// <param name="direction">Comms direction (Up/Down).</param>
        /// <param name="name">Name of the command queue, to be displayed in the progress bar window.</param>
        public static void AddNewCommandSubqueue(CommsDirection direction, string name, SubqueueCompletedHandler onCompletion) => _commandQueue.AddSubqueue(new CommandSubqueue(direction, onCompletion) { Name = name });


        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="dataReceiver">Handler to which the response will be sent.</param>
        public static void EnqueueCommand(byte[] commandData, ReceivedResponseDataHandler dataReceiver = null)                           => _commandQueue.Enqueue(new Command() { CommandData = commandData, DataReceiver = dataReceiver });
        public static void EnqueueCommand(byte[] commandData, int index, ReceivedResponseDataHandler dataReceiver = null)                => _commandQueue.Enqueue(new Command() { CommandData = commandData, Index = index, DataReceiver = dataReceiver });
        public static void EnqueueCommand(byte[] commandData, int index, int? index2, MultiIndexResponseDataHandler dataReceiver = null) => _commandQueue.Enqueue(new Command() { CommandData = commandData, Index = index, Index2 = index2, DataReceiver2 = dataReceiver });


        public static void StartSendingCommandQueue(Action onStart, OnFinishedHandler onEnd)
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            //_transferInProgress = true;
            _queueWasCompleted = false;

            CommsCommandLog = new(Cultures.Resources.Log_Comms_Comands, _commandQueue.Direction);

            ShowProgressBarWindow(onStart, onEnd);
        }


        public static Log CommsCommandLog { get; private set; }


        public static void CancelCommandQueue() => _commandQueue?.Clear();
        public static void CancelCurrentQueue()
        {
            try
            {
                //subtract the discarded queue's items from the total
                Application.Current.Dispatcher.Invoke(new Action(() => { if (_progressBarWindow is not null) _progressBarWindow.ProgressBarOverallMax -= _commandQueue.CommandsInCurrentSubqueue; }));
            }
            catch { };

            _commandQueue?.CancelCurrentQueue();
        }


        private static void sendNextCommandInQueue()
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

Debug.WriteLine("SerialComms.sendNextCommandInQueue()");
            _progressOverall++;
            _progressWithinSubqueue++;

            if (_commandQueue.TotalCommandCount > 0)
                SendData(_commandQueue.Peek());
            else
                _queueWasCompleted = true;
        }


        private static void resendCommand()
        {
Debug.WriteLine("SerialComms.resendCommand()");
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            var cmd = _commandQueue.Peek();
            if (cmd is not null)
            {
                if (cmd.Tries > 5)
                {
                    //the number of retries on the current command is excessive
                    // - report the last-noted exception (if any)
                    if (_lastException is not null)
                    {
                        if (_lastException is TimeoutException)
                            error(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, _lastException);
                        else if (_lastException is FormatException)
                            error(Cultures.Resources.Error_Checksum_Fail, _lastException);
                        else
                            error(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Error_Uploading_Data : Cultures.Resources.Error_Downloading_Data, _lastException);
                    }
                    else
                        error(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Error_Upload_Retries : Cultures.Resources.Error_Download_Retries);
                }
                else
                {
                    Debug.WriteLine("resendCommand() - index=" + cmd.Index);
                    if (_port is not null & _port.IsOpen)
                    {
                        _port?.DiscardOutBuffer();
                        _port?.DiscardInBuffer();
                    }
                    SendData(cmd);
                }
            }
        }
        #endregion


        #region send/receive
        private static object _portLock = new();

        internal static void SendData(Command command)
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

            lock (_portLock)
            {
                try
                {
                    _lastException = null;

                    if (command is not null && command.CommandData is not null)
                    {
                        command.Tries++;
Debug.WriteLine("SerialComms.SendData() #" + command.Tries + " " + ByteArrayProcessing.ByteArrayToHexString(command.CommandData));

                        try
                        {
                            //check & open port if required
                            openPort();
                            _port?.Write(command.CommandData, 0, command.CommandData.Length);
                        }
                        catch (Exception ex)
                        {
                            error(Cultures.Resources.Error_Serial_Port, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("  **Exception** (SendData) " + ex.Message);
                }
            }
        }


        /// <summary>
        /// called by port on reception of data
        /// </summary>
        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_disconnected == 1)
            {
                doDisconnect();
                return;
            }

            if (IsDisconnected)
                return;

//Debug.WriteLine("SerialComms.dataReceived()");
            //lock (_portLock)
            {
                try
                {
                    if (sender is not SerialPort port)
                        return;

                    _responseTimer.Start(ResponseTimerPeriod);

                    if (PingMode == PingModes.Listening)
                        listenerDataReceived(port);
                    else
                        responseDataReceived(port);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("  **Exception** (dataReceived) " + ex.Message);
                }
            }
        }

        public static void errorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            Debug.WriteLine("!!!! errorReceived(" + e.ToString() + ") !!!!");
            resendCommand();
        }


        private static async void responseDataReceived(SerialPort port)
        {
            try
            {
                var incoming = readIncomingResponse(port);

                if (isPingResponse(incoming))
                {
                    //Debug.WriteLine("SerialComms.responseDataReceived() - ping response");
                    
                    //status is one of the Connected statuses: if not already thought to be writeable set it to read-only
                    if (_connectionStatus != ConnectionStatus.ConnectedWriteable)
                        notifyConnectionStatus(ConnectionStatus.ConnectedReadOnly);

                    //panel responded to ping, so request its firmware version
                    //...only if there is no active upload/download
                    if (_commandQueue.TotalCommandCount == 0)
                        sendWriteableCheck();
                }
                else if (isCheckWriteableResponse(incoming))
                {
                    //Debug.WriteLine("SerialComms.responseDataReceived() - check writeable response");
                    
                    //read-only response received?
                    //...only if there is no active upload/download
                    if (_commandQueue.TotalCommandCount == 0)
                    {
                        var readOnly = incoming.Length > 2 && incoming[2] == 0;
                        notifyConnectionStatus(readOnly ? ConnectionStatus.ConnectedReadOnly : ConnectionStatus.ConnectedWriteable);
                        sendFirmwareVersionCheck();
                    }
                }
                else if (isCheckFirmwareResponse(incoming))
                {
                    //Debug.WriteLine("SerialComms.responseDataReceived() - check firmware response");
                    
                    //panel responded to ping, so notify the version number and request its read-only status
                    //...only if there is no active upload/download
                    if (_commandQueue.TotalCommandCount == 0)
                    {
                        if (!NotifyFirmwareVersion?.Invoke(incoming) ?? true)
                            notifyConnectionStatus(ConnectionStatus.FirmwareNotSupported);
                        else
                            sendProtocolCheck();
                    }
                }
                else if (isCheckProtocolResponse(incoming))
                {
                    //Debug.WriteLine("SerialComms.responseDataReceived() - check protocol response");
                    
                    //do this only if there is no active upload/download
                    if (_commandQueue.TotalCommandCount == 0)
                        NotifyDeviceProtocol?.Invoke(incoming);
                }
                else if (isNak(incoming))
                {
                    Debug.WriteLine("SerialComms.responseDataReceived() - NAK");
                    resendCommand();
                }
                else if (isAck(incoming))
                {
                    Debug.WriteLine("SerialComms.responseDataReceived() - ACK");

                    if (_commandQueue.Dequeue())
                    {
                        //new queue - reset the count
                        _progressWithinSubqueue = 0;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (_progressBarWindow is not null)
                                _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.InitialCommandsInCurrentSubqueue;

                        }), DispatcherPriority.Normal);
                    }
                    sendNextCommandInQueue();
                }
                else if (_commandQueue.TotalCommandCount > 0)
                {
//Debug.WriteLine("SerialComms.responseDataReceived() - something...");
                    var ok = false;
                    
                    notifyConnectionStatus(_connectionStatus);
                    var cmd = _commandQueue.Peek();

                    if (cmd is not null)
                    {
//Debug.WriteLine("SerialComms.responseDataReceived() - " + _commandQueue.CurrentSubqueueName + " - " + cmd.Index + ": " + ByteArrayProcessing.ByteArrayToHexString(cmd.CommandData));
                        var savedQueueId = _commandQueue.Id;

                        if (incoming is not null)
                        {
                            //send response to data receiver
                            await Task.Run(new Action(() =>
                            {
                                if ((cmd = _commandQueue.Peek()) is not null)
                                {
                                    try
                                    {
                                        if (cmd.DataReceiver2 is not null)
                                            ok = cmd.DataReceiver2?.Invoke(incoming, cmd.Index, cmd.Index2) == true;
                                        else
                                            ok = cmd.DataReceiver?.Invoke(incoming, cmd.Index) == true;
                                    }
                                    catch (Exception ex)
                                    {
                                        CommsCommandLog.AddError(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Error_Uploading_Data : Cultures.Resources.Error_Downloading_Data, ex);
                                    }
                                }
                            }));
                        }

//Debug.WriteLine("SerialComms.responseDataReceived() - ok=" + ok);

                        if (ok)
                        {
                            //NB: cmd.DataReceiver may have started a new command queue, so check the Id before dequeueing
                            if (_commandQueue.Id == savedQueueId && _commandQueue.Dequeue(cmd))
                            {
                                //new queue - reset the count
                                _progressWithinSubqueue = 0;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    if (_progressBarWindow is not null)
                                        _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.InitialCommandsInCurrentSubqueue;

                                }), DispatcherPriority.Normal);
                            }
                        }

                        if (ok)
                            sendNextCommandInQueue();
                        else
                            resendCommand();
                    }
                }
            }
            catch (FormatException ex)
            {
                //checksum fail
                Debug.WriteLine("  **FormatException** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                resendCommand();
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine("  **TimeoutException** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                resendCommand();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                resendCommand();
            }
        }


        private static byte[] readIncomingResponse(SerialPort port)
        {
//Debug.WriteLine("SerialComms.readIncomingResponse()");
            try
            {
                var timeout = DateTime.Now.AddMilliseconds(IncomingDataTimerPeriod);

                //wait data to start appearing - note: SerialPort.DataReceived is often called by the port when BytesToRead is still zero
                while (port.BytesToRead == 0)
                {
                    Thread.Sleep(10);
                    if (DateTime.Now > timeout)
                        throw new TimeoutException("");
                }

                //read first byte: either Ack/Nak or the command ID
                byte[] header = new byte[2];
                port.Read(header, 0, 1);

                if (isAck(header[0]) || isNak(header[0]))
                    return new byte[] { header[0] };

                //read payload length byte
                while (port.BytesToRead == 0)
                {
                    Thread.Sleep(5);
                    if (DateTime.Now > timeout)
                        throw new TimeoutException("");
                }

                port.Read(header, 1, 1);
                var payloadLength = header[1];

                //now we know how many more bytes to expect - i.e. header + payloadLength + 1 byte for checksum
                byte[] buffer = new byte[header.Length + payloadLength + 1];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);

                int offset = header.Length;
                while (offset < buffer.Length)
                {
                    while (port.BytesToRead == 0)
                    {
                        Thread.Sleep(20);
                        if (DateTime.Now > timeout)
                            throw new TimeoutException("");
                    }

                    //Read payload & checksum
                    var bytes = Math.Min(port.BytesToRead, buffer.Length - offset);
                    if (bytes > 0)
                        port.Read(buffer, offset, bytes);
                    offset += bytes;
                }

                if (!checkChecksum(buffer))
                    throw new FormatException("CTecUtil.IO.SerialComms.readIncomingResponse(): checksum error");

                return buffer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (readIncomingResponse) " + ex.Message);
                _lastException = ex;
                return null;
            }
            finally
            {
                //port.DiscardInBuffer();
            }
        }


        private static async void listenerDataReceived(SerialPort port)
        {
//Debug.WriteLine("SerialComms.listenerDataReceived()");
            try
            {
                notifyConnectionStatus(ConnectionStatus.Listening);

                var incoming = readIncomingListenerData(port);

                if (incoming is not null && ListenerDataReceiver is not null)
                {
                    //send response to data receiver
                    await Task.Run(new Action(() =>
                    {
                        ListenerDataReceiver?.Invoke(incoming);
                    }));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (ListenerDataReceived) " + ex.Message);
                _lastException = ex;
                resendCommand();
            }
        }


        private static byte[] readIncomingListenerData(SerialPort port)
        {
//Debug.WriteLine("SerialComms.readIncomingListenerData()");
            try
            {
                var timeout = DateTime.Now.AddMilliseconds(IncomingDataTimerPeriod);

                //wait for buffering [sometimes SerialPort.DataReceived is called by the port when BytesToRead is still zero]
                while (port.BytesToRead == 0)
                {
                    if (DateTime.Now > timeout)
                        throw new TimeoutException("");
                }

                Thread.Sleep(100);
                var numBytes = port.BytesToRead;
                byte[] buffer = new byte[numBytes];
                port.Read(buffer, 0, numBytes);

                return buffer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (readIncomingListenerData) " + ex.Message);
                return null;
            }
            finally
            {
                //port.DiscardInBuffer();
            }
        }

        
        private static bool isAck(byte[] data) => data is not null && data.Length > 0 && isAck(data[0]);
        private static bool isNak(byte[] data) => data is not null && data.Length > 0 && isNak(data[0]);
        private static bool isAck(byte data) => data == AckByte;
        private static bool isNak(byte data) => data == NakByte;

        //private static bool isPingResponse(byte[] data)           => data is not null && data.Length > 0 && data[0] == _pingCommand?[_pingCommand.Length - 3];
        //private static bool isCheckFirmwareResponse(byte[] data)  => data is not null && _checkFirmwareVersionCommand is not null && data.Length > 0 && data[0] == _checkFirmwareVersionCommand[_checkFirmwareVersionCommand.Length - 3];
        //private static bool isCheckWriteableResponse(byte[] data) => data is not null && _checkWriteableCommand is not null && data.Length > 0 && data[0] == _checkWriteableCommand[_checkWriteableCommand.Length - 3];
        private static bool isPingResponse(byte[] data)           => data is not null && data.Length > 0 && data[0] == PingCommand?.Invoke()?[^3];
        private static bool isCheckFirmwareResponse(byte[] data)  => data is not null && data.Length > 0 && data[0] == CheckFirmwareCommand?.Invoke()?[^3];
        private static bool isCheckWriteableResponse(byte[] data) => data is not null && data.Length > 0 && data[0] == CheckWriteableCommand?.Invoke()?[^3];
        private static bool isCheckProtocolResponse(byte[] data)  => data is not null && data.Length > 0 && data[0] == CheckProtocolCommand?.Invoke()?[^3];
        #endregion


        private static void responseTimerTimeout()
        {
            if (_connectionStatus != ConnectionStatus.Listening)
            {
                //try again...
                if (_commandQueue.TotalCommandCount > 0)
                    resendCommand();
                else
                    notifyConnectionStatus(ConnectionStatus.Disconnected);
            }

            _responseTimer.Start(ResponseTimerPeriod);
        }


        /// <summary>
        /// Returns the checksum for the given data packet.  Takes into account 
        /// </summary>
        /// <param name="packet">The data for which to calculate the checksum</param>
        /// <param name="outgoing"></param>
        /// <param name="checkOnly">If true calculates the checksum excluding the final byte</param>
        /// <returns></returns>
        public static byte CalcChecksum(byte[] packet, bool outgoing = false, bool checkOnly = false)
        {
            var startByte = outgoing ? 1 : 0;
            var checkLength = checkOnly ? packet.Length - 1 : packet.Length;
            int checksumCalc = 0;
            for (int i = startByte; i < checkLength; i++)
                checksumCalc += packet[i];
            return (byte)(checksumCalc & 0xff);
        }


        private static bool checkChecksum(byte[] data) => data.Length > 0 && CalcChecksum(data, false, true) == data[^1];


        /// <summary>
        /// Cancel queued commands and notify user
        /// </summary>
        private static void error(string message, Exception ex = null)
        {
            Debug.WriteLine("SerialComms.error(" + message + ", " + ex?.Message + ")");

            //check queue - avoids erroring on ping fail
            var showError = _commandQueue.TotalCommandCount > 0 || string.IsNullOrEmpty(ex?.Message);
            CancelCommandQueue();
            notifyConnectionStatus(ConnectionStatus.Disconnected);
            if (showError)
                ShowErrorMessage2?.Invoke(message, ex?.Message);
        }


        #region port
        private static bool openPort()
        {
            if (_port is null)
            {
                getNewSerialPort();
            }

            if (_port?.IsOpen == false)
            {
Debug.WriteLine("SerialComms.openPort()");
                try
                { _port?.Open(); } catch { }
            }

            return _port?.IsOpen == true;
        }

        /// <summary>
        /// Discard any pending commands and close the serial port
        /// </summary>
        public static bool ClosePort()
        {
Debug.WriteLine("SerialComms.ClosePort()");
            try
            {
                CancelCommandQueue();

                lock (_portLock)
                {
                    if (_port?.IsOpen == true)
                    {
                        GC.SuppressFinalize(_port.BaseStream);
                        try
                        {
                            _port.DiscardInBuffer();
                            _port.DiscardOutBuffer();
                            _port.Close();
                        }
                        catch { }

                        //pause to allow port's internal threads to terminate per MS documentation (though they don't specify a time)
                        // - otherwise we would typically get an UnauthorizedAccessException if we try to open it immediately after closing
                        Thread.Sleep(666);
                        _port = null;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (ClosePort) " + ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Open the serial port
        /// </summary>
        public static bool Connect()
        {
            _disconnected = 0;
            return openPort();
        }


        /// <summary>
        /// Cancel any queued commands, close the serial port and stop further activity until Connect() is called
        /// </summary>
        public static void Disconnect()
        {
            _disconnected = 1;
            //if (ClosePort())
            //{
            //    _disconnected = 1;
            //    NotifyConnectionStatus?.Invoke(setConnectionStatus(ConnectionStatus.Disconnected));
            //    return true;
            //}
            //return false;
        }


        private static void doDisconnect()
        {
            if (ClosePort())
            {
                _disconnected = 2;
                notifyConnectionStatus(ConnectionStatus.Disconnected);
            }
        }


        /// <summary>
        /// Gets a list of serial ports available on the system
        /// </summary>
#if NET8_0_OR_GREATER
        public static List<string> GetAvailablePorts() => [.. SerialPort.GetPortNames()];
#else
        public static List<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();
#endif

        /// <summary>
        /// Gets a new serial port Initialised with the current PortName, BaudRate, etc. properties.
        /// </summary>
        private static SerialPort getNewSerialPort()
        {
Debug.WriteLine("SerialComms.getNewSerialPort()");
            try
            {
                ClosePort();

                if (Settings is null)
                    return null;

                _port = new SerialPort(Settings.PortName, Settings.BaudRate, Settings.Parity, Settings.DataBits, Settings.StopBits)
                {
                    ReadTimeout  = Settings.ReadTimeout,
                    WriteTimeout = Settings.WriteTimeout
                };

                //var available = GetAvailablePorts();
                //if (available.Count > 0 && !available.Contains(_port.PortName))
                //    _port.PortName = available[0];

                _port.DataReceived  += dataReceived;
                _port.ErrorReceived += errorReceived;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("  **Exception** (openNewSerialPort) " + ex.Message);
                error(Cultures.Resources.Error_Serial_Port, ex);
            }
            return null;
        }
#endregion


        #region progress bar
        private static ProgressBarWindow _progressBarWindow;
        private static int _progressOverall, _progressWithinSubqueue;

        private static Action _onStartProgress;
        private static OnFinishedHandler _onEndProgress;

        private const int _timeoutSeconds = 10;
        private const int _completionMessageTime = 500;


        internal static void ShowProgressBarWindow(Action onStart, OnFinishedHandler onEnd)
        {
            _onStartProgress = onStart;
            _onEndProgress   = onEnd;

            try
            {
                _progressOverall = _progressWithinSubqueue = 0;

                // Start thread to handle progressbar
                Thread _backgroundThread = new Thread(new ThreadStart(progressBarThread));
                _backgroundThread.Start();
            }
            catch (Exception ex)
            {
                error(ex.Message, ex);
            }
        }
        

        private static void progressBarThread()
        {
            string currentCommsDesc = _commandQueue.SubqueueNames?.Count > 0 ? _commandQueue.SubqueueNames?[0] : "";

            var startTime = DateTime.Now;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _onStartProgress?.Invoke();

                //launch the progress bar window
                _progressBarWindow = new();
                _progressBarWindow.ProgressBarLegend      = _commandQueue.OperationDesc;
                _progressBarWindow.SubqueueCount          = _commandQueue.SubqueueCount;
                _progressBarWindow.ProgressBarOverallMax  = _commandQueue.TotalCommandCount;
                _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.InitialCommandsInCurrentSubqueue;
                _progressBarWindow.OnCancel               = CancelCommandQueue;

                _progressBarWindow.Show(OwnerWindow);

            }), DispatcherPriority.Send);

Debug.WriteLine("progressBarThread() - start sending commands...");
            Application.Current.Dispatcher.Invoke(new Action(() => sendNextCommandInQueue()));

            var timeout = DateTime.Now.AddSeconds(_timeoutSeconds);
            int lastProgress = 0;

            while (true)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.InitialCommandsInCurrentSubqueue ));
                    if (_commandQueue.TotalCommandCount == 0)
                        break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("  **Exception** (progressBarThread) " + ex.Message);
                    continue;
                }

                //stop if progress hasn't changed for _timeoutSeconds secs
                if (DateTime.Now > timeout)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (_commandQueue.Direction != CommsDirection.Idle)
                            error(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, new TimeoutException("Timeout"));

                    }), DispatcherPriority.Send);
                    break;
                }

               // Debug.WriteLine("---progress: " + _progressOverall + " (" + lastProgress + ")");

                if (_progressOverall > lastProgress)
                {
                    lastProgress = _progressOverall;
                    timeout = DateTime.Now.AddSeconds(_timeoutSeconds);
                }

                //report progress
                Application.Current.Dispatcher.Invoke(new Action(() => _progressBarWindow.UpdateProgress(_commandQueue?.SubqueueNames, _progressOverall, _progressWithinSubqueue)), DispatcherPriority.Normal);
                Thread.Sleep(100);
            }

            //hide progress window if it hasn't already done it itself
            CancelCommandQueue();
            Application.Current.Dispatcher.Invoke(new Action(() => { 
                _progressBarWindow.Hide();
                _onEndProgress?.Invoke(_queueWasCompleted);
            }), DispatcherPriority.Normal);

            //if comms finishes rapidly the progressbar window may not have had time to be shown, so show a message
            if (DateTime.Now < startTime.AddMilliseconds(_completionMessageTime))
                ShowMessage?.Invoke(string.Format(_commandQueue.Direction == CommsDirection.Upload ? Cultures.Resources.Comms_x_Upload_Complete : Cultures.Resources.Comms_x_Download_Complete, currentCommsDesc));
        }

        #endregion
    }
}
