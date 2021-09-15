using System;
using System.Collections.ObjectModel;
using Windows.System.Profile;

namespace xdxminer_lib.settings
{
    public class Coin
    {
        public int id {get; set;}
        public string name { get; set;}

        public string defaultWalletAddress { get; set; }
        public CoinType coinType { get; set;}
        public ObservableCollection<PoolConfig>  poolConfig { get; set; }

        public static ObservableCollection<Coin> getCoinConfig()
        {
            var coins = new ObservableCollection<Coin>();

            OperatingSystem os_info = Environment.OSVersion;
            coins.Add(new Coin
            {
                id = 1,
                coinType = CoinType.ETC,
                name = "Ethereum Classic",
                poolConfig = PoolConfig.GetETCPoolConfigs(),
                defaultWalletAddress = "0x78ca71f410caee1d854f1745e62378f1df829a7a"
            });
            string device = AnalyticsInfo.VersionInfo.DeviceFamily;
            if (!device.ToLower().Contains("xbox"))
            {
                coins.Add(new Coin { id = 2, coinType = CoinType.ETH, name = "Ethereum", 
                    poolConfig = PoolConfig.GetETHPoolConfigs(),
                    defaultWalletAddress= "0xaaf6355972aa43689be989f21fc76e012bedbb0e"
                });
            }

            return coins;
        }

        public static Coin selectedCoin { get; set; }
    }
    public enum CoinType { ETH, ETC}
}
