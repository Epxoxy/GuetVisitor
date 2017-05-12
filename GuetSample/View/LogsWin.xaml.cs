using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for LogsWin.xaml
    /// </summary>
    public partial class LogsWin : Window
    {
        public LogsWin()
        {
            InitializeComponent();
            this.Loaded += LogsWinLoaded;
        }

        private void LogsWinLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= LogsWinLoaded;
            this.Unloaded += OnLogWinUnloaded;
            XLog.OnLogUpdated += XLogOnLogUpdated;
            contentRoot.ManipulationBoundaryFeedback += StopFeedback;
            this.logTb.Text = XLog.logBuilder.ToString();
        }

        private void OnLogWinUnloaded(object sender, RoutedEventArgs e)
        {
            contentRoot.ManipulationBoundaryFeedback -= StopFeedback;
            this.Unloaded -= OnLogWinUnloaded;
            XLog.OnLogUpdated -= XLogOnLogUpdated;
        }

        //Disable the ManipulationBoundaryFeedback event to prevent window shake.
        private void StopFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void XLogOnLogUpdated(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.logTb.Text = XLog.logBuilder.ToString();
            }));
        }
    }
}
