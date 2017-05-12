using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    public class ExecutableContent<T>
    {
        public T Value { get; set; }
        public System.Windows.Input.ICommand Command { get; set; }
    }
}
