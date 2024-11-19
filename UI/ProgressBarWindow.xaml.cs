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
using System.Windows.Threading;

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


        internal void Show(Window owner)
        {
            CTecUtil.Debug.WriteLine(">>> ProgressBarWindow.Show() <<<");
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Show();
        }


        internal string ProgressBarLegend { set => txtOperationName.Text = value; get => txtOperationName.Text; }

        internal int ProgressBarOverallMax  { get; set; }

        internal int ProgressBarSubqueueMax { get => (int)pbProgressSubqueue.Maximum; set => pbProgressSubqueue.Maximum = value; }

        internal int SubqueueCount { set => pbProgressSubqueue.Visibility = txtProgressSubqueue.Visibility = value > 1 ? Visibility.Visible : Visibility.Collapsed; }


        internal delegate void CancelHandler();

        /// <summary>
        /// Delegate called when Cancel button is clicked - assign this if any clean-up is required on cancellation.
        /// </summary>
        internal CancelHandler OnCancel;


        internal void UpdateProgress(List<string> subqueueNames, int valueOverall, int valueSubqueue)
        {
            try
            {
                Debug.WriteLine("  ProgressBarWindow.UpdateProgress() - start");
                if ((subqueueNames?.Count ?? 0) > 0 && txtSubqueueName.Text != subqueueNames[0])
                {
                    txtSubqueueName.Visibility = Visibility.Hidden;
                    stpQueue.Visibility = Visibility.Hidden;
                    System.Timers.Timer tx = new(100) { AutoReset = false, Enabled = true };
                    tx.Elapsed += new((s, e) => Application.Current.Dispatcher.Invoke(new Action(() => updateText(subqueueNames))));
                }

                pbProgressOverall.Value = (double)valueOverall / ProgressBarOverallMax * 100;
                pbProgressSubqueue.Value = valueSubqueue;
                txtProgressSubqueue.Text = valueSubqueue + " / " + ProgressBarSubqueueMax;

                // When progress reaches 100%, close the progress bar window.
                if (valueOverall >= ProgressBarOverallMax)
                    Hide();
                Debug.WriteLine("  ProgressBarWindow.UpdateProgress() - end");
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private void updateText(List<string> subqueueNames)
        {
            txtSubqueueName.Visibility = Visibility.Visible;
            txtSubqueueName.Text = subqueueNames.Count > 0 ? subqueueNames[0] : "";
            txtNext1.Text        = subqueueNames.Count > 1 ? subqueueNames[1] : "";
            txtNext2.Text        = subqueueNames.Count > 2 ? subqueueNames[2] : "";
            txtNext3.Text        = subqueueNames.Count > 3 ? subqueueNames[3] : "";
            txtNext4.Text        = subqueueNames.Count > 4 ? subqueueNames[4] : "";
            txtNext5.Text        = subqueueNames.Count > 5 ? subqueueNames[5] : "";
            stpQueue.Visibility = Visibility.Visible;
        }


        private void Button_Click(object sender, RoutedEventArgs e) => OnCancel?.Invoke();
    }
}
