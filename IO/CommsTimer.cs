using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CTecUtil.IO
{
    internal class CommsTimer
    {
        internal CommsTimer()
        {
            TimedOut = false;
            _timer.AutoReset = false;
            _timer.Enabled = false;
            _timer.Interval = 1000;  //default 1 sec
            _timer.Elapsed += new ElapsedEventHandler(onTimedCommsEvent);
        }


        internal delegate void TimeoutHandler();
        internal TimeoutHandler OnTimedOut;


        private Timer _timer = new();


        /// <summary>
        /// 
        /// </summary>
        internal bool TimedOut { get; set; }


        //internal double Interval { get => _timer.Interval; set => _timer.Interval = value; }


        /// <summary>
        /// (Re)start the timer with the specified timeout period (ms)
        /// </summary>
        internal void Start(double timeoutperiod)
        {
            Stop();
            _timer.Interval = timeoutperiod;
            _timer.Start();
        }


        /// <summary>
        /// Stop the timer; reset TimedOut
        /// </summary>
        internal void Stop()
        {
            _timer.Stop();
            TimedOut = false;
        }
 

        /// <summary>
        /// Event triggered when timer expires: sets TimedOut = true
        /// </summary>
        private void onTimedCommsEvent(object source, ElapsedEventArgs e)
        {
            CTecUtil.Debug.WriteLine("***** CommsTimer TimedOut *****");
            _timer.Stop();
            TimedOut = true;
            OnTimedOut?.Invoke();
        }


        ~CommsTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
