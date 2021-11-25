using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CTecUtil.IO
{
    public class CommsTimer
    {
        public CommsTimer()
        {
            TimedOut = false;
            _timer.AutoReset = false;
            _timer.Enabled = false;
            _timer.Interval = 1000;  //default 1 sec
            _timer.Elapsed += new ElapsedEventHandler(onTimedCommsEvent);
        }


        private Timer _timer = new();
        

        /// <summary>
        /// 
        /// </summary>
        public bool  TimedOut { get; set; }


        /// <summary>
        /// (Re)start the timer with the specified timeout period (ms)
        /// </summary>
        public void Start(double timeoutperiod)
        {
            _timer.Stop();
            _timer.Interval = timeoutperiod;
            TimedOut = false;
            _timer.Start();
        }


        /// <summary>
        /// Stop the timer; reset TimedOut
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            TimedOut = false;
        }
 

        /// <summary>
        /// Event triggered when timer expires: sets TimedOut = true
        /// </summary>
        private void onTimedCommsEvent(object source, ElapsedEventArgs e)
        {
            Debug.WriteLine(DateTime.Now + " - ***** CommsTimer TimedOut *****");
            TimedOut = true;
            _timer.Stop();
        }


        ~CommsTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
