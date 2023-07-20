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

namespace CTecUtil.IO
{
    //ref: https://stackoverflow.com/questions/10550635/new-com-port-available-event

    /// <summary>
    /// 
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class SerialPortWatcher : IDisposable
    {
        public SerialPortWatcher(ObservableCollection<string> ports)
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _comPorts = ports;

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

            _watcher = new ManagementEventWatcher(query);           
            _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
            _watcher.Start();       
        }
        

        private ObservableCollection<string> _comPorts;

        private ManagementEventWatcher _watcher;
        private TaskScheduler _taskScheduler;


        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        private void CheckForNewPortsAsync()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

            foreach (string comPort in _comPorts)
                if (!ports.Contains(comPort))
                    _comPorts.Remove(comPort);

            foreach (var port in ports)
                if (!_comPorts.Contains(port))
                    AddPort(port);
        }

        private void AddPort(string port)
        {
            for (int j = 0; j < _comPorts.Count; j++)
            {
                if (port.CompareTo(_comPorts[j]) < 0)
                {
                    _comPorts.Insert(j, port);
                    break;
                }
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

