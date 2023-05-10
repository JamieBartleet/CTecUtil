using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil.Comms
{
    public class PipeClient
    {
        public void Send(string message, string pipeName, int timeOut = 1000)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                pipeStream.Connect(timeOut);
                pipeStream.BeginWrite(data, 0, data.Length, AsyncSend, pipeStream);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Pipe Send Exception: " + ex.Message + "\n\n" + ex.ToString(), "Send");
                Debug.WriteLine("PipeServer.Send - " + ex.Message);
            }
        }

        private void AsyncSend(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeClientStream pipeStream = (NamedPipeClientStream)asyncResult.AsyncState;

                //end the write
                pipeStream.EndWrite(asyncResult);
                pipeStream.Flush();
                pipeStream.Close();
                pipeStream.Dispose();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\n\n" + ex.ToString(), "AsyncSend");
                Debug.WriteLine("PipeServer.AsyncSend - " + ex.Message);
            }
        }
    }
}
