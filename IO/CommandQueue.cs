using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <summary>The queue of subqueues</summary>
        private Queue<CommandSubqueue> _subqueues = new();

        /// <summary>The subqueue at the front of the queue</summary>
        private CommandSubqueue _currentQueue;


        /// <summary>
        /// The description of the overall operation - e.g. 'Downloading from panel...'
        /// </summary>
        public string OperationDesc { get; set; }


        /// <summary>
        /// Name attached to the first subqueue (i.e. the one currently being serviced)
        /// </summary>
        public string CurrentSubqueueName { get => _subqueues.Count > 0 && _subqueues?.Peek()?.Count > 0 ? _subqueues?.Peek()?.Name : null; }


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
        public void Enqueue(Command command)
        {
            _currentQueue?.Enqueue(command);
        }


        /// <summary>
        /// Dequeue the first command in the first subqueue.
        /// </summary>
        /// <returns>True if a the subqueue was changed.</returns>
        public bool Dequeue()
        {
            //remove first command in the first subqueue
            _subqueues.Peek()?.Dequeue();

            //if there are no more commands in this subqueue, remove it so the first command in the next subqueue becomes front-of-queue
            if (_subqueues.Peek()?.Count == 0)
            {
                _subqueues.Peek().OnSubqueueComplete?.Invoke();

                if (_subqueues.Count > 1)
                {
                    _subqueues.Dequeue();
                }
                else
                {
                    Clear();
                }
                return true;
            }

            return false;
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
            _subqueues?.Clear();
        }


        /// <summary>
        /// Total count of all commands in all subqueues.
        /// </summary>
        public int TotalCommandCount
        {
            get
            {
                int count = 0;
                foreach (var q in _subqueues)
                    count += q.Count;
                return count;
            }
        }


        /// <summary>
        /// Count of subqueues.
        /// </summary>
        public int SubqueueCount { get => _subqueues.Count; }


        /// <summary>
        /// Count of commands in current subqueue.
        /// </summary>
        public int CommandsInCurrentSubqueue { get => _subqueues.Count > 0 ? _subqueues?.Peek()?.Count??0 : 0; }

    }
}
