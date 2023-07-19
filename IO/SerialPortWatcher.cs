using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.IO
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class SerialPortWatcher : IDisposable
    {
    }
}

https://stackoverflow.com/questions/10550635/new-com-port-available-event