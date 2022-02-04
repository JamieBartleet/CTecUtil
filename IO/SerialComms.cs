using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using CTecUtil.UI;
using static CTecUtil.IO.CommsTimer;

namespace CTecUtil.IO
{
    public partial class SerialComms
    {
        static SerialComms()
        {
            _settings = Registry.ReadSerialPortSettings();
            _progressBarWindow.OnCancel = CancelCommandQueue;
            _responseTimer.OnTimedOut = new(() => { responseTimerTimeout(); });
            _responseTimer.Start(_responseTimerPeriod);
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

            /// <summary>Comms is actively connected panel is writeable</summary>
            ConnectedWriteable
        }


        //direction of the current data
        public enum Direction
        {
            /// <summary>No data transfer in progress</summary>
            Idle,
            
            /// <summary>Uploading to panel</summary>
            Upload,
        
            /// <summary>Downloading from panel</summary>
            Download
        }


        private const int _incomingDataTimerPeriod = 3000;
        private const int _responseTimerPeriod     = 7500;


        private static SerialPort _port;
        private static CommandQueue _commandQueue = new();

        /// <summary>Timer for reading data; the OnTimedOut event means incomplete data was received</summary>
        private static CommsTimer _incomingDataTimer = new();
        
        /// <summary>Timer on response to SendData; the OnTimedOut event is used to notify the connection status</summary>
        private static CommsTimer _responseTimer = new();

        private static Exception _lastException = null;

        
        public delegate bool ReceivedResponseDataHandler(byte[] incomingData);
        public delegate void ReceivedListenerDataHandler(byte[] incomingData);
        public delegate void ProgressMaxSetter(int maxValue);
        public delegate void ProgressValueUpdater(int value);

        public static ReceivedListenerDataHandler ListenerDataReceiver;


        public static Window OwnerWindow { get; set; }

        private static SerialPortSettings _settings;
        public static SerialPortSettings Settings { get => _settings; }

        /// <summary>Use of this delegate allows house style message box to be used</summary>
        public delegate void ErrorMessageHandler(string message);

        /// <summary>Delegate called when a command subqueue's requests have been completed</summary>
        public delegate void SubqueueCompletedHandler();


        /// <summary>Set this to provide house style message box for any error messages generated during serial comms</summary>
        public static ErrorMessageHandler ShowErrorMessage;


        public static byte AckByte { get; set; }
        public static byte NakByte { get; set; }


        private static bool _listenerMode;
        /// <summary>
        /// Set to true when the EventLogViewer page is active so that the correct ping command is sent to the panel
        /// </summary>
        public static bool ListenerMode { get => _listenerMode; set { NotifyConnectionStatus?.Invoke(_connectionStatus = (_listenerMode = value) ? ConnectionStatus.Listening : ConnectionStatus.Unknown); } }


        #region ping
        public delegate void ConnectionStatusNotifier(ConnectionStatus status);
        public static ConnectionStatusNotifier NotifyConnectionStatus;

        /// <summary>Timer for pinging the panel every few seconds</summary>
        private static System.Timers.Timer _pingTimer;

        private static ConnectionStatus _connectionStatus = ConnectionStatus.Unknown;
        private static byte[] _pingCommand;
        private static byte[] _checkWriteableCommand;

        public static void SetPingCommands(byte[] pingCommand, byte[] checkWriteableCommand = null)
        {
            var start = _pingCommand is null;
            _pingCommand = pingCommand;
            _checkWriteableCommand = checkWriteableCommand;

            if (start)
            {
                _pingTimer = new()
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = 3000
                };
                _pingTimer.Elapsed += new(sendPing);
            }
        }

        private static void sendPing(object sender, ElapsedEventArgs e)
        {
            //only ping the panel if there is no active upload/download
            if (_commandQueue.SubqueueCount == 0)
                SendData(new Command() { CommandData = _pingCommand });
        }

        private static void sendWriteableCheck()
        {
            //only ping the panel if there is no active upload/download
            if (_checkWriteableCommand is not null)
                if (_commandQueue.SubqueueCount == 0)
                    SendData(new Command() { CommandData = _checkWriteableCommand });
        }
        #endregion


        #region command queue
        public delegate void OnFinishedHandler(bool wasCompleted);
        public static OnFinishedHandler OnFinish;
        private static bool _queueWasCompleted;


        /// <summary>
        /// Initialise new set of command command queues with the given process name.
        /// </summary>
        /// <param name="operationName">Name of the process to be displayed in the progress bar window.<br/>
        /// E.g. 'Downloading from panel...'</param>
        public static void InitCommandQueue(string operationName)
        {
            _commandQueue.Clear();
            _commandQueue.OperationDesc = operationName;
        }


        /// <summary>
        /// Add a new command subqueue to the set
        /// </summary>
        /// <param name="direction">Comms direction (Up/Down).</param>
        /// <param name="name">Name of the command queue, to be displayed in the progress bar window.</param>
        public static void AddNewCommandSubqueue(Direction direction, string name, SubqueueCompletedHandler onCompletion)
            => _commandQueue.AddSubqueue(new CommandSubqueue(direction, onCompletion) { Name = name });


        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="dataReceiver">Handler to which the response will be sent.</param>
        public static void EnqueueCommand(byte[] commandData, ReceivedResponseDataHandler dataReceiver = null)
            => _commandQueue.Enqueue(new Command() { CommandData = commandData, DataReceiver = dataReceiver });


        ///// <summary>
        ///// Queue a new command ready to send to the panel.
        ///// </summary>
        ///// <param name="commandData">The command data.</param>
        ///// <param name="dataReceiver">Handler to which the response will be sent.</param>
        ///// <param name="index">(Optional) the index of the item requested - for the case where the index is not included in the response data (e.g. devices).</param>
        //public static void EnqueueCommand(byte[] commandData, ReceivedResponseDataHandlerWithValidation dataReceiver)
        //    => _commandQueue.Enqueue(new Command() { CommandData = commandData, DataReceiverWithValidation = dataReceiver });

        public static void StartSendingCommandQueue(Action onStart, OnFinishedHandler onEnd)
        {
            _queueWasCompleted = false;

            //arbitrarily don't show progress bar for single jobs (especially don't want to do that for firmware version request)
            if (_commandQueue.TotalCommandCount < 2)
                SendFirstCommandInQueue();
            else
                ShowProgressBarWindow(onStart, onEnd);
        }


        public static void CancelCommandQueue()
        {
            //CTecUtil.Debug.WriteLine(DateTime.Now + " - CancelCommandQueue()");
            _incomingDataTimer.Stop();
            _commandQueue?.Clear();
        }


        private static void SendFirstCommandInQueue()
        {
            CTecUtil.Debug.WriteLine("SendFirstCommandInQueue()");
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _progressBarWindow.SubqueueCount = _commandQueue.SubqueueCount;

            }), DispatcherPriority.Normal);

            SendData(_commandQueue.Peek());
        }


        private static void SendNextCommandInQueue()
        {
            CTecUtil.Debug.WriteLine("SendNextCommandInQueue()");
            _progressOverall++;
            _progressWithinSubqueue++;
            SendData(_commandQueue.Peek());
        }


        private static void ResendCommand()
        {
            var cmd = _commandQueue.Peek();
            if (cmd != null)
            {
                CTecUtil.Debug.WriteLine("ResendCommand()");

                if (cmd.Tries > 19)
                {
                    //the number of retries on the current command is excessive
                    // - report the last-noted exception (if any)
                    if (_lastException != null)
                    {
                        if (_lastException is TimeoutException)
                            error(_commandQueue.Direction == Direction.Upload ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, _lastException);
                        else if (_lastException is FormatException)
                            error(Cultures.Resources.Error_Checksum_Fail, _lastException);
                        else
                            error(_commandQueue.Direction == Direction.Upload ? Cultures.Resources.Error_Uploading_Data : Cultures.Resources.Error_Downloading_Data, _lastException);
                    }
                    else
                        error(_commandQueue.Direction == Direction.Upload ? Cultures.Resources.Error_Upload_Retries : Cultures.Resources.Error_Download_Retries);
                }
                else
                    SendData(cmd);
            }
        }
        #endregion


        #region send/receive
        private static object _sendLock = new();

        public static void SendData(Command command)
        {
            lock (_sendLock)
            {
                try
                {
                    _lastException = null;

                    if (command != null && command.CommandData != null)
                    {
                        command.Tries++;
                        CTecUtil.Debug.WriteLine("SendData() - (try=" + command.Tries + ")  [" + command.ToString() + "]");

                        try
                        {
                            if (_port is null)
                                _port = newSerialPort();

                            if (!_port.IsOpen)
                                _port.Open();

                            _port.DiscardInBuffer();
                            _port.DiscardOutBuffer();
                            _port.Write(command.CommandData, 0, command.CommandData.Length);
                        }
                        catch (Exception ex)
                        {
                            error(Cultures.Resources.Error_Serial_Port, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CTecUtil.Debug.WriteLine("  **Exception** (SendData) " + ex.Message);
                }
            }
        }


        /// <summary>
        /// called by port on reception of data
        /// </summary>
        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var port = sender as SerialPort;
                if (port == null)
                    return;

                _responseTimer.Start(_responseTimerPeriod);

                if (ListenerMode)
                    ListenerDataReceived(port);
                else
                    ResponseDataReceived(port);
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine("  **Exception** (dataReceived) " + ex.Message);
            }
        }


        private static async void ResponseDataReceived(SerialPort port)
        {
            try
            {
                //data received, so status is one of the Connected statuses
                if (_connectionStatus != ConnectionStatus.ConnectedWriteable)
                    _connectionStatus = ConnectionStatus.ConnectedReadOnly;
                NotifyConnectionStatus?.Invoke(_connectionStatus);

                var incoming = readIncomingResponse(port);

                CTecUtil.Debug.WriteLine("  incoming: [" + Utils.ByteArrayToHexString(incoming) + "] --> '" + Utils.ByteArrayToString(incoming) + "'");

                if (isPingResponse(incoming))
                {
                    //panel responded to ping, so request its read-only status
                    sendWriteableCheck();
                }
                else if (isCheckWriteableResponse(incoming))
                {
                    //read-only response received?
                    var readOnly = incoming.Length > 2 && incoming[2] == 0;
                    NotifyConnectionStatus?.Invoke(_connectionStatus = readOnly ? ConnectionStatus.ConnectedReadOnly : ConnectionStatus.ConnectedWriteable);
                }
                else if (isNak(incoming))
                {
                    CTecUtil.Debug.WriteLine("**NAK**");
                    ResendCommand();
                }
                else if (isAck(incoming))
                {
                    CTecUtil.Debug.WriteLine("  ACK");
                    if (_commandQueue.Dequeue())
                    {
                        //new queue - reset the count
                        _progressWithinSubqueue = 0;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.CommandsInCurrentSubqueue;

                        }), DispatcherPriority.Normal);
                    }
                    SendNextCommandInQueue();
                }
                else if (_commandQueue.TotalCommandCount > 0)
                {
                    var ok = false;
                    var cmd = _commandQueue.Peek();

                    if (cmd != null)
                    {
                        if (incoming != null)
                        {
                            //send response to data receiver
                            await Task.Run(new Action(() =>
                            {
                                ok = cmd.DataReceiver?.Invoke(incoming) == true;
                            }));

                            CTecUtil.Debug.WriteLine("progress: subq=" + _progressWithinSubqueue + " o/a=" + _progressOverall + "/" + _numCommandsToProcess);

                            if (_commandQueue.Dequeue())
                            {
                                //new queue - reset the count
                                _progressWithinSubqueue = 0;
                                Application.Current.Dispatcher.Invoke(new Action(() =>
                                {
                                    _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.CommandsInCurrentSubqueue;

                                }), DispatcherPriority.Normal);
                            }
                            CTecUtil.Debug.WriteLine("dequeued         : Qs=" + _commandQueue.SubqueueCount + " this=" + _commandQueue.CurrentSubqueueName + "(" + _commandQueue.CommandsInCurrentSubqueue + ") tot=" + _commandQueue.TotalCommandCount);

                        }
                    }

                    if (ok)
                    {
                        //send next command, if any
                        if (_commandQueue.TotalCommandCount > 0)
                            SendNextCommandInQueue();
                        else
                            _queueWasCompleted = true;
                    }
                    else
                    {
                        ResendCommand();
                    }
                }
            }
            catch (FormatException ex)
            {
                //checksum fail
                CTecUtil.Debug.WriteLine("  **FormatException** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
            catch (TimeoutException ex)
            {
                CTecUtil.Debug.WriteLine("  **TimeoutException** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine("  **Exception** (ResponseDataReceived) " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
        }


        private static byte[] readIncomingResponse(SerialPort port)
        {
            try
            {
                //1.5 sec timeout
                _incomingDataTimer.Start(_incomingDataTimerPeriod);

                //wait for buffering - often SerialPort.DataReceived is called by the port when BytesToRead is still zero
                while (port.BytesToRead == 0)
                {
                    Thread.Sleep(10);
                    if (_incomingDataTimer.TimedOut)
                        throw new TimeoutException();
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
                    if (_incomingDataTimer.TimedOut)
                        throw new TimeoutException();
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
                        Thread.Sleep(5);
                        if (_incomingDataTimer.TimedOut)
                            throw new TimeoutException();
                    }

                    //Read payload & checksum
                    var bytes = Math.Min(port.BytesToRead, buffer.Length - offset);
                    port.Read(buffer, offset, bytes);
                    offset += bytes;
                }

                if (!CheckChecksum(buffer))
                    throw new FormatException();

                return buffer;
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine("  **Exception** (readIncomingResponse) " + ex.Message);
                return null;
            }
            finally
            {
                _incomingDataTimer.Stop();
                port.DiscardInBuffer();
            }
        }


        private static async void ListenerDataReceived(SerialPort port)
        {
            try
            {
                NotifyConnectionStatus?.Invoke(_connectionStatus = ConnectionStatus.Listening);

                var incoming = readIncomingListenerData(port);

                if (incoming != null && ListenerDataReceiver != null)
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
                CTecUtil.Debug.WriteLine("  **Exception** (ListenerDataReceived) " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
        }


        private static byte[] readIncomingListenerData(SerialPort port)
        {
            try
            {
                _incomingDataTimer.Start(_incomingDataTimerPeriod);

                //wait for buffering [sometimes SerialPort.DataReceived is called by the port when BytesToRead is still zero]
                while (port.BytesToRead == 0)
                {
                    if (_incomingDataTimer.TimedOut)
                        throw new TimeoutException();
                }

                Thread.Sleep(100);
                var numBytes = port.BytesToRead;
                byte[] buffer = new byte[numBytes];
                port.Read(buffer, 0, numBytes);

                return buffer;
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine("  **Exception** (readIncomingListenerData) " + ex.Message);
                return null;
            }
            finally
            {
                _incomingDataTimer.Stop();
                port.DiscardInBuffer();
            }
        }

        
        private static bool isAck(byte[] data) => data != null && data.Length > 0 && isAck(data[0]);
        private static bool isNak(byte[] data) => data != null && data.Length > 0 && isNak(data[0]);
        private static bool isAck(byte data) => data == AckByte;
        private static bool isNak(byte data) => data == NakByte;

        private static bool isPingResponse(byte[] data)           => data is not null && data.Length > 0 && data[0] == _pingCommand[1];
        private static bool isCheckWriteableResponse(byte[] data) => data is not null && _checkWriteableCommand is not null && data.Length > 0 && data[0] == _checkWriteableCommand[1];
        #endregion


        private static void responseTimerTimeout()
        {
            if (_connectionStatus != ConnectionStatus.Listening && _commandQueue.TotalCommandCount == 0)
            {
                NotifyConnectionStatus?.Invoke(_connectionStatus = ConnectionStatus.Disconnected);

                //try again...
                ResendCommand();
            }
            _responseTimer.Start(_responseTimerPeriod);
        }


        public static byte CalcChecksum(byte[] data, bool outgoing = false, bool check = false)
        {
            var startByte = outgoing ? 1 : 0;
            var checkLength = check ? data.Length - 1 : data.Length;
            int checksumCalc = 0;
            for (int i = startByte; i < checkLength; i++)
                checksumCalc += data[i];
            return (byte)(checksumCalc & 0xff);
        }


        private static bool CheckChecksum(byte[] data) => data.Length > 0 && CalcChecksum(data, false, true) == data[data.Length - 1];


        private static void error(string message, Exception ex = null)
        {
            CTecUtil.Debug.WriteLine("  **Exception** " + ex?.Message);

            //check queue - avoids erroring on ping fail
            var showError = _commandQueue.TotalCommandCount > 0;
            CancelCommandQueue();
            NotifyConnectionStatus?.Invoke(_connectionStatus = ConnectionStatus.Disconnected);
            if (showError)
                ShowErrorMessage?.Invoke(message + "\n\n" + ex?.Message);
        }


        #region port
        /// <summary>
        /// Discard any pending commands and close the serial port
        /// </summary>
        public static bool ClosePort()
        {
            try
            {
                CancelCommandQueue();
                if (_port?.IsOpen == true)
                    _port?.Close();
                _port?.Dispose();
                _port = null;
                return true;
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine("  **Exception** (ClosePort) " + ex.Message);
                return false;
            }
        }

        ~SerialComms()
        {
            _port?.Close();
            _port?.Dispose();
        }


        /// <summary>
        /// Gets a list of serial ports available on the system
        /// </summary>
        public static List<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();


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
                CTecUtil.Debug.WriteLine("  **Exception** (newSerialPort) " + ex.Message);
                error(Cultures.Resources.Error_Serial_Port, ex);
            }
            return null;
        }
        #endregion


        #region progress bar
        private static ProgressBarWindow _progressBarWindow = new();
        private static int _progressOverall, _progressWithinSubqueue, _numCommandsToProcess;

        private static Action _onStartProgress;
        private static OnFinishedHandler _onEndProgress;


        public static void ShowProgressBarWindow(Action onStart, OnFinishedHandler onEnd)
        {
            _onStartProgress = onStart;
            _onEndProgress   = onEnd;

            try
            {
                _progressOverall = _progressWithinSubqueue = 0;
                //use background worker to run work method asynchronously
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += processAsynch;
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                error(ex.Message, ex);
            }
        }

        private static void hideProgressBarWindow()
        {
            CancelCommandQueue();
            _progressBarWindow.Hide();
            _onEndProgress?.Invoke(_queueWasCompleted);
        }

        private static void processAsynch(object sender, DoWorkEventArgs e)
        {
            _numCommandsToProcess  = _commandQueue.TotalCommandCount;
            var commandsInSubqueue = _commandQueue.CommandsInCurrentSubqueue;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _onStartProgress?.Invoke();

                //launch the progress bar window
                _progressBarWindow.Show(OwnerWindow);

            }), DispatcherPriority.Send);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CTecUtil.Debug.WriteLine("---progress: max=" + _numCommandsToProcess + ", subQMax=" + commandsInSubqueue);
                _progressBarWindow.ProgressBarLegend      = _commandQueue.OperationDesc;
                _progressBarWindow.SubqueueCount          = _commandQueue.SubqueueCount;
                _progressBarWindow.ProgressBarOverallMax  = _numCommandsToProcess;
                _progressBarWindow.ProgressBarSubqueueMax = commandsInSubqueue;

            }), DispatcherPriority.Render);


            var startTime = DateTime.Now;
            int lastProgress = 0;

            while (_commandQueue?.TotalCommandCount > 0)
            {
                //stop if progress hasn't changed for 10 secs
                if (DateTime.Now > startTime.AddSeconds(10))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (_commandQueue.Direction != Direction.Idle)
                            error(_commandQueue.Direction == Direction.Upload ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, new TimeoutException());

                    }), DispatcherPriority.Send);
                    break;
                }

                CTecUtil.Debug.WriteLine("---progress: " + _progressOverall + " (" + lastProgress + ")");

                if (_progressOverall > lastProgress)
                {
                    lastProgress = _progressOverall;
                    startTime = DateTime.Now;
                }

                //report progress
                Application.Current.Dispatcher.Invoke(new Action(() => _progressBarWindow.UpdateProgress(_commandQueue?.SubqueueNames, _progressOverall, _progressWithinSubqueue)), DispatcherPriority.Normal);
                Thread.Sleep(100);
            }

            //hide progress window if it hasn't already done it itself
            Application.Current.Dispatcher.Invoke(new Action(() => { hideProgressBarWindow(); }), DispatcherPriority.Normal);
        }
        #endregion

    }
}
