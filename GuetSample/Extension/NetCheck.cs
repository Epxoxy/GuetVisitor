using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuetSample
{
    public class NetCheck
    {
        public static bool CheckCanConnect(string hostNameOrAddress)
        {
            try
            {
                System.Net.Dns.GetHostEntry(hostNameOrAddress);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
