using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for SubTaskWin.xaml
    /// </summary>
    public partial class SubTaskWin : Window
    {
        private const string token = "SubTask";
        IDisposable disposableData;
        public SubTaskWin(object dataContext)
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.DataContext = dataContext;
            disposableData = dataContext as IDisposable;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.Unloaded += OnUnloaded;
            contentRoot.ManipulationBoundaryFeedback += StopFeedback;
            MessengerLight.Messenger.Default.Register<DialogContent>(this, token, onCallDialog);
            MessengerLight.Messenger.Default.Register<ExecutableContent<CellMonitor>>(this, token, onExecutableContentMsg);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= OnUnloaded;
            contentRoot.ManipulationBoundaryFeedback -= StopFeedback;
            MessengerLight.Messenger.Default.Unregister<DialogContent>(this);
            MessengerLight.Messenger.Default.Unregister<ExecutableContent<CellMonitor>>(this);
        }

        //Disable the ManipulationBoundaryFeedback event to prevent window shake.
        private void StopFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void onDatagridCurrentCellChanged(object sender, EventArgs e)
        {
            if (datagrid.CurrentCell != null && datagrid.CurrentCell.Column != null)
            {
                colTb.Text = datagrid.CurrentCell.Column.DisplayIndex.ToString();
            }
        }

        private void onCallDialog(DialogContent content)
        {
            ViewWindowEx.CallDialogForReceiver(content, this);
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            if (disposableData != null) disposableData.Dispose();
            base.OnClosing(e);
        }

        private async void onExecutableContentMsg(ExecutableContent<CellMonitor> content)
        {
            if (null != content.Value)
            {
                CellMonitor modify = (CellMonitor)content.Value.Clone();
                CellMonitor[] parameters = new CellMonitor[] { content.Value, modify };
                ICommand command = content.Command;
                var dialog = new Epxoxy.Controls.ContentDialog()
                {
                    PrimaryButtonText = "Save changes",
                    PrimaryButtonCommand = command,
                    PrimaryButtonCommandParameter = parameters,
                    SecondaryButtonText = "Cancel",
                    Title = "Edit CellMonitor",
                    Content = new CellMonitorView()
                    {
                        CellMonitor = modify
                    }
                };
                await dialog.ShowAsync(contentRoot);
            }
        }

    }
}
