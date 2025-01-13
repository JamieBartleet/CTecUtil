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
        public Log(string name) => _name = name;


        private string _name;
        private List<string> _log = new();
        private int _errors = 0;

        private static string logPrefix => DateTime.Now + " - ";


        public void Write(string value) => _log.Add(logPrefix + value);



        public void WriteError(string header, Exception exception)
        {
            _errors++;
            _log.Add("****************");
            _log.Add(logPrefix + header);
            _log.Add(exception.StackTrace);
            _log.Add(exception.ToString());
            _log.Add("****************");
        }


        /// <summary>
        /// Reads the log data into a string
        /// </summary>
        public string Read()
        {
            var result = new StringBuilder();
            foreach (var l in _log)
                result.Append(l + "\n");
            return result.ToString();
        }


        public bool ErrorsWereLogged => _errors > 0;
    }
}
