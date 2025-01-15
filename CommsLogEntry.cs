using System;

namespace CTecUtil
{
    public class CommsLogEntry : LogEntry
    {
        public CommsLogEntry(string text) : base(text) { }
        public CommsLogEntry(Exception ex) : base(ex) { }
        public CommsLogEntry(string text, CommsDirection direction) : base(text) => _direction = direction;


        private CommsDirection? _direction;

        public CommsDirection? Direction => _direction;
    }
}
