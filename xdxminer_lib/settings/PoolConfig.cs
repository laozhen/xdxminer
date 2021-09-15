using System.Collections.ObjectModel;

namespace xdxminer_lib.settings
{
    public class PoolConfig
    {
        public int id { get; set; }
        public string url { get; set; }

        public int port { get; set; }
        public bool isSSL { get; set; }
        string rigName { get; set; }
        bool accountMining { get; set; }
        bool anonymousMining { get; set; }
        string username { get; set; }
        string password { get; set; }

        public string desc {  get => url + ":" + port + (isSSL ? " (ssl) " : ""); }


        public static ObservableCollection<PoolConfig> GetETCPoolConfigs()
        {
            return new ObservableCollection<PoolConfig> {
                new PoolConfig() { id = 1, url = "asia1-etc.ethermine.org", port = 5555 , isSSL =true} ,
                new PoolConfig { id = 2, url = "eu1-etc.ethermine.org" ,port = 5555, isSSL =true} ,
                new PoolConfig { id = 3, url = "us1-etc.ethermine.org" ,port = 5555 ,isSSL =true} ,

                new PoolConfig { id = 5, url = "etc-eu1.nanopool.org" ,port = 9433 ,isSSL =true} ,
                new PoolConfig { id = 6, url = "etc-us-east1.nanopool.org" ,port = 9433, isSSL =true} ,
                new PoolConfig { id = 7, url = "etc-asia1.nanopool.org" ,port = 9433, isSSL =true} ,

            };
        }


        public static ObservableCollection<PoolConfig> GetETHPoolConfigs()
        {
            return new ObservableCollection<PoolConfig> {
                new PoolConfig() { id = 1, url = "asia1.ethermine.org", port = 5555 , isSSL =true} , 
                new PoolConfig { id = 2, url = "eu1.ethermine.org" ,port = 5555, isSSL =true} ,
                new PoolConfig { id = 3, url = "us1.ethermine.org" ,port = 5555 ,isSSL =true} ,

                new PoolConfig { id = 8, url = "eth-eu1.nanopool.org" ,port = 9433 ,isSSL =true} ,
                new PoolConfig { id = 9, url = "eth-us-east1.nanopool.org" ,port = 9433, isSSL =true} ,
                new PoolConfig { id = 9, url = "eth-asia1.nanopool.org" ,port = 9433, isSSL =true} ,
         

                new PoolConfig { id = 8, url = "eth-us-east.flexpool.io" ,port = 5555 ,isSSL =true} ,
                new PoolConfig { id = 9, url = "eth-us-west.flexpool.io" ,port = 5555, isSSL =true} ,
                new PoolConfig { id = 9, url = "eth-de.flexpool.io" ,port = 5555, isSSL =true} ,

                new PoolConfig { id = 11, url = "asia.sparkpool.com" ,port = 3333 ,isSSL =false } ,
                new PoolConfig { id = 12, url = "eth-eu.sparkpool.com" ,port = 3333, isSSL =false} ,
                new PoolConfig { id = 13, url = "eth-us.sparkpool.com" ,port = 3333, isSSL =false} ,
            };
        }
    }
}
