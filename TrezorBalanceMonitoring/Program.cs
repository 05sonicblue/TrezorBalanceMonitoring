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
            var trezorFirmwareWorkingDirectory = "trezorFirmware";
            try
            {
                LibGit2Sharp.Repository.Clone(trezorFirmwareRepo, trezorFirmwareWorkingDirectory);

                if (!Directory.Exists(trezorFirmwareWorkingDirectory))
                {
                    Console.WriteLine("Error getting trezor firmware data from github.");
                }
                else
                {
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
                            if (alreadyMonitoredConfig==null)
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

                            Console.WriteLine(coinName);
                        }
                        //Console.WriteLine(bitcoinConfigFile.FullName);
                    }






                    ConfigurationManager.RefreshSection("Coins");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error getting trezor firmware data from github: " + ex.Message);
            }

            if (Directory.Exists(trezorFirmwareWorkingDirectory))
            {
                ForceDeleteDirectory(trezorFirmwareWorkingDirectory);
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
