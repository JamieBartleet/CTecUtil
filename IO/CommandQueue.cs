using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil.IO
{
    /// <summary>
    /// Class to maintain commands queued for sending to the panel.<br/>
    /// Commands can be grouped in one or more queues - these would typically be according to the config page they relate to.<br/>
    /// E.g. when requesting device details the zones and groups are also requested:<br/>
    /// queue #1 = device request commands, queue #2 = zone request commands, queue #3 = group request commands.
    /// </summary>
    internal class CommandQueue
    {
        public CommandQueue() { }

        public CommandQueue(CommandQueue original)
        {
            OperationDesc = original.OperationDesc;

            foreach (var sq in _subqueues)
                AddSubqueue(new CommandSubqueue(sq));
        }


        /// <summary>The queue of subqueues</summary>
        private Queue<CommandSubqueue> _subqueues = new();

        /// <summary>The subqueue at the front of the queue</summary>
        private CommandSubqueue _currentQueue;


        /// <summary>The comms direction of the current subqueue (Up/Down; or Idle if there is nothing queued)</summary>
        public SerialComms.Direction Direction { get => _subqueues.Count > 0 ? _subqueues.Peek()?.Direction ?? SerialComms.Direction.Idle : SerialComms.Direction.Idle; }


        /// <summary>
        /// The description of the overall operation - e.g. 'Downloading from panel...'
        /// </summary>
        public string OperationDesc { get; set; }


        /// <summary>
        /// Name attached to the first subqueue (i.e. the one currently being serviced)
        /// </summary>
        public string CurrentSubqueueName { get => _subqueues.Count > 0 && _subqueues.Peek()?.Count > 0 ? _subqueues.Peek()?.Name : null; }


        /// <summary>
        /// Name attached to the first subqueue (i.e. the one currently being serviced)
        /// </summary>
        public List<string> SubqueueNames { get => _subqueues.Select(sq => sq.Name).ToList(); }


        /// <summary>
        /// Add a new subqueue to the queue
        /// </summary>
        /// <param name="commandQueue"></param>
        public void AddSubqueue(CommandSubqueue commandQueue) => _subqueues.Enqueue(_currentQueue = commandQueue);


        /// <summary>
        /// Enqueues the command in the currently-enqueueing queue.<br/>
        /// NB: AddSubQueue() must have been called prior to this to initialise the queue that is actively being added to.
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command) => _currentQueue?.Enqueue(command);


        /// <summary>
        /// Dequeue the first command in the first subqueue.
        /// </summary>
        /// <returns>True if a new subqueue was started (or there are none left to process).</returns>
        public bool Dequeue()
        {
            //CTecUtil.Debug.WriteLine("Dequeue() - _subqueues.Count=" + _subqueues.Count);

            //remove first command in the current subqueue
            _subqueues.Peek()?.Dequeue();

            if (_subqueues.Count == 0 || _subqueues.Peek()?.Count > 0)
                return false;

            //if the current subqueue is empty, remove it so the first command in the next subqueue becomes front-of-queue
            while (_subqueues.Count > 0 && _subqueues.Peek()?.Count == 0)
            {
                _subqueues.Peek().OnSubqueueComplete?.Invoke();

                if (_subqueues.Count > 1)
                    _subqueues.Dequeue();
                else
                    Clear();
            }

            return true;
        }


        /// <summary>
        /// Returns the first command in the first subqueue.
        /// </summary>
        public Command Peek()
        {
            try
            {
                return _subqueues.Peek()?.Peek();
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Clear all command queues.
        /// </summary>
        public void Clear()
        {
            foreach (var q in _subqueues)
                q.Clear();
            _subqueues.Clear();
        }


        /// <summary>
        /// Total count of all commands in all subqueues.
        /// </summary>
        public int TotalCommandCount { get => _subqueues.Select(q => q.Count)?.Sum() ?? 0; }


        /// <summary>
        /// Count of subqueues.
        /// </summary>
        public int SubqueueCount { get => _subqueues.Count; }


        /// <summary>
        /// Count of commands in current subqueue.
        /// </summary>
        public int CommandsInCurrentSubqueue { get => _subqueues.Count > 0 ? _subqueues.Peek()?.Count ?? 0 : 0; }


        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("Subqueues=" + SubqueueCount);
            result.Append(" TotCmds=" + TotalCommandCount);
            if (_subqueues.Count > 0)
            {
                result.Append(" CurrQ=" + _subqueues.Peek()?.Name);
                result.Append(" Count=" + CommandsInCurrentSubqueue);
                if (_subqueues.Peek()?.Count > 0)
                {
                    var cmd = _subqueues.Peek().Peek();
                    if (cmd != null)
                        result.Append(" head=" + cmd.ToString());
                }
            }
            return result.ToString();
        }
    }
}
