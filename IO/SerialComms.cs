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
            _responseTimer.OnTimedOut = new(() => OnConnectionStatusChange?.Invoke(ConnectionStatus.Disconnected));
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

            /// <summary>Comms is actively connected</summary>
            Connected
        }


        public enum Direction { Idle, Up, Down }


        private static SerialPort _port;
        private static CommandQueue _commandQueue = new();
        private static CommsTimer _timer = new();
        private static Exception _lastException = null;

        
        public delegate void ReceivedResponseDataHandler(byte[] incomingData, int index = -1);

        public delegate void ReceivedListenerDataHandler(byte[] incomingData);
        public static ReceivedListenerDataHandler ListenerDataReceiver;


        public delegate void ProgressMaxSetter(int maxValue);
        public delegate void ProgressValueUpdater(int value);

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
        public static bool ListenerMode { get => _listenerMode; set { if (_listenerMode = value) OnConnectionStatusChange?.Invoke(ConnectionStatus.Listening); } }


        #region ping
        public delegate void ConnectionStatusChangeHandler(ConnectionStatus status);
        public static ConnectionStatusChangeHandler OnConnectionStatusChange;


        /// <summary>Timer for pinging the panel every few seconds</summary>
        private static System.Timers.Timer _pingTimer;

        /// <summary>Timer on response to SendData; the OnTimedOut event us used to notify the connection status</summary>
        private static CommsTimer _responseTimer = new();


        private static byte[] _pingCommand;
        public static void SetPingCommand(byte[] command)
        {
            var start = _pingCommand is null;
            _pingCommand = command;

            if (start)
            {
                _pingTimer = new()
                {
                    AutoReset = true,
                    Enabled = true,
                    Interval = 3000
                };
                _pingTimer.Elapsed += new(sendPing);

                //_pingResponseTimer = new();
                //_pingResponseTimer.OnTimedOut = new(() => { OnConnectionStatusChange?.Invoke(ConnectionStatus.Disconnected); });
            }
        }


        private static void sendPing(object sender, ElapsedEventArgs e)
        {
            //only ping the panel if there is no active upload/download
            if (_commandQueue.SubqueueCount == 0)
            {
                //_pingResponseTimer.Start(5000);
                SendData(new Command() { CommandData = _pingCommand });
            }
            else
            {
                _responseTimer.Stop();
            }
        }
        #endregion


        /// <summary>
        /// Discard any pending commands and close the serial port
        /// </summary>
        public static bool Close()
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


        /// <summary>
        /// Gets a list of serial ports available on the system
        /// </summary>
        public static List<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();


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
        {
            _commandQueue.AddSubqueue(new CommandSubqueue(direction, onCompletion) { Name = name });
        }


        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="dataReceiver">Handler to which the response will be sent.</param>
        /// <param name="index">(Optional) the index of the item requested - for the case where the index is not included in the response data (e.g. devices).</param>
        public static void EnqueueCommand(byte[] commandData, ReceivedResponseDataHandler dataReceiver, int? index = null)
            => _commandQueue.Enqueue(new Command() { CommandData = commandData, DataReceiver = dataReceiver, Index = index });


        /// <summary>
        /// Queue a new command ready to send to the panel.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="index">(Optional) the index of the item requested - for the case where the index is not included in the response data (e.g. devices).</param>
        public static void EnqueueCommand(byte[] commandData, int? index = null) => EnqueueCommand(commandData, null, index);


        public static void StartSendingCommandQueue(Action onStart, Action onEnd)
        {
            if (_commandQueue.TotalCommandCount > 2)
                ShowProgressBarWindow(onStart, onEnd);
            else
                SendFirstCommandInQueue();
        }


        public static void CancelCommandQueue()
        {
            //Debug.WriteLine(DateTime.Now + " - CancelCommandQueue()");
            _timer.Stop();
            _commandQueue?.Clear();
            //Thread.Sleep(500);
        }


        private static void SendFirstCommandInQueue()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _progressBarWindow.SubqueueCount = _commandQueue.SubqueueCount;

            }), DispatcherPriority.Normal);

            SendData(_commandQueue.Peek());
        }


        private static void SendNextCommandInQueue() { SendData(_commandQueue.Peek()); }


        private static void ResendCommand()
        {
            var cmd = _commandQueue.Peek();
            if (cmd != null)
            {
                Debug.WriteLine(DateTime.Now + " - ResendCommand()");

                if (cmd.Tries > 19)
                {
                    //the number of retries on the current command is excessive
                    // - report the last-noted exception (if any)
                    if (_lastException != null)
                    {
                        if (_lastException is TimeoutException)
                            error(_commandQueue.Direction == Direction.Up ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, _lastException);
                        else if (_lastException is FormatException)
                            error(Cultures.Resources.Error_Checksum_Fail, _lastException);
                        else
                            error(_commandQueue.Direction == Direction.Up ? Cultures.Resources.Error_Uploading_Data : Cultures.Resources.Error_Downloading_Data, _lastException);
                    }
                    else
                        error(_commandQueue.Direction == Direction.Up ? Cultures.Resources.Error_Upload_Retries : Cultures.Resources.Error_Download_Retries);
                }
                else
                    SendData(cmd);
            }
        }


        private static object _sendLock = new();

        public static void SendData(Command command)
        {
            lock (_sendLock)
            {
                _lastException = null;

                if (command != null)
                {
                    command.Tries++;
                    //Debug.WriteLine(DateTime.Now + " - SendData() - (try=" + command.Tries + ")  [" + command.ToString() + "]");

                    try
                    {
                        if (_port is null)
                            _port = newSerialPort();

                        if (!_port.IsOpen)
                            _port.Open();

                        _port.DiscardInBuffer();
                        _port.DiscardOutBuffer();
                        _port.Write(command.CommandData, 0, command.CommandData.Length);
                
                        _responseTimer.Start(5000);
                    }
                    catch (Exception ex)
                    {
                        error(Cultures.Resources.Error_Serial_Port, ex);
                    }
                }
            }
        }


        /// <summary>
        /// called by port on reception of data
        /// </summary>
        private static void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            if (port == null)
                return;

            if (ListenerMode)
                ListenerDataReceived(port);
            else
                ResponseDataReceived(port);
        }


        private static async void ResponseDataReceived(SerialPort port)
        {
            try
            {

                var incoming = readIncomingResponse(port);
                
                _responseTimer.Stop();
                OnConnectionStatusChange?.Invoke(ConnectionStatus.Connected);

                //Debug.WriteLine(DateTime.Now + " -   incoming: [" + Utils.ByteArrayToString(incoming) + "]");

                if (isNak(incoming))
                {
                    Debug.WriteLine(DateTime.Now + " -   Nak");
                    ResendCommand();
                }
                else if (_commandQueue.TotalCommandCount > 0)
                {
                    var cmd = _commandQueue.Peek();

                    if (incoming != null && cmd != null)
                    {
                        if (_commandQueue.Dequeue())
                        {
                            //new queue - reset the count
                            _progressWithinSubqueue = 0;
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                _progressBarWindow.ProgressBarSubqueueMax = _commandQueue.CommandsInCurrentSubqueue;

                            }), DispatcherPriority.Normal);
                        }
                        //Debug.WriteLine(DateTime.Now + " - dequeued         : Qs=" + _commandQueue.SubqueueCount + " this=" + _commandQueue.CurrentSubqueueName + "(" + _commandQueue.CommandsInCurrentSubqueue + ") tot=" + _commandQueue.TotalCommandCount);

                        //send response to data receiver
                        await Task.Run(new Action(() =>
                        {
                            if (cmd.Index != null)
                                cmd.DataReceiver?.Invoke(incoming, cmd.Index.Value);
                            else
                                cmd.DataReceiver?.Invoke(incoming);
                        }));

                        //Debug.WriteLine(DateTime.Now + " - progress: subq=" + _progressWithinSubqueue + " o/a=" + _progressOverall + "/" + _numCommandsToProcess);
                        _progressOverall++;
                        _progressWithinSubqueue++;
                    }

                    //send next command, if any
                    if (_commandQueue.TotalCommandCount > 0)
                        SendNextCommandInQueue();
                }
            }
            catch (FormatException ex)
            {
                //checksum fail
                Debug.WriteLine(DateTime.Now + " -   **FormatException** " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine(DateTime.Now + " -   **TimeoutException** " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now + " -   **Exception** " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
        }


        private static byte[] readIncomingResponse(SerialPort port)
        {
            try
            {
                //1.5 sec timeout
                _timer.Start(2000);

                //wait for buffering [sometimes dataReceived() is called by the port when BytesToRead is still zero]
                while (port.BytesToRead == 0)
                {
                    Thread.Sleep(10);
                    if (_timer.TimedOut)
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
                    //Thread.Sleep(20);
                    if (_timer.TimedOut)
                        throw new TimeoutException();
                }

                port.Read(header, 1, 1);
                var payloadLength = header[1];

                //now we know how many more bytes to expect - i.e. header + payloadLength + 1 byte for checksum
                byte[] buffer = new byte[header.Length + payloadLength + 1];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                //Debug.WriteLine(DateTime.Now + " - buffer=" + Utils.ByteArrayToString(buffer));

                int offset = header.Length;
                while (offset < buffer.Length)
                {
                    while (port.BytesToRead == 0)
                    {
                        //Debug.WriteLine(DateTime.Now + " -    wait - expecting " + payloadLength + " bytes payload");
                        //Thread.Sleep(20);
                        if (_timer.TimedOut)
                            throw new TimeoutException();
                    }

                    //Debug.WriteLine(DateTime.Now + " -   read " + port.BytesToRead + " bytes");

                    //Read payload & checksum
                    var bytes = Math.Min(port.BytesToRead, buffer.Length - offset);
                    port.Read(buffer, offset, bytes);
                    //Debug.WriteLine(DateTime.Now + " - buffer=" + Utils.ByteArrayToString(buffer));
                    offset += bytes;
                }

                if (!CheckChecksum(buffer))
                    throw new FormatException();

                return buffer;
            }
            finally
            {
                _timer.Stop();
                port.DiscardInBuffer();
            }
        }


        private static async void ListenerDataReceived(SerialPort port)
        {
            try
            {
                OnConnectionStatusChange?.Invoke(ConnectionStatus.Listening);

                var incoming = readIncomingListenerData(port);

                //Debug.WriteLine(DateTime.Now + " -   incoming: [" + Utils.ByteArrayToString(incoming) + "]");

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
                Debug.WriteLine(DateTime.Now + " -   **Exception** " + ex.Message);
                _lastException = ex;
                ResendCommand();
            }
        }


        private static byte[] readIncomingListenerData(SerialPort port)
        {
            try
            {
                //1.5 sec timeout
                _timer.Start(2500);

                //wait for buffering [sometimes dataReceived() is called by the port when BytesToRead is still zero]
                while (port.BytesToRead == 0)
                {
                    if (_timer.TimedOut)
                        throw new TimeoutException();
                }

                var numBytes = port.BytesToRead;
                byte[] buffer = new byte[numBytes];
                port.Read(buffer, 0, numBytes);

                return buffer;
            }
            finally
            {
                _timer.Stop();
                port.DiscardInBuffer();
            }
        }

        
        private static bool isAck(byte[] data) => data != null && data.Length > 0 && isAck(data[0]);
        private static bool isNak(byte[] data) => data != null && data.Length > 0 && isNak(data[0]);
        private static bool isAck(byte data) => data == AckByte;
        private static bool isNak(byte data) => data == NakByte;


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


        private static void error(string message, Exception ex = null)
        {
            //check quque - avoids erroring on ping fail
            var showError = _commandQueue.TotalCommandCount > 0;
            CancelCommandQueue();
            OnConnectionStatusChange?.Invoke(ConnectionStatus.Disconnected);
            if (showError)
                ShowErrorMessage?.Invoke(message + "\n\n" + ex?.Message);
        }


        #region progress bar
        private static ProgressBarWindow _progressBarWindow = new();
        private static int _progressOverall, _progressWithinSubqueue, _numCommandsToProcess;

        private static Action _onStartProgress, _onEndProgress;


        public static void ShowProgressBarWindow(Action onStart, Action onEnd)
        {
            _onStartProgress = onStart;
            _onEndProgress = onEnd;

            try
            {
                _progressOverall = _progressWithinSubqueue = 0;
                //use background worker to asynchronously run work method
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
            _onEndProgress?.Invoke();
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
                _progressBarWindow.ProgressBarLegend      = _commandQueue.OperationDesc;
                _progressBarWindow.ProgressBarOverallMax  = _numCommandsToProcess;
                _progressBarWindow.ProgressBarSubqueueMax = commandsInSubqueue;

            }), DispatcherPriority.ContextIdle);

            //start the job
            SendFirstCommandInQueue();

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
                            error(_commandQueue.Direction == Direction.Up ? Cultures.Resources.Error_Upload_Timeout : Cultures.Resources.Error_Download_Timeout, new TimeoutException());

                    }), DispatcherPriority.Send);
                    break;
                }

                if (_progressOverall > lastProgress)
                {
                    lastProgress = _progressOverall;
                    startTime = DateTime.Now;
                }

                //report progress
                Application.Current.Dispatcher.Invoke(new Action(() => _progressBarWindow.UpdateProgress(_commandQueue?.SubqueueNames, _progressOverall, _progressWithinSubqueue)), DispatcherPriority.Normal);
                Thread.Sleep(100);
            }

            //Debug.WriteLine(DateTime.Now + " - finished?");

            //hide progress window if it hasn't already done it itself
            Application.Current.Dispatcher.Invoke(new Action(() => { hideProgressBarWindow(); }), DispatcherPriority.ApplicationIdle);
        }
        #endregion

    }
}
