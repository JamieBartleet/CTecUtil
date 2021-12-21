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


        public void Show(Window owner)
        {
            if (Application.Current.MainWindow != null)
            {
                Top  = Application.Current.MainWindow.Top  + Application.Current.MainWindow.ActualHeight / 2 - Height / 2;
                Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.ActualWidth  / 2 - Width  / 2;
            }
            
            base.Owner = owner;
            base.Show();
        }


        public string ProgressBarLegend { set => txtOperationName.Text = value; }

        public int ProgressBarOverallMax  { get; set; }

        public int ProgressBarSubqueueMax { get => (int)pbProgressSubqueue.Maximum; set => pbProgressSubqueue.Maximum = value; }

        public int SubqueueCount { set => pbProgressSubqueue.Visibility = txtProgressSubqueue.Visibility = value > 1 ? Visibility.Visible : Visibility.Collapsed; }


        public delegate void CancelHandler();

        /// <summary>
        /// Delegate called when Cancel button is clicked - assign this if any clean-up is required on cancellation.
        /// </summary>
        public CancelHandler OnCancel;


        public void UpdateProgress(List<string> subqueueNames, int valueOverall, int valueSubqueue)
        {
            txtSubqueueName.Text = subqueueNames.Count > 0 ? subqueueNames[0] : "";
            txtNext1.Text        = subqueueNames.Count > 1 ? subqueueNames[1] : "";
            txtNext2.Text        = subqueueNames.Count > 2 ? subqueueNames[2] : "";
            txtNext3.Text        = subqueueNames.Count > 3 ? subqueueNames[3] : "";
            txtNext4.Text        = subqueueNames.Count > 4 ? subqueueNames[4] : "";
            pbProgressOverall.Value = (double)valueOverall / ProgressBarOverallMax * 100;
            pbProgressSubqueue.Value = valueSubqueue;
            txtProgressSubqueue.Text = valueSubqueue + " / " + ProgressBarSubqueueMax;

            // When progress reaches 100%, close the progress bar window.
            if (valueOverall >= ProgressBarOverallMax)
                Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => OnCancel?.Invoke();
    }
}
