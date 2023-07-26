using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO.Ports;
//using System.Composition;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CTecUtil.IO
{
    /// <summary>
    /// 
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class SerialPortWatcher : IDisposable
    {
        public SerialPortWatcher()
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _comPorts = new();

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

            _watcher = new ManagementEventWatcher(query);           
            _watcher.EventArrived += (sender, eventArgs) => checkForNewPorts(eventArgs);
            _watcher.Start();
        }
        

        private List<string> _comPorts;
        private ManagementEventWatcher _watcher;
        private TaskScheduler _taskScheduler;

        public delegate void PortsChangedHandler(List<string> ports);
        public PortsChangedHandler PortsChanged;


        private void checkForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(checkForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }


        private static object _lock = new();

        private void checkForNewPortsAsync()
        {
            lock (_lock)
            {
                IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

                try
                {
                    foreach (string comPort in _comPorts)
                        if (!ports.Contains(comPort))
                            _comPorts.Remove(comPort);
                }
                catch (InvalidOperationException) { }

                try
                {
                    foreach (var port in ports)
                        if (!_comPorts.Contains(port))
                            _comPorts.Add(port);
                    
                    _comPorts.Sort();
                }
                catch (InvalidOperationException) { }

                PortsChanged?.Invoke(_comPorts);
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            _watcher.Stop();    
        }

        #endregion
    }
}

