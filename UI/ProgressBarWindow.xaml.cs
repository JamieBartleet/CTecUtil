using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CTecUtil.UI
{
    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
            Topmost = true;
        }


        public string ProgressBarLegend { set => txtOperationName.Text = value; }

        public int    ProgressBarMax    { get;    set; }


        public delegate void CancelHandler();

        /// <summary>
        /// Delegate called when Cancel button is clicked - assign this if any clean-up is required on cancellation.
        /// </summary>
        public CancelHandler OnCancel;


        public void UpdateProgress(string processName, int value)
        {
            txtProcessName.Text = processName;
            pbProgress.Value = (double)value / ProgressBarMax * 100;
            txtProgress.Text = value + " / " + ProgressBarMax;

            // When progress reaches 100%, close the progress bar window.
            if (value >= ProgressBarMax)
            {
                Thread.Sleep(500);
                Hide();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) => OnCancel?.Invoke();
    }
}
