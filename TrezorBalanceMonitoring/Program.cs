using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace TrezorBalanceMonitoring
{
    class Program
    {
        static Dictionary<String, double> configuredCoinList = new Dictionary<string, double>();
        static string coinMarketCapApiKey = "";
        static int refreshInterval = 30;
        static int fiatRefreshInterval = 900;
        static DateTime exchangeRateDateTime = DateTime.Now;
        static double globalFiatAmount = 0.00;

        static void Main(string[] args)
        {
            try
            {
                coinMarketCapApiKey = ConfigurationManager.AppSettings["CoinMarketCapApiKey"];
                refreshInterval = Convert.ToInt32(ConfigurationManager.AppSettings["BalanceRefreshInterval"]);
                fiatRefreshInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ExchangeRateRefreshInterval"]);
                
                var refreshConfig = Convert.ToBoolean(ConfigurationManager.AppSettings["RefreshFromTrezorFirmware"]);
                if (refreshConfig)
                {
                    GenerateConfig();
                }

                SetupConfiguredCoinList();
                GetFiatData();
                RefreshBalances();
                ConfigureThreads();
                Console.WriteLine("Press <enter> to exit...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ConfigureThreads()
        {
            Timer refreshBalanaceTimer = new Timer();
            refreshBalanaceTimer.Elapsed += RefreshBalanaceTimer_Elapsed;
            refreshBalanaceTimer.Interval = refreshInterval * 1000;
            refreshBalanaceTimer.Start();

            Timer refreshFiatTimer = new Timer();
            refreshFiatTimer.Elapsed += RefreshFiatTimer_Elapsed;
            refreshFiatTimer.Interval = fiatRefreshInterval * 1000;
            refreshFiatTimer.Start();
        }

        private static void RefreshFiatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = (Timer)sender;
            timer.Stop();
            GetFiatData();
            timer.Start();
        }

        private static void RefreshBalanaceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = (Timer)sender;
            timer.Stop();
            RefreshBalances();
            timer.Start();
        }

        private static void GenerateConfig()
        {
            Console.WriteLine("Refreshing Trezor Info");
            var trezorFirmwareRepo = "https://github.com/trezor/trezor-firmware.git";
            var ethChainsRepo = "https://github.com/ethereum-lists/chains.git";
            var ethTokensRepo = "https://github.com/ethereum-lists/tokens.git";
            var trezorFirmwareWorkingDirectory = "trezorFirmware";
            var ethChainsWorkingDirectory = "ethChains";
            var ethTokensWorkingDirectory = "ethTokens";
            try
            {
                if (Directory.Exists(trezorFirmwareWorkingDirectory))
                {
                    ForceDeleteDirectory(trezorFirmwareWorkingDirectory);
                }

                if (Directory.Exists(ethChainsWorkingDirectory))
                {
                    ForceDeleteDirectory(ethChainsWorkingDirectory);
                }

                if (Directory.Exists(ethTokensWorkingDirectory))
                {
                    ForceDeleteDirectory(ethTokensWorkingDirectory);
                }
                LibGit2Sharp.Repository.Clone(trezorFirmwareRepo, trezorFirmwareWorkingDirectory);
                LibGit2Sharp.Repository.Clone(ethChainsRepo, ethChainsWorkingDirectory);
                LibGit2Sharp.Repository.Clone(ethTokensRepo, ethTokensWorkingDirectory);
                var monitoredCoinSection = ConfigurationManager.GetSection("Coins") as MonitoredCoinSection;

                //trezor - bitcoin types
                var bitCoinDirectoryInfo = new DirectoryInfo(Path.Combine(trezorFirmwareWorkingDirectory, "common", "defs", "bitcoin"));
                foreach (var bitcoinConfigFile in bitCoinDirectoryInfo.GetFiles("*.json"))
                {
                    using (var reader = bitcoinConfigFile.OpenText())
                    {
                        var configContents = reader.ReadToEnd();
                        dynamic configData = JsonConvert.DeserializeObject(configContents);
                        var coinName = configData.coin_name.Value;
                        var coinId = configData.coin_shortcut.Value;
                        var key = coinId;
                        if (coinName.ToUpper().Contains("TESTNET"))
                        {
                            key += ".TESTNET";
                        }

                        var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                        if (alreadyMonitoredConfig == null)
                        {
                            var xmlDoc = new XmlDocument();
                            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            var nodeRegion = xmlDoc.CreateElement("add");
                            nodeRegion.SetAttribute("key", key);
                            nodeRegion.SetAttribute("coinId", coinId);
                            nodeRegion.SetAttribute("coinName", coinName);
                            nodeRegion.SetAttribute("address", String.Empty);
                            xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                        }
                    }
                }

                //trezor - misc types
                var miscDirectoryInfo = new DirectoryInfo(Path.Combine(trezorFirmwareWorkingDirectory, "common", "defs", "misc"));
                foreach (var miscConfigFile in miscDirectoryInfo.GetFiles("*.json"))
                {
                    using (var reader = miscConfigFile.OpenText())
                    {
                        var configContents = reader.ReadToEnd();
                        dynamic configData = JsonConvert.DeserializeObject(configContents);
                        foreach (var configItem in configData)
                        {
                            var coinName = configItem.name.Value;
                            var coinId = configItem.shortcut.Value;
                            var key = coinId;
                            if (coinName.ToUpper().Contains("TESTNET"))
                            {
                                key += ".TESTNET";
                            }

                            var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                            if (alreadyMonitoredConfig == null)
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                var nodeRegion = xmlDoc.CreateElement("add");
                                nodeRegion.SetAttribute("key", key);
                                nodeRegion.SetAttribute("coinId", coinId);
                                nodeRegion.SetAttribute("coinName", coinName);
                                nodeRegion.SetAttribute("address", String.Empty);
                                xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                                xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            }
                        }

                    }
                }

                //trezor - nem types
                var nemDirectoryInfo = new DirectoryInfo(Path.Combine(trezorFirmwareWorkingDirectory, "common", "defs", "nem"));
                foreach (var nemConfigFile in nemDirectoryInfo.GetFiles("*.json"))
                {
                    using (var reader = nemConfigFile.OpenText())
                    {
                        var configContents = reader.ReadToEnd();
                        dynamic configData = JsonConvert.DeserializeObject(configContents);
                        foreach (var configItem in configData)
                        {
                            var coinName = configItem.name.Value;
                            var coinId = configItem.ticker.Value;
                            var key = coinId;
                            if (coinName.ToUpper().Contains("TESTNET"))
                            {
                                key += ".TESTNET";
                            }

                            var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                            if (alreadyMonitoredConfig == null)
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                var nodeRegion = xmlDoc.CreateElement("add");
                                nodeRegion.SetAttribute("key", key);
                                nodeRegion.SetAttribute("coinId", coinId);
                                nodeRegion.SetAttribute("coinName", coinName);
                                nodeRegion.SetAttribute("address", String.Empty);
                                xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                                xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            }
                        }

                    }
                }

                //trezor - eth chains
                var ethChainsDirectoryInfo = new DirectoryInfo(Path.Combine(ethChainsWorkingDirectory, "_data", "chains"));
                var ethTokensDirectoryInfo = new DirectoryInfo(Path.Combine(ethTokensWorkingDirectory, "tokens"));
                foreach (var ethChainConfigFile in ethChainsDirectoryInfo.GetFiles("*.json"))
                {
                    using (var reader = ethChainConfigFile.OpenText())
                    {
                        var configContents = reader.ReadToEnd();
                        dynamic configData = JsonConvert.DeserializeObject(configContents);
                        var coinName = configData.name.Value;
                        var coinId = configData.nativeCurrency.symbol.Value;
                        var shortName = configData?.shortName?.Value;
                        var key = coinId;
                        if (coinName.ToUpper().Contains("TESTNET"))
                        {
                            key += ".TESTNET";
                        }

                        if (!String.IsNullOrEmpty(shortName))
                        {
                            key += "." + shortName;
                        }
                        

                        var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId && x.CoinName == coinName).FirstOrDefault();
                        if (alreadyMonitoredConfig == null)
                        {
                            var xmlDoc = new XmlDocument();
                            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            var nodeRegion = xmlDoc.CreateElement("add");
                            nodeRegion.SetAttribute("key", key);
                            nodeRegion.SetAttribute("coinId", coinId);
                            nodeRegion.SetAttribute("coinName", coinName);
                            nodeRegion.SetAttribute("address", String.Empty);
                            xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                        }

                        foreach (var tokenChain in ethTokensDirectoryInfo.GetDirectories())
                        {
                            if (tokenChain.Name.Equals(coinId, StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var tokenConfigFile in tokenChain.GetFiles("*.json"))
                                {
                                    using (var tokenReader = tokenConfigFile.OpenText())
                                    {
                                        var tokenContents = tokenReader.ReadToEnd();
                                        dynamic tokenData = JsonConvert.DeserializeObject(tokenContents);
                                        var tokenName = tokenData.name.Value;
                                        var tokenId = tokenData.symbol.Value;
                                        var tokenType = tokenData?.type?.Value;
                                        var tokenContract = tokenData.address.Value;

                                        var alreadyMonitoredTokenConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == (coinId + "." + tokenId + "." + tokenContract)).FirstOrDefault();
                                        if (alreadyMonitoredTokenConfig==null)
                                        {
                                            var xmlDoc = new XmlDocument();
                                            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                            var nodeRegion = xmlDoc.CreateElement("add");
                                            nodeRegion.SetAttribute("key", coinId + "." + tokenId + "." + tokenContract);
                                            nodeRegion.SetAttribute("coinId", tokenId);
                                            nodeRegion.SetAttribute("coinName", tokenName);
                                            nodeRegion.SetAttribute("address", String.Empty);
                                            nodeRegion.SetAttribute("type", tokenType);
                                            nodeRegion.SetAttribute("contractaddress", tokenContract);
                                            nodeRegion.SetAttribute("parentcoin", coinId);
                                            xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                                            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ConfigurationManager.RefreshSection("Coins");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting trezor firmware data from github: " + ex.Message);
            }
        }

        private static void GetFiatData()
        {
            if (!String.IsNullOrEmpty(coinMarketCapApiKey))
            {
                var configuredCoinCodes = String.Join(",", configuredCoinList.Keys);
                var client = new RestClient("https://pro-api.coinmarketcap.com/v1");
                var request = new RestRequest(String.Format("cryptocurrency/quotes/latest?symbol={0}", configuredCoinCodes));
                request.AddHeader("X-CMC_PRO_API_KEY", coinMarketCapApiKey);
                var result = client.Execute(request);
                dynamic apiResponse = JsonConvert.DeserializeObject(result.Content);
                foreach (var apiItem in apiResponse.data)
                {
                    foreach (var nestedItem in apiItem)
                    {
                        foreach (var quoteItem in nestedItem.quote)
                        {
                            if (quoteItem.Name == "USD")
                            {
                                foreach (var superNested in quoteItem)
                                {
                                    var symbol = nestedItem.symbol.Value;
                                    var usdPrice = Convert.ToDouble(superNested.price.Value);
                                    if (configuredCoinList.ContainsKey(symbol))
                                    {
                                        configuredCoinList[symbol] = usdPrice;
                                    }
                                }
                            }
                        }
                    }
                }
                exchangeRateDateTime = DateTime.Now;
            }
        }

        private static void SetupConfiguredCoinList()
        {
            var monitoredCoinSection = ConfigurationManager.GetSection("Coins") as MonitoredCoinSection;
            foreach (var coin in monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => !String.IsNullOrEmpty(x.Address)))
            {
                configuredCoinList.Add(coin.CoinID, 0.00);
            }
        }

        private static double GetBalanceFromRpc(MonitoredCoinElement coin)
        {
            var apiUrl = coin.RpcEndpoint;
            if (String.IsNullOrEmpty(apiUrl))
            {
                Console.WriteLine(String.Format("No API URL defined for coin: {0}", coin.CoinID));
                return 0.00;
            }
            double result = 0.00;

            switch (coin.AlternateRpcMethod)
            {
                //Default is to use address groupings and import wallet if it doesn't exist
                default:
                    result = GetBalanceFromAddressGroupings(coin);
                    break;
            }

            return result;
        }

        private static double GetBalanceFromAddressGroupings(MonitoredCoinElement coin, bool reExecuted = false)
        {
            double result = 0.00;
            RestClient client = new RestClient(coin.RpcEndpoint);
            RestRequest request = new RestRequest("", Method.POST);
            request.Credentials = new NetworkCredential(coin.RpcUser, coin.RpcPassword);
            request.AddHeader("Content-Type", "application/json-rpc");
            JObject requestBody = new JObject();
            requestBody.Add(new JProperty("jsonrpc", "1.0"));
            requestBody.Add(new JProperty("id", Guid.NewGuid()));
            requestBody.Add(new JProperty("method", "listaddressgroupings"));

            var jsonData = JsonConvert.SerializeObject(requestBody);
            request.AddJsonBody(jsonData);
            var response = client.Execute(request);
            dynamic responseContent = JsonConvert.DeserializeObject(response.Content);
            if (responseContent.error != null)
            {
                if (responseContent?.error?.message?.Value == "Loading block index..." ||
                    responseContent?.error?.message?.Value == "Rewinding blocks..." ||
                    responseContent?.error?.message?.Value == "Rescanning..."
                    )
                {
                    return 0.00;
                }
            }

            bool foundAddress = false;
            if (responseContent?.result != null)
            {
                foreach (var grouping in responseContent.result)
                {
                    foreach (var addressData in grouping)
                    {
                        string address = addressData[0].Value;
                        if (address.Equals(coin.Address))
                        {
                            return addressData[1].Value;
                        }
                    }
                }
            }
            if (!foundAddress && !reExecuted)
            {
                ImportAddressIntoWallet(coin);
                result = GetBalanceFromAddressGroupings(coin, true);
            }
            return result;
        }

        private static void ImportAddressIntoWallet(MonitoredCoinElement coin)
        {
            RestClient client = new RestClient(coin.RpcEndpoint);
            RestRequest request = new RestRequest("", Method.POST);
            request.Credentials = new NetworkCredential(coin.RpcUser, coin.RpcPassword);
            request.AddHeader("Content-Type", "application/json-rpc");
            JObject requestBody = new JObject();
            requestBody.Add(new JProperty("jsonrpc", "1.0"));
            requestBody.Add(new JProperty("id", Guid.NewGuid()));
            requestBody.Add(new JProperty("method", "importaddress"));
            JArray requestParams = new JArray();
            requestParams.Add(coin.Address);
            requestBody.Add(new JProperty("params", requestParams));
            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            request.AddJsonBody(requestBodyJson);
            var response = client.Execute(request);
            dynamic responseContent = JsonConvert.DeserializeObject(response.Content);
        }

        private static void RefreshBalances()
        {
            Dictionary<string, string> coinBalances = new Dictionary<string, string>();
            var monitoredCoinSection = ConfigurationManager.GetSection("Coins") as MonitoredCoinSection;
            globalFiatAmount = 0.00;
            foreach (var coin in monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => !String.IsNullOrEmpty(x.Address)))
            {
                try
                {
                    double balance = GetBalanceFromRpc(coin);
                    double usdExchangeRate = configuredCoinList[coin.CoinID];
                    var fiatAmount = balance * usdExchangeRate;
                    globalFiatAmount += fiatAmount;
                    if (balance > 1)
                    {
                        balance = Math.Round(balance, 2);
                    }
                    else
                    {
                        balance = Math.Round(balance, 5);
                    }
                    coinBalances.Add(coin.CoinID, String.Format("{0} (${1})", balance, Math.Round(fiatAmount,2)));
                }
                catch(Exception ex)
                {
                    coinBalances.Add(coin.CoinID, ex.Message);
                }
            }
            Console.SetCursorPosition(0, 0);
            var whiteSpace = new StringBuilder().Append(' ', 60).ToString();
            Console.WriteLine($"Last Updated: {DateTime.Now}{whiteSpace}");
            Console.WriteLine($"Exchange Rate Last Updated: {exchangeRateDateTime}{whiteSpace}");
            Console.WriteLine(String.Format("Global Fiat: ${0}{1}", Math.Round(globalFiatAmount, 2), whiteSpace));
            foreach (var item in coinBalances.OrderBy(x => x.Key))
            {
                Console.WriteLine(String.Format("{0}: {1}{2}", item.Key, item.Value, whiteSpace));
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void ForceDeleteDirectory(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }
    }
}
