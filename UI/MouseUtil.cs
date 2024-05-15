using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CTecUtil.UI
{
    public class MouseUtil
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public int X;
            public int Y;
        };


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(nint hwnd, ref Win32Point pt);


        /// <summary>
        /// Returns the mouse cursor location.  This method is necessary during
        /// a drag-drop operation because the WPF mechanisms for retrieving the
        /// cursor coordinates are unreliable.
        /// </summary>
        /// <param name="relativeTo">The Visual to which the mouse coordinates will be relative.</param>
        public static Point GetMousePosition(Visual relativeTo)
        {
            var mouse = new MouseUtil.Win32Point();
            MouseUtil.GetCursorPos(ref mouse);
            return relativeTo.PointFromScreen(new Point(mouse.X, mouse.Y));
        }
    }
}
