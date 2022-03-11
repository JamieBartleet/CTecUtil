using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTecUtil.UI
{
    public class WindowUtils
    {
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
        public static Point AdjustXY(Point topLeft, Point maxSize, int offset, int margin)
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
    }
}
