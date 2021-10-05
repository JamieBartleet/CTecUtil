using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    internal class CommandQueue
    {
        private Queue<CommandSubQueue> _subqueues = new();

        private CommandSubQueue _currentQueue;


        /// <summary>
        /// The name of the overall operation - e.g. 'Downloading from panel'
        /// </summary>
        public string OperationName { get; set; }


        /// <summary>
        /// Name attached to the first subqueue (i.e. the one currently being serviced)
        /// </summary>
        public string QueueName { get => _subqueues.Count > 0 && _subqueues?.Peek()?.Count > 0 ? _subqueues?.Peek()?.Name : null; }


        public void AddSubqueue(CommandSubQueue commandQueue) => _subqueues.Enqueue(_currentQueue = commandQueue);


        /// <summary>
        /// Enqueues the command in the currently-enqueueing queue.<br/>
        /// NB: AddSubQueue() must be called prior to this to initialise the queue that is actively being added to.
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
                if (_subqueues.Count > 1)
                    _subqueues.Dequeue();
                else
                    Clear();
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
