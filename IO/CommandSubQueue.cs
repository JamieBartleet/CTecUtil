using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    internal class CommandSubqueue
    {
        internal CommandSubqueue(CommandSubqueue original)
        {
            Direction          = original.Direction;
            OnSubqueueComplete = original.OnSubqueueComplete;
            Name               = original.Name;
            foreach (var c in _commandQueue)
                _commandQueue.Append(new Command(c));
        }

        internal CommandSubqueue(SerialComms.Direction direction, SerialComms.SubqueueCompletedHandler onCompletion)
        {
            Direction = direction;
            OnSubqueueComplete = onCompletion;
        }


        private Queue<Command> _commandQueue = new();


        internal string Name { get; set; }

        internal SerialComms.Direction Direction { get; set; }


        internal void    Enqueue(Command command) => _commandQueue.Enqueue(command);
        //public void    Dequeue()                => _commandQueue.Dequeue();
        internal void Dequeue()
        {
            _commandQueue.Dequeue();
            CTecUtil.Debug.WriteLine("Dequeue() - _commandQueue.Count=" + _commandQueue.Count);
        }

        internal Command Peek()                   { try { return _commandQueue?.Peek(); } catch { return null; } }
        internal void    Clear()                  => _commandQueue?.Clear();
        internal int     Count                    => _commandQueue.Count;


        internal SerialComms.SubqueueCompletedHandler OnSubqueueComplete;
    }
}
