using System;
using System.ComponentModel;

namespace GuetSample
{
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NotificationObjectEx : NotificationObject
    {
        public NotificationObjectEx()
        {
            this.OnInitCommands();
        }

        protected virtual void OnInitCommands()
        {

        }
    }
}
