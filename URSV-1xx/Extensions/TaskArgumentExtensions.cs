using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URSV1xx.Extensions
{
    internal static class TaskArgumentExtensions
    {
        public static bool TryGetAddress(this IReadOnlyDictionary<string, string> aArguments, out byte aNetworkAddress)
        {
            try
            {
                if(aArguments.TryGetValue("NetWorkAddress", out string netAdd))
                {
                    aNetworkAddress = (byte)Convert.ToInt32(netAdd);
                    return true;
                }
                aNetworkAddress = 0;
                return false;
            } catch(ArgumentNullException)
            {
                aNetworkAddress = 0;
                return false;
            }
        }
    }
}
