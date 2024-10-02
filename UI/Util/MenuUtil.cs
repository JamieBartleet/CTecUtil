using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.UI.Util
{
    public class MenuUtil
    {

        [DllImport("user32.dll")]
        public static extern nint GetSystemMenu(nint hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool EnableMenuItem(nint hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        public static extern bool RemoveMenu(nint hMenu, uint uPosition, uint uFlags);
    }
}
