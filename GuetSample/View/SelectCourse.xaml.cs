using System;
using System.Windows;
using System.Windows.Input;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for SelectCourse.xaml
    /// </summary>
    public partial class SelectCourse : Window
    {
        private const string token = "HomeView";
        public SelectCourse()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.Unloaded += OnUnloaded;
            contentRoot.ManipulationBoundaryFeedback += StopFeedback;
            homePane.IsOpen = true;
            MessengerLight.Messenger.Default.Register<DialogContent>(this, token, onCallDialog);
            MessengerLight.Messenger.Default.Register<DataContextMsg>(this, token, onDataContextMsg);
            MessengerLight.Messenger.Default.Register<ExecutableContent<CellMonitor>>(this, token, onExecutableContentMsg);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= OnUnloaded;
            contentRoot.ManipulationBoundaryFeedback -= StopFeedback;
            MessengerLight.Messenger.Default.Unregister<DialogContent>(this);
            MessengerLight.Messenger.Default.Unregister<DataContextMsg>(this);
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

        private void newCustomClick(object sender, RoutedEventArgs e)
        {
            CustomRequest cwin = new CustomRequest();
            cwin.Show();
        }
        
        private void logsBtnClick(object sender, RoutedEventArgs e)
        {
            new LogsWin().Show();
        }
        
        private async void onEditMonitorClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as System.Windows.Controls.MenuItem;
            if (menuItem != null)
            {
                CellMonitor monitor = menuItem.CommandParameter as CellMonitor;
                if (monitor != null)
                {
                    CellMonitor editItem = (CellMonitor)monitor.Clone();
                    CellMonitor[] parameters = new CellMonitor[] { monitor, editItem };
                    ICommand saveCommand = menuItem.Tag as ICommand;
                    var dialog = new Epxoxy.Controls.ContentDialog()
                    {
                        PrimaryButtonText = "Save changes",
                        PrimaryButtonCommand = saveCommand,
                        PrimaryButtonCommandParameter = parameters,
                        SecondaryButtonText = "Cancel",
                        Title = "Edit CellMonitor",
                        Content = new CellMonitorView()
                        {
                            CellMonitor = editItem
                        }
                    };
                    await dialog.ShowAsync(this.Content as FrameworkElement);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CellMonitor null");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ICommandSource null");
            }
        }
        
        #region ------- Hand Message From Messenger

        private void onDataContextMsg(DataContextMsg msg)
        {
            if(msg.Message == "NewMonitorViewModel")
            {
                SubTaskWin win = new SubTaskWin(msg.DataContext);
                win.Show();
            }else if(msg.Message == "ShowHtmlBrowser")
            {
                string value = msg.DataContext.ToString();
                HtmlWin win = new HtmlWin(value);
                win.Show();
            }
        }

        private void onCallDialog(DialogContent content)
        {
            ViewWindowEx.CallDialogForReceiver(content, this);
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

        #endregion
    }
}
