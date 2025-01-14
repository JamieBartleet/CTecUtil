using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTecUtil
{
    /// <summary>
    /// Replicates WriteLine() and Write() functions from System.Diagnostics.Debug, prefixing the output with a timestamp.
    /// </summary>
    public class LogEntry
    {
        public LogEntry(string text)
        {
            Time = DateTime.Now;
            Text = text;
        }

        public LogEntry(Exception ex)
        {
            Time = DateTime.Now;
            Text    = ex.Message;
            IsError = true;
            StackTrace = ex.StackTrace;
        }


        public DateTime Time { get; set; }
        public string   Text { get; set; }
        public bool     IsError { get; set; }
        public string   StackTrace { get; set; }
    }
}
