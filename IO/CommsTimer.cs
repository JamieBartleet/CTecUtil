using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CTecUtil.IO
{
    public class CommsTimer //: IDisposable
    {
        public CommsTimer()
        {
            TimedOut = false;
            _timer.AutoReset = false;
            _timer.Enabled = false;
            _timer.Interval = 1000;  //default 1 sec
            _timer.Elapsed += new ElapsedEventHandler(OnTimedCommsEvent);
        }


        private Timer _timer = new();
        
        public bool  TimedOut;


        public void Start(double timeoutperiod)
        {
            _timer.Stop();
            _timer.Interval = timeoutperiod;     //milliseconds
            TimedOut = false;
            _timer.Start();
        }


        public void Stop()
        {
            _timer.Stop();
            TimedOut = false;
        }


        //public void Dispose()
        //{
        //    _timer.Dispose();
        //}


        ~CommsTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
 
        public void OnTimedCommsEvent(object source, ElapsedEventArgs e)
        {
            Debug.WriteLine(DateTime.Now + " - ***** CommsTimer TimedOut *****");
            TimedOut = true;
            _timer.Stop();
        }
    }
}
