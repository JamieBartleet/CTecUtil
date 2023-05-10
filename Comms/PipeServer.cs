using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil.Comms
{
    public class PipeServer
    {
        private string _pipeName;
        private NamedPipeServerStream _pipeServer;

        public delegate void PipeMessageHandler(string data);
        public PipeMessageHandler ReceivePipeMessage;


        public void Listen(string PipeName)
        {
            try
            {
                _pipeName = PipeName;
                _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                _pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), _pipeServer);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\n\n" + ex.ToString(), "Listen");
                Debug.WriteLine("PipeServer.Listen - " + ex.Message);
            }
        }


        private object _lock = new();


        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            lock (_lock)
            {
                try
                {
                    _pipeServer.EndWaitForConnection(iar);

                    //read the incoming message
                    //NB: fixed buffer size
                    var bufferSize = 1000;
                    byte[] buffer = new byte[bufferSize];
                    _pipeServer.Read(buffer, 0, bufferSize);
                    ReceivePipeMessage?.Invoke(Encoding.UTF8.GetString(buffer, 0, buffer.Length));

                    //start new wait server
                    _pipeServer.Flush();
                    _pipeServer.Close();
                    _pipeServer.Dispose();
                    _pipeServer = null;
                    _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    _pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), _pipeServer);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message + "\n\n" + ex.ToString(), "WaitForConnectionCallBack");
                    Debug.WriteLine("PipeServer.WaitForConnectionCallBack - " + ex.Message);
                }
            }
        }
    }
}
