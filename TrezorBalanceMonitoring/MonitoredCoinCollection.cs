using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrezorBalanceMonitoring
{
    class MonitoredCoinCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MonitoredCoinElement();
        }


        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MonitoredCoinElement)element).Key;

        }
    }
}
