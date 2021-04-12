using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrezorBalanceMonitoring
{
    class MonitoredCoinElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)base["key"];
            }
            set
            {
                base["key"] = value;
            }
        }

        [ConfigurationProperty("coinId", IsRequired = false)]
        public string CoinID
        {
            get
            {
                return (string)base["coinId"];
            }
            set
            {
                base["coinId"] = value;
            }
        }

        [ConfigurationProperty("coinName", IsRequired = false)]
        public string CoinName
        {
            get
            {
                return (string)base["coinName"];
            }
            set
            {
                base["coinName"] = value;
            }
        }

        [ConfigurationProperty("address", IsRequired = false)]
        public string Address
        {
            get
            {
                return (string)base["address"];
            }
            set
            {
                base["address"] = value;
            }
        }

        [ConfigurationProperty("rpcendpoint", IsRequired = false)]
        public string RpcEndpoint
        {
            get
            {
                return (string)base["rpcendpoint"];
            }
            set
            {
                base["rpcendpoint"] = value;
            }
        }

        [ConfigurationProperty("rpcuser", IsRequired = false)]
        public string RpcUser
        {
            get
            {
                return (string)base["rpcuser"];
            }
            set
            {
                base["rpcuser"] = value;
            }
        }

        [ConfigurationProperty("rpcpassword", IsRequired = false)]
        public string RpcPassword
        {
            get
            {
                return (string)base["rpcpassword"];
            }
            set
            {
                base["rpcpassword"] = value;
            }
        }

        [ConfigurationProperty("altrpcmethod", IsRequired = false)]
        public string AlternateRpcMethod
        {
            get
            {
                return (string)base["altrpcmethod"];
            }
            set
            {
                base["altrpcmethod"] = value;
            }
        }

        [ConfigurationProperty("supported", IsRequired = false, DefaultValue = true)]
        public bool Supported
        {
            get
            {
                return (bool)base["supported"];
            }
            set
            {
                base["supported"] = value;
            }
        }

        [ConfigurationProperty("erc20", IsRequired = false, DefaultValue = false)]
        public bool Erc20
        {
            get
            {
                return (bool)base["erc20"];
            }
            set
            {
                base["erc20"] = value;
            }
        }
    }
}
