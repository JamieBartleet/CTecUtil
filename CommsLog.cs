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
        private int _exceptions = 0;


        public string LogDescription => _logDesc;


        private static string timestamp(DateTime time) => string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", time);
        private static string direction(CommsDirection? direction) => direction switch { CommsDirection.Upload => Cultures.Resources.Send_Abbr, CommsDirection.Download => Cultures.Resources.Receive_Abbr, _ => "" };


        public void AddText(string value)                                => _log.Add(new(value));
        public void AddCommsData(byte[] value, CommsDirection direction) => _log.Add(new(ByteArrayProcessing.ByteArrayToHexString(value), direction));
        public void AddDownload(string value, CommsDirection direction)  => _log.Add(new(value, CTecUtil.CommsDirection.Download));
        public void AddUpload(string value, CommsDirection direction)    => _log.Add(new(value, CTecUtil.CommsDirection.Upload));

        
        public void AddException(string header, Exception exception)
        {
            _exceptions++;
            _log.Add(new(header));
            _log.Add(new(exception));
        }


        /// <summary>
        /// Reads the log data into a string
        /// </summary>
        public string GetLog()
        {
            var result = new StringBuilder();

            if (_log.Count == 0)
                return Cultures.Resources.No_Data;
           
            foreach (var l in _log)
            {
                result.Append(string.Format("{0}  {1,-4} {2}\n", timestamp(l.Time), direction(l.Direction), l.Text));

                if (l.IsError)
                {
                    result.Append(l.ExceptionType.Name + "\n");
                    result.Append(l.StackTrace + "\n");
                }
            }
            return result.ToString();
        }


        public bool ContainsExceptions => _exceptions > 0;
    }
}
