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
    public class CommsLog
    {
        public CommsLog(string logDesc) => _logDesc = logDesc;


        private string _logDesc;
        private List<CommsLogEntry> _log = new();
        private bool _failed = false;
        private int _exceptionCount = 0;


        public string LogDescription => _logDesc;


        private static string formattedTime(DateTime time) => string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", time);
        private static string blankTime                => new string(' ', 23);
        private static string direction(CommsDirection? direction) => direction switch { CommsDirection.Upload => Cultures.Resources.Send_Abbr, CommsDirection.Download => Cultures.Resources.Receive_Abbr, _ => "" };


        public void AddText(string value, bool logTime = false)          => _log.Add(new(value, logTime));
        public void AddCommsData(byte[] value, CommsDirection direction) => _log.Add(new(ByteArrayProcessing.ByteArrayToHexString(value), direction));
        public void AddDownload(string value, CommsDirection direction)  => _log.Add(new(value, CTecUtil.CommsDirection.Download));
        public void AddUpload(string value, CommsDirection direction)    => _log.Add(new(value, CTecUtil.CommsDirection.Upload));

        
        public void AddException(string header, Exception exception, bool failed = false)
        {
            _exceptionCount++;
            if (failed)
                _failed = true;
            _log.Add(new("\n"));
            _log.Add(new(header, true));
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
                var timeStamp = l.LogTime ? formattedTime(l.Time) : blankTime;

                if (l.IsError)
                {
                    result.Append(string.Format("{0}  {1,-4} {2}\n", timeStamp, direction(l.Direction), l.Text));
                    result.Append(l.ExceptionType.Name + "\n");
                    result.Append(l.StackTrace + "\n");
                }
                else
                {
                    result.Append(string.Format("{0}  {1,-4} {2}\n", timeStamp, direction(l.Direction), l.Text));
                }
            }
            return result.ToString();
        }


        public bool Failed             => _failed;
        public bool ContainsExceptions => _exceptionCount > 0;
        public int  ExceptionCount     => _exceptionCount;
    }
}
