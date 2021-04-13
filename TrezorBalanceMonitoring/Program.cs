using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TrezorBalanceMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateConfig();
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

                //trezor - eth tokens
                


                ConfigurationManager.RefreshSection("Coins");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting trezor firmware data from github: " + ex.Message);
            }
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
