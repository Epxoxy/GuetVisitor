using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    [Serializable]
    public class CellMonitor :ICloneable
    {
        public string PrimaryKeyValue { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public string OriginValue { get; set; }
        public string NewValue { get; set; }
        public HttpFireTask FireTask { get; set; }

        public CellMonitor(string primaryKeyValue, int columnIndex, string columnName, string origin)
        {
            this.PrimaryKeyValue = primaryKeyValue;
            this.ColumnIndex = columnIndex;
            this.ColumnName = columnName;
            this.OriginValue = origin;
            this.NewValue = null;
        }
        public override string ToString()
        {
            return string.Format("[{0}]-[{1}], {2} -> {3}",
                PrimaryKeyValue, ColumnName,  OriginValue, NewValue);
        }

        public object Clone()
        {
            CellMonitor monitor = new CellMonitor(PrimaryKeyValue, ColumnIndex, ColumnName, OriginValue)
            {
                NewValue = NewValue
            };
            if (FireTask != null)
            {
                monitor.FireTask = new HttpFireTask()
                {
                    Url = FireTask.Url,
                    Data = FireTask.Data,
                    TypeOfPost = FireTask.TypeOfPost
                };
            }
            return monitor;
        }

        public void SyncWith(CellMonitor cellMonitor)
        {
            this.PrimaryKeyValue = cellMonitor.PrimaryKeyValue;
            this.ColumnIndex = cellMonitor.ColumnIndex;
            this.ColumnName = cellMonitor.ColumnName;
            this.OriginValue = cellMonitor.OriginValue;
            this.NewValue = cellMonitor.NewValue;
            this.FireTask = cellMonitor.FireTask;
        }

    }
}
