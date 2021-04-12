using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrezorBalanceMonitoring
{
    class MonitoredCoinSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public MonitoredCoinCollection MonitoredCoins
        {
            get
            {
                return (MonitoredCoinCollection)this[""];
            }
            set
            {
                this[""] = value;
            }
        }
    }
}
