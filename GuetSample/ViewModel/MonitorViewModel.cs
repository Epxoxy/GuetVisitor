using MessengerLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace GuetSample.ViewModel
{
    public class MonitorViewModel : NotificationObjectEx
    {
        protected virtual string Token { get; set; }

        private const string DEF_SAVE_NAME = "monitors.dat";

        private string saveName;

        private BindingList<CellMonitor> _monitorItems;
        public BindingList<CellMonitor> MonitorItems
        {
            get
            {
                if (_monitorItems == null)
                    _monitorItems = new BindingList<CellMonitor>();
                return _monitorItems;
            }
            set
            {
                _monitorItems = value;
                RaisePropertyChanged(nameof(MonitorItems));
            }
        }

        public string PrimaryKey { get; set; } = "ClassNo";
        private DataTable _dataTable;
        public DataTable DataTable
        {
            get
            {
                return _dataTable;
            }
            set
            {
                if(_dataTable != value)
                {
                    _dataTable = value;
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        RaisePropertyChanged(nameof(DataTable));
                        OnDataChanged(value, MonitorItems);
                    });
                }
            }
        }

        public DelegateCommand AddMonitorCommand { get; set; }
        public DelegateCommand RemoveMonitorCommand { get; set; }
        public DelegateCommand LoadMonitorCommand { get; set; }
        public DelegateCommand SaveMonitorCommand { get; set; }
        public DelegateCommand SaveMonitorChangesCommand { get; set; }
        public DelegateCommand CallEditMonitorCommand { get; set; }

        public MonitorViewModel() : this(DEF_SAVE_NAME) { }

        public MonitorViewModel(string saveName)
        {
            this.saveName = saveName;
        }

        protected override void OnInitCommands()
        {
            base.OnInitCommands();
            AddMonitorCommand = new DelegateCommand();
            RemoveMonitorCommand = new DelegateCommand();
            SaveMonitorCommand = new DelegateCommand();
            LoadMonitorCommand = new DelegateCommand();
            SaveMonitorChangesCommand = new DelegateCommand();
            CallEditMonitorCommand = new DelegateCommand();

            AddMonitorCommand.ExecuteCommand += o =>
            {
                object[] values = o as object[];
                if (values != null && values.Length == 2)
                {
                    int col, row;
                    if (int.TryParse(values[0].ToString(), out col)
                    && int.TryParse(values[1].ToString(), out row))
                    {
                        AddToMonitoring(row, col);
                    }
                }
            };
            RemoveMonitorCommand.ExecuteCommand += o =>
            {
                if (o is CellMonitor)
                {
                    CellMonitor item = (CellMonitor)o;
                    if (MonitorItems.Contains(item))
                    {
                        MonitorItems.Remove(item);
                    }
                }
            };
            LoadMonitorCommand.ExecuteCommand += o =>
            {
                object obj = SerializeHelper.DeserializeDefault(saveName);
                if (obj != null)
                {
                    BindingList<CellMonitor> items = obj as BindingList<CellMonitor>;
                    if (items != null) this.MonitorItems = items;
                }
            };
            SaveMonitorCommand.ExecuteCommand += o =>
            {
                SerializeHelper.SerializeDefault(saveName, MonitorItems);
            };
            SaveMonitorChangesCommand.ExecuteCommand += o =>
            {
                object obj = o;
                CellMonitor[] values = obj as CellMonitor[];
                if (values != null && values.Length == 2)
                {
                    SaveChangesTo(values[0], values[1]);
                }
            };
            CallEditMonitorCommand.ExecuteCommand += o =>
            {
                if(null != o)
                {
                    CellMonitor item = o as CellMonitor;
                    if(null != item)
                    {
                        ExecutableContent<CellMonitor> executeable = new ExecutableContent<CellMonitor>()
                        {
                            Value = item,
                            Command = SaveMonitorChangesCommand
                        };
                        Messenger.Default.Send(executeable, Token);
                    }
                }
            };
        }
        
        private CellMonitor AddToMonitoring(DataTable data, int rowIndex, int columnIndex)
        {
            //Validate
            if (data == null || rowIndex < 0 || columnIndex < 0) return null;
            if (data.Rows.Count <= rowIndex || data.Columns.Count <= columnIndex) return null;
            if (data.Columns.Contains(PrimaryKey))
            {
                //Get values
                string primary = data.Rows[rowIndex][PrimaryKey].ToString();
                string columnName = data.Columns[columnIndex].ColumnName;
                string orgin = data.Rows[rowIndex][columnIndex].ToString();
                //Add
                var item = new CellMonitor(primary, columnIndex, columnName, orgin);
                MonitorItems.Add(item);
                return item;
            }
            return null;
        }

        protected CellMonitor AddToMonitoring(string primaryValue, int columnIndex)
        {
            var data = this.DataTable;
            if (data == null || columnIndex < 0 || data.Columns.Count <= columnIndex) return null;
            if (data.Columns.Contains(PrimaryKey))
            {
                //Get values
                string columnName = data.Columns[columnIndex].ColumnName;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    if(data.Rows[i][PrimaryKey].ToString() == primaryValue)
                    {
                        string orgin = data.Rows[i][columnIndex].ToString();
                        //Add
                        var item = new CellMonitor(primaryValue, columnIndex, columnName, orgin);
                        MonitorItems.Add(item);
                        return item;
                    }
                }
            }
            return null;
        }

        protected CellMonitor AddToMonitoring(int rowIndex, int columnIndex)
        {
            return this.AddToMonitoring(DataTable, rowIndex, columnIndex);
        }

        private void OnDataChanged(DataTable data, BindingList<CellMonitor> items)
        {
            if(data == null || data.Rows.Count <= 0 || data.Columns.Count <= 0)
            {
                if(items.Count > 0)  OnItemChanged(new CellMonitor[] { }, items);
            }
            else
            {
                int rowCount = data.Rows.Count;
                int columnCount = data.Columns.Count;
                List<CellMonitor> changedList = new List<CellMonitor>();
                List<CellMonitor> removedList = new List<CellMonitor>();
                for (int i = 0; i < items.Count; i++)
                {
                    CellMonitor item = items[i];
                    if (item == null) continue;
                    var row = data.Select($"{PrimaryKey} = {item.PrimaryKeyValue}");
                    if(row == null || row.Length <= 0 || row.Length >= item.ColumnIndex)
                    {
                        removedList.Add(item);
                    }
                    else
                    {
                        string newValue = row[0].ItemArray[item.ColumnIndex].ToString();
                        if (!newValue.Equals(item.OriginValue))
                        {
                            item.NewValue = newValue;
                            changedList.Add((CellMonitor)item.Clone());
                            item.OriginValue = newValue;
                        }
                        else
                        {
                            XLog.LogLine($"No change of {item.OriginValue} -> {newValue}.");
                        }
                    }
                }
                if (changedList.Count > 0 || removedList.Count > 0)
                    OnItemChanged(changedList, removedList);
            }
        }

        public void EmptyTableWithoutMonitoring()
        {
            _dataTable = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged(nameof(DataTable));
            });
        }

        protected virtual void OnItemChanged(IEnumerable<CellMonitor> changedList, IEnumerable<CellMonitor> removedList)
        {
        }

        private void SaveChangesTo(CellMonitor src, CellMonitor changed)
        {
            int index = MonitorItems.IndexOf(src);
            if(index >= 0)
            {
                MonitorItems[index] = changed;
            }
        }

        public void AddMonitorFromFile(string filePath)
        {
            using (System.IO.Stream stream = System.IO.File.Open(filePath, System.IO.FileMode.Open))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(stream);
                var rootNodeList = doc.SelectSingleNode("cellmonitors").ChildNodes;
                foreach (System.Xml.XmlNode childNode in rootNodeList)
                {
                    if (childNode.Name == "cellmonitor")
                    {
                        string primary = string.Empty;
                        string columnIndexValue = string.Empty;
                        foreach (System.Xml.XmlAttribute attribute in childNode.Attributes)
                        {
                            switch (attribute.Name)
                            {
                                case "primaryKeyValue": primary = attribute.Value; break;
                                case "columnIndex": columnIndexValue = attribute.Value; break;
                            }
                        }
                        if (string.IsNullOrEmpty(primary) || string.IsNullOrEmpty(columnIndexValue)) continue;
                        int columnIndex = -1;
                        if (int.TryParse(columnIndexValue, out columnIndex))
                        {
                            if (columnIndex < 0) continue;
                            HttpFireTask fireTask = null;
                            if (childNode.ChildNodes.Count > 0)
                            {
                                string url = string.Empty;
                                string data = string.Empty;
                                string typeofpost = string.Empty;
                                foreach (System.Xml.XmlAttribute attribute in childNode.ChildNodes[0].Attributes)
                                {
                                    switch (attribute.Name)
                                    {
                                        case "url": url = attribute.Value; break;
                                        case "data": data = attribute.Value; break;
                                        case "typeofpost": typeofpost = attribute.Value; break;
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(typeofpost))
                                {
                                    bool isPost = false;
                                    if (bool.TryParse(typeofpost, out isPost))
                                    {
                                        fireTask = new HttpFireTask()
                                        {
                                            Url = url,
                                            Data = data,
                                            TypeOfPost = isPost
                                        };
                                    }
                                    else continue;
                                }
                            }
                            this.AddToMonitoring(primary, columnIndex).FireTask = fireTask;
                        }
                    }
                }
            }
        }
    }
}
