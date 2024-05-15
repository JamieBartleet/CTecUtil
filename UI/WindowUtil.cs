using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace CTecUtil.UI
{
    public class WindowUtil
    {
        #region app window

        [DllImport("User32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9;

        #endregion


        #region window helpers

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x0001;
        public const int WS_EX_RIGHT = 0x00001000;
        public const int WS_EX_RTLREADING = 0x00002000;

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_FRAMECHANGED = 0x0020;
        public const uint WM_SETICON = 0x0080;

        #endregion


        /// <summary>
        /// Ensure that a window maximises to the correct size when its Shell:WindowChrome 
        /// has been overridden, otherwise the extremities may be clipped.
        /// </summary>
        /// <param name="window"></param>
        public static void PreventClipWhenMaximised(System.Windows.Window window) => ((HwndSource)System.Windows.PresentationSource.FromVisual(window)).AddHook(HookGetMinMaxInfo);

        public static IntPtr HookGetMinMaxInfo(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MonitorUtil.WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized so the extremities aren't clipped
                MonitorUtil.MINMAXINFO mmi = (MonitorUtil.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MonitorUtil.MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorUtil.MonitorFromWindow(hwnd, MonitorUtil.MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MonitorUtil.MONITORINFO monitorInfo = new MonitorUtil.MONITORINFO { cbSize = Marshal.SizeOf(typeof(MonitorUtil.MONITORINFO)) };
                    MonitorUtil.GetMonitorInfo(monitor, ref monitorInfo);
                    MonitorUtil.RECT rcWorkArea     = monitorInfo.rcWork;
                    MonitorUtil.RECT rcMonitorArea  = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X     = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y     = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) + 3;
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }


        /// <summary>
        /// Set the window's size, position and state.
        /// </summary>
        public static WindowState SetWindowDimensions(Window window, WindowSizeParams? dimensions)
            {
            if (dimensions?.Size is not null)
            {
                window.Width  = dimensions.Size.Value.Width;
                window.Height = dimensions.Size.Value.Height;
            }

            if (dimensions?.Location is not null)
            {
                try
                {
                    //ensure top-left of app screen is visible
                    var loc = AdjustXY(new((int)dimensions.Location.Value.X, (int)dimensions.Location.Value.Y), new((int)window.Width, (int)window.Height), 0, 0);

                    window.Top  = loc.Y;
                    window.Left = loc.X;
                }
                catch { }
            }

            if (dimensions?.IsMaximised ?? false)
                return window.WindowState = WindowState.Maximized;

            return WindowState.Normal;
        }


        #region AdjustXY
        /// <summary>
        /// Adjust the window position to avoid it going out of screen bounds.<br/>
        /// Works for multi screens - with different aspect ratios.
        /// </summary>
        /// <remark><code>
        /// 1) topLeft - position of the top left at the desktop (works for multi screens - with different aspect ratio).
        ///    
        ///            Screen1              Screen2                                        
        ///        ─  ┌───────────────────┐┌───────────────────┐ Screen3                   
        ///        ▲  │                   ││                   │┌─────────────────┐  ─     
        ///        │  │                   ││                   ││   ▼-            │  ▲     
        ///   1080 │  │                   ││                   ││                 │  │     
        ///        │  │                   ││                   ││                 │  │ 900 
        ///        ▼  │                   ││                   ││                 │  ▼     
        ///        ─  └──────┬─────┬──────┘└──────┬─────┬──────┘└──────┬────┬─────┘  ─     
        ///                 ─┴─────┴─            ─┴─────┴─            ─┴────┴─             
        ///           │◄─────────────────►││◄─────────────────►││◄───────────────►│        
        ///                   1920                 1920                1440                
        ///    If the mouse is in Screen3 a possible value might be:                        
        ///    topLeft.X=4140 topLeft.Y=195                                                 
        /// 2) offset - the offset from the top left, one value for both X and Y directions.
        /// 3) maxSize - the maximal size of the window - including its size when it is expanded - from the following
        ///    example we need maxSize.X = 200, maxSize.Y = 150 - To avoid the expansion being out of bounds.
        ///
        ///   Non expanded window:                                                         
        ///   ┌──────────────────────────────┐ ─                                           
        ///   │ Window Name               [X]│ ▲                                           
        ///   ├──────────────────────────────┤ │                                           
        ///   │         ┌─────────────────┐  │ │ 100                                       
        ///   │  Text1: │                 │  │ │                                           
        ///   │         └─────────────────┘  │ │                                           
        ///   │                         [▼]  │ ▼                                           
        ///   └──────────────────────────────┘ ─                                           
        ///   │◄────────────────────────────►│                                             
        ///                 200                                                            
        ///
        ///   Expanded window:                                                             
        ///   ┌──────────────────────────────┐ ─                                           
        ///   │ Window Name               [X]│ ▲                                           
        ///   ├──────────────────────────────┤ │                                           
        ///   │         ┌─────────────────┐  │ │                                           
        ///   │  Text1: │                 │  │ │                                           
        ///   │         └─────────────────┘  │ │ 150                                       
        ///   │                         [▲]  │ │                                           
        ///   │         ┌─────────────────┐  │ │                                           
        ///   │  Text2: │                 │  │ │                                           
        ///   │         └─────────────────┘  │ ▼                                           
        ///   └──────────────────────────────┘ ─                                           
        ///   │◄────────────────────────────►│                                             
        ///                 200                                                            
        /// 4) margin - The distance the window should be from the screen work-area - Example:
        /// 
        ///   ┌─────────────────────────────────────────────────────────────┐ ─            
        ///   │                                                             │ ↕ Margin     
        ///   │                                                             │ ─            
        ///   │                                                             │              
        ///   │                                                             │              
        ///   │                                                             │              
        ///   │                          ┌──────────────────────────────┐   │              
        ///   │                          │ Window Name               [X]│   │              
        ///   │                          ├──────────────────────────────┤   │              
        ///   │                          │         ┌─────────────────┐  │   │              
        ///   │                          │  Text1: │                 │  │   │              
        ///   │                          │         └─────────────────┘  │   │              
        ///   │                          │                         [▲]  │   │              
        ///   │                          │         ┌─────────────────┐  │   │              
        ///   │                          │  Text2: │                 │  │   │              
        ///   │                          │         └─────────────────┘  │   │              
        ///   │                          └──────────────────────────────┘   │ ─            
        ///   │                                                             │ ↕ Margin     
        ///   ├──────────────────────────────────────────────────┬──────────┤ ─            
        ///   │[start] [♠][♦][♣][♥]                              │en│ 12:00 │              
        ///   └──────────────────────────────────────────────────┴──────────┘              
        ///   │◄─►│                                                     │◄─►│              
        ///    Margin                                                    Margin            
        ///
        /// * Note that this simple algorithm will always want to leave the cursor          
        ///   out of the window, therefore the window will jump to its left:                
        ///  ┌─────────────────────────────────┐        ┌─────────────────────────────────┐
        ///  │                  ▼-┌──────────────┐      │  ┌──────────────┐▼-             │
        ///  │                    │ Window    [X]│      │  │ Window    [X]│               │
        ///  │                    ├──────────────┤      │  ├──────────────┤               │
        ///  │                    │       ┌───┐  │      │  │       ┌───┐  │               │
        ///  │                    │  Val: │   │  │ ->   │  │  Val: │   │  │               │
        ///  │                    │       └───┘  │      │  │       └───┘  │               │
        ///  │                    └──────────────┘      │  └──────────────┘               │
        ///  │                                 │        │                                 │
        ///  ├──────────────────────┬──────────┤        ├──────────────────────┬──────────┤
        ///  │[start] [♠][♦][♣]     │en│ 12:00 │        │[start] [♠][♦][♣]     │en│ 12:00 │
        ///  └──────────────────────┴──────────┘        └──────────────────────┴──────────┘
        ///  If this is not a requirement, you can add a parameter to just use             
        ///  the margin:                                                                   
        ///  ┌─────────────────────────────────┐        ┌─────────────────────────────────┐
        ///  │                  ▼-┌──────────────┐      │                ┌─▼-───────────┐ │
        ///  │                    │ Window    [X]│      │                │ Window    [X]│ │
        ///  │                    ├──────────────┤      │                ├──────────────┤ │
        ///  │                    │       ┌───┐  │      │                │       ┌───┐  │ │
        ///  │                    │  Val: │   │  │ ->   │                │  Val: │   │  │ │
        ///  │                    │       └───┘  │      │                │       └───┘  │ │
        ///  │                    └──────────────┘      │                └──────────────┘ │
        ///  │                                 │        │                                 │
        ///  ├──────────────────────┬──────────┤        ├──────────────────────┬──────────┤
        ///  │[start] [♠][♦][♣]     │en│ 12:00 │        │[start] [♠][♦][♣]     │en│ 12:00 │
        ///  └──────────────────────┴──────────┘        └──────────────────────┴──────────┘
        /// * Supports also the following scenarios:
        ///  1) Screen over screen:
        ///       ┌─────────────────┐  
        ///       │                 │
        ///       │                 │
        ///       │                 │
        ///       │                 │
        ///       └─────────────────┘
        ///     ┌───────────────────┐ 
        ///     │                   │ 
        ///     │  ▼-               │ 
        ///     │                   │ 
        ///     │                   │ 
        ///     │                   │ 
        ///     └──────┬─────┬──────┘ 
        ///           ─┴─────┴─       
        ///  2) Window bigger than screen height or width
        ///     ┌─────────────────────────────────┐        ┌─────────────────────────────────┐ 
        ///     │                                 │        │ ┌──────────────┐                │
        ///     │                                 │        │ │ Window    [X]│                │
        ///     │                  ▼-┌────────────│─┐      │ ├──────────────┤ ▼-             │
        ///     │                    │ Window    [│]│      │ │       ┌───┐  │                │
        ///     │                    ├────────────│─┤ ->   │ │  Val: │   │  │                │ 
        ///     │                    │       ┌───┐│ │      │ │       └───┘  │                │
        ///     │                    │  Val: │   ││ │      │ │       ┌───┐  │                │
        ///     │                    │       └───┘│ │      │ │  Val: │   │  │                │
        ///     ├──────────────────────┬──────────┤ │      ├──────────────────────┬──────────┤
        ///     │[start] [♠][♦][♣]     │en│ 12:00 │ │      │[start] [♠][♦][♣]     │en│ 12:00 │
        ///     └──────────────────────┴──────────┘ │      └──────────────────────┴──────────┘
        ///                          │       ┌───┐  │        │       └───┘  │
        ///                          │  Val: │   │  │        └──────────────┘
        ///                          │       └───┘  │
        ///                          └──────────────┘
        ///
        ///
        ///     ┌─────────────────────────────────┐             ┌─────────────────────────────────┐     
        ///     │                                 │             │                                 │ 
        ///     │                                 │             │ ┌───────────────────────────────│───┐
        ///     │    ▼-┌──────────────────────────│────────┐    │ │ W▼-dow                        │[X]│
        ///     │      │ Window                   │     [X]│    │ ├───────────────────────────────│───┤
        ///     │      ├──────────────────────────│────────┤    │ │       ┌───┐      ┌───┐      ┌─┤─┐ │
        ///     │      │       ┌───┐      ┌───┐   │  ┌───┐ │ -> │ │  Val: │   │ Val: │   │ Val: │ │ │ │
        ///     │      │  Val: │   │ Val: │   │ Va│: │   │ │    │ │       └───┘      └───┘      └─┤─┘ │
        ///     │      │       └───┘      └───┘   │  └───┘ │    │ └───────────────────────────────│───┘
        ///     ├──────────────────────┬──────────┤────────┘    ├──────────────────────┬──────────┤
        ///     │[start] [♠][♦][♣]     │en│ 12:00 │             │[start] [♠][♦][♣]     │en│ 12:00 │     
        ///     └──────────────────────┴──────────┘             └──────────────────────┴──────────┘     
        /// </code></remark>
        /// <param name="topLeft">The requiered possition without its offset</param>
        /// <param name="maxSize">The max possible size of the window</param>
        /// <param name="offset">The offset of the topLeft postion</param>
        /// <param name="margin">The margin from the screen</param>
        /// <returns>The adjusted position of the window</returns>
        internal static System.Drawing.Point AdjustXY(System.Drawing.Point topLeft, System.Drawing.Point maxSize, int offset, int margin)
        {
            Screen currentScreen = Screen.FromPoint(topLeft);
            Rectangle rect = currentScreen.WorkingArea;

            if (topLeft.Y < rect.Top)
                topLeft.Y = rect.Top;
            if (topLeft.X < rect.Left)
                topLeft.X = rect.Left;

            // Set an offset from mouse position.
            topLeft.Offset(offset, offset);

            // Check if the window needs to go above the task bar, 
            // when the task bar shadows the HUD window.
            //int totalHeight = Math.Max(topLeft.Y, 0) + Math.Max(maxSize.Y, 0) + margin;
            int totalHeight = maxSize.Y + margin;

            if (topLeft.Y + totalHeight > rect.Bottom)
            {
                topLeft.Y = Math.Max(rect.Bottom - maxSize.Y - 2 * offset - margin, rect.Top);

                // If the screen dimensions exceed the height of the window
                // set it just bellow the top bound.
                if (topLeft.Y < rect.Top)
                    topLeft.Y = rect.Top + margin;
            }

            //int totalWidth = Math.Max(topLeft.X, 0) + Math.Max(maxSize.X, 0) + margin;
            int totalWidth = maxSize.X + margin;

            // Check if the window needs to move to the left of the mouse, 
            // when the HUD exceeds the right window bounds.
            if (topLeft.X + totalWidth > rect.Right)
            {
                // Since we already set an offset remove it and add the offset to
                // the other side of the mouse (2x) in addition include the margin.
                topLeft.X = Math.Max(rect.Right - maxSize.X - 2 * offset - margin, rect.Left);

                // If the screen dimensions exceed the width of the window
                // don't exceed the left bound.
                if (topLeft.X < rect.Left)
                    topLeft.X = rect.Left + margin;
            }

            return topLeft;
        }
        #endregion
    }
}
