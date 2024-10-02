using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.UI.Util
{
    public class IconUtil
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(nint hObject);
    }
}
