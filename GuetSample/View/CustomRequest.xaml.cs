using System;
using System.Windows;
using System.Windows.Input;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for CustomRequestWin.xaml
    /// </summary>
    public partial class CustomRequest : Window
    {
        private const string token = "Custom";
        public CustomRequest()
        {
            InitializeComponent();
            this.Loaded += OnThisLoaded;
        }

        private void OnThisLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnThisLoaded;
            this.Unloaded += OnThisUnloaded;
            contentRoot.ManipulationBoundaryFeedback += StopFeedback;
            MessengerLight.Messenger.Default.Register<DialogContent>(this, token, onCallDialog);
        }

        private void OnThisUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= OnThisUnloaded;
            contentRoot.ManipulationBoundaryFeedback -= StopFeedback;
            MessengerLight.Messenger.Default.Unregister<DialogContent>(this);
        }

        //Disable the ManipulationBoundaryFeedback event to prevent window shake.
        private void StopFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void ensureActive()
        {
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            if (!this.IsActive)
                this.Activate();
        }

        private void onCallDialog(DialogContent content)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ensureActive();
                var dialog = new Epxoxy.Controls.MessageDialog(this)
                {
                    Title = content.Title,
                    Content = content.Content
                };
                System.Media.SystemSounds.Beep.Play();
                dialog.ShowDialog();
            }));
        }
    }
}
