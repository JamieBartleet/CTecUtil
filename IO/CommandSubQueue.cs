using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    internal class CommandSubqueue
    {
        public CommandSubqueue(SerialComms.SubqueueCompletedHandler onCompletion) => OnSubqueueComplete = onCompletion;


        private Queue<Command> _commandQueue = new();


        public string Name { get; set; }


        public void    Enqueue(Command command) => _commandQueue.Enqueue(command);
        public void    Dequeue()                => _commandQueue.Dequeue();
        public Command Peek()                   { try { return _commandQueue?.Peek(); } catch { return null; } }
        public void    Clear()                  => _commandQueue?.Clear();
        public int     Count                    => _commandQueue.Count;


        public SerialComms.SubqueueCompletedHandler OnSubqueueComplete;
    }
}
