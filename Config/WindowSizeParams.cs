using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace CTecUtil.Config
{
    public class WindowSizeParams
    {
        public WindowSizeParams() { }

        public WindowSizeParams(Window window, double scale)
        {
            Location    = new Point((int)window.Left, (int)window.Top);
            Size        = new Size((int)window.Width, (int)window.Height);
            IsMaximised = window.WindowState == WindowState.Maximized;
            Scale       = scale;
            ApplicationConfig.Save = true;
        }

        
        private Point? _location;
        private Size?  _size;
        private bool   _isMaximised;
        private double _scale = 0.75;

        public Point? Location    { get => _location;    set { _location = value;    ApplicationConfig.Save = true; } }
        public Size?  Size        { get => _size;        set { _size = value;        ApplicationConfig.Save = true; } }
        public bool   IsMaximised { get => _isMaximised; set { _isMaximised = value; ApplicationConfig.Save = true; } }

        public double Scale
        {
            get => _scale;
            set
            {
                if (value >= UI.MinZoom && value <= UI.MaxZoom)
                {
                    _scale = value;
                    ApplicationConfig.Save = true;
                }
            }
        }
    }
}
