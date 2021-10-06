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

            var location = Registry.ReadMessageBoxPosition();
            if (location.X != 0 && location.Y != 0)
            {
                Left = location.X;
                Top = location.Y;
            }
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


        public void UpdateProgress(string subqueueName, int valueOverall, int valueSubqueue)
        {
            txtSubqueueName.Text = subqueueName;
            pbProgressOverall.Value = (double)valueOverall / ProgressBarOverallMax * 100;
            pbProgressSubqueue.Value = valueSubqueue;
            txtProgressSubqueue.Text = valueSubqueue + " / " + ProgressBarSubqueueMax;

            // When progress reaches 100%, close the progress bar window.
            if (valueOverall >= ProgressBarOverallMax)
                Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => OnCancel?.Invoke();


        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => Registry.SaveMessageBoxPosition(this);
    }
}
