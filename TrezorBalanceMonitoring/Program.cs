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

                        var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                        if (alreadyMonitoredConfig == null)
                        {
                            var xmlDoc = new XmlDocument();
                            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            var nodeRegion = xmlDoc.CreateElement("add");
                            nodeRegion.SetAttribute("key", coinId);
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

                            var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                            if (alreadyMonitoredConfig == null)
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                var nodeRegion = xmlDoc.CreateElement("add");
                                nodeRegion.SetAttribute("key", coinId);
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

                            var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId).FirstOrDefault();
                            if (alreadyMonitoredConfig == null)
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                var nodeRegion = xmlDoc.CreateElement("add");
                                nodeRegion.SetAttribute("key", coinId);
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
                foreach (var ethChainConfigFile in ethChainsDirectoryInfo.GetFiles("*.json"))
                {
                    using (var reader = ethChainConfigFile.OpenText())
                    {
                        var configContents = reader.ReadToEnd();
                        dynamic configData = JsonConvert.DeserializeObject(configContents);
                        var coinName = configData.name.Value;
                        var coinId = configData.nativeCurrency.symbol.Value;

                        var alreadyMonitoredConfig = monitoredCoinSection.MonitoredCoins.OfType<MonitoredCoinElement>().Where(x => x.Key == coinId && x.CoinName == coinName).FirstOrDefault();
                        if (alreadyMonitoredConfig == null)
                        {
                            var xmlDoc = new XmlDocument();
                            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                            var nodeRegion = xmlDoc.CreateElement("add");
                            nodeRegion.SetAttribute("key", coinId);
                            nodeRegion.SetAttribute("coinId", coinId);
                            nodeRegion.SetAttribute("coinName", coinName);
                            nodeRegion.SetAttribute("address", String.Empty);
                            xmlDoc.SelectSingleNode("//Coins").AppendChild(nodeRegion);
                            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
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
