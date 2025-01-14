using CTecUtil.IO;
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
    public class Log
    {
        public Log(string logName, CommsDirection direction) { _logName = logName; _direction = direction; }


        private string _logName;
        private CommsDirection _direction;
        private List<LogEntry> _log = new();
        private int _errors = 0;


        public string         LogName   => _logName;
        public CommsDirection Direction => _direction;


        private static string logPrefix => DateTime.Now + " - ";


        public void Add(string value) => _log.Add(new(value));

        public void AddError(string header, Exception exception)
        {
            _errors++;
            _log.Add(new(exception));
        }


        /// <summary>
        /// Reads the log data into a string
        /// </summary>
        public string GetLog()
        {
            var result = new StringBuilder();

            foreach (var l in _log)
            {
                result.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", l.Time) + " - " + l.Text + "\n");
                if (l.IsError)
                    result.Append(l.StackTrace + "\n");
            }
            return result.ToString();
        }


        public bool ErrorsWereLogged => _errors > 0;
    }
}
