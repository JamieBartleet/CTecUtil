using System;

namespace CTecUtil
{
    public class LogEntry
    {
        public LogEntry() { }

        public LogEntry(string text, bool logTime = false)
        {
            Text = text;
            if (LogTime = logTime)
                Time = DateTime.Now;
        }

        public LogEntry(Exception ex)
        {
            Text          = ex.Message;
            IsError       = true;
            ExceptionType = ex.GetType();
            StackTrace    = ex.StackTrace;
            Time          = DateTime.Now;
            LogTime       = true;
        }


        public DateTime Time { get; set; }
        public string   Text { get; set; }
        public bool     IsError { get; set; }
        public Type     ExceptionType { get; set; }
        public string   StackTrace { get; set; }
        public bool     LogTime { get; set; }
    }
}
