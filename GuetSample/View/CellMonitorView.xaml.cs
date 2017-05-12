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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuetSample
{
    /// <summary>
    /// Interaction logic for MonitoringTask.xaml
    /// </summary>
    public partial class CellMonitorView : UserControl, INotifyPropertyChanged
    {
        public CellMonitorView()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded += OnLoaded;
            this.root.DataContext = this;
        }

        private CellMonitor _cellMonitor;
        public CellMonitor CellMonitor
        {
            get { return _cellMonitor; }
            set
            {
                _cellMonitor = value;
                RaisePropertyChanged(nameof(CellMonitor));
            }
        }

        public HttpFireTask FireTask
        {
            get
            {
                if (CellMonitor.FireTask == null)
                    CellMonitor.FireTask = new HttpFireTask();
                return CellMonitor.FireTask;
            }
            set
            {
                CellMonitor.FireTask = value;
                RaisePropertyChanged(nameof(FireTask));
            }
        }
        
        private void createDefaultBtnClick(object sender, RoutedEventArgs e)
        {
            FireTask = HttpFireTask.CreateDefault();
        }

        private void clearTaskClick(object sender, RoutedEventArgs e)
        {
            FireTask.Url = null;
            FireTask.Data = null;
            FireTask.TypeOfPost = false;
            RaisePropertyChanged(nameof(FireTask));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
