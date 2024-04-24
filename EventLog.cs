using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CTecUtil
{
    public class EventLog
    {
        public static string WriteInfo(string message)    => write(message, EventLogEntryType.Information);
        public static string WriteWarning(string message) => write(message, EventLogEntryType.Warning);
        public static string WriteError(string message)   => write(message, EventLogEntryType.Error);


        private static string _source  = ".NET Runtime";        // <-- use this in the absence of a registered event source
        private static int    _eventId = 1000;                  // <-- use 1000 to avoid the message "The description for Event ID [xxx] from source .NET Runtime cannot be found" in the event log


        private static string write(string message, EventLogEntryType level)
        {
            System.Diagnostics.EventLog.WriteEntry(_source, "Application: " + Process.GetCurrentProcess().ProcessName + "\n" + message, level, _eventId);
            return message;
        }
    }
}
