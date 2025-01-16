using System;

namespace CTecUtil
{
    public class CommsLogEntry : LogEntry
    {
        public CommsLogEntry(string text, bool logEntry = false) : base(text, logEntry) { }
        public CommsLogEntry(Exception ex) : base(ex) { }
        public CommsLogEntry(string text, CommsDirection direction) : base(text, true) => _direction = direction;


        private CommsDirection? _direction;
        public CommsDirection? Direction => _direction;
    }
}
