using System;

namespace CTecUtil
{
    public class LogEntry
    {
        public LogEntry() { }

        public LogEntry(string text)
        {
            Time = DateTime.Now;
            Text = text;
        }

        public LogEntry(Exception ex)
        {
            Time    = DateTime.Now;
            Text    = ex.Message;
            IsError = true;
            ExceptionType = ex.GetType();
            StackTrace    = ex.StackTrace;
        }


        public DateTime Time { get; set; }
        public string   Text { get; set; }
        public bool     IsError { get; set; }
        public Type     ExceptionType { get; set; }
        public string   StackTrace { get; set; }
    }
}
