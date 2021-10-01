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
        }


        public string ProgressBarLegend { set => txtLegend.Text = value; }

        public int    ProgressBarMax    { get;    set; }


        public void UpdateProgress(int value)
        {
            pbLoad.Value = (double)value / ProgressBarMax * 100;
            txtProgress.Text = value + " / " + ProgressBarMax;

            // When progress reaches 100%, close the progress bar window.
            if (value >= ProgressBarMax)
            {
                Thread.Sleep(500);
                Hide();
            }
        }
    }
}
