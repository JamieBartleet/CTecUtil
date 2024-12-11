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
            _updateTimer = new Timer() { Interval = 5000 };
            _updateTimer.Elapsed += updateTimerTick;
        }

        
        private Point? _location;
        private Size?  _size;
        private bool   _isMaximised;
        private double _scale = 0.75;
        private static Timer _updateTimer;

        public Point? Location    { get => _location;    set { _location = value;    timedUpdate(); } }
        public Size?  Size        { get => _size;        set { _size = value;        timedUpdate(); } }
        public bool   IsMaximised { get => _isMaximised; set { _isMaximised = value; timedUpdate(); } }

        public double Scale
        {
            get => _scale;
            set
            {
                if (value >= UI.MinZoom && value <= UI.MaxZoom)
                {
                    _scale = value;
                    timedUpdate();
                }
            }
        }


        private void timedUpdate()
        {
            if (!_updateTimer.Enabled)
                _updateTimer.Start();
        }

        private void updateTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            ApplicationConfig.SaveSettings();
            t i m e r.Stop();
        }

    }
}
