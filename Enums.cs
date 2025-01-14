using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil
{
    public enum SupportedApps
    {
        NotSet,
        XFP,
        ZFP,
        Quantec,
    }


    /// <summary>Direction of the current data transfer, if any</summary>
    public enum CommsDirection
    {
        /// <summary>No data transfer in progress</summary>
        Idle,

        /// <summary>Uploading to panel</summary>
        Upload,

        /// <summary>Downloading from panel</summary>
        Download
    }
}
