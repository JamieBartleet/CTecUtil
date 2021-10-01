using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    internal class CommandQueue
    {
        public Queue<Command> Commands = new();

        public string Legend { get; set; }


        public void    Enqueue(Command command) => Commands.Enqueue(command);
        public void    Dequeue() => Commands.Dequeue();
        public Command Peek() => Commands?.Peek();
        public void    Clear() => Commands?.Clear();
        public int     Count { get => Commands.Count; }
    }
}
