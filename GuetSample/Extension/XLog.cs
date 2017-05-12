using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    class XLog
    {
        public readonly static StringBuilder logBuilder = new StringBuilder();
        public static event EventHandler OnLogUpdated;

        public static void Log(string value)
        {
            logBuilder.Append(value);
            System.Diagnostics.Debug.Write(value);
            OnLogUpdated?.Invoke(logBuilder, EventArgs.Empty);
        }

        public static void LogLine(string value)
        {
            logBuilder.AppendLine(value);
            System.Diagnostics.Debug.WriteLine(value);
            OnLogUpdated?.Invoke(logBuilder, EventArgs.Empty);
        }
    }
}
