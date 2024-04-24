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

    class Enums
    {
        public static string SupportedAppsToString(SupportedApps app)
         => app switch
         {
            SupportedApps.XFP     => "XfpTools",
            SupportedApps.ZFP     => "ZfpTools",
            SupportedApps.Quantec => "Quantec",
            _                     => "Unknown C-Tec app",
        };
    }
}
