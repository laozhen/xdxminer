using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using xdxminer_lib;
using xdxminer_lib.util;
using xdxminer_lib.settings;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace xdxminer_app
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Logger logger = new Logger("ui");
        ObservableCollection<Coin> coins = Coin.getCoinConfig();
        Coin selectedCoin;
        PoolConfig selectedPoolConfig;
        Manager manager;

        public MainPage()
        {
            selectedCoin = coins.FirstOrDefault();
            this.InitializeComponent();
        }

        [Obsolete]
        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
       
            username.PlaceholderText = selectedCoin.defaultWalletAddress;
            Timer timer = new Timer();
            timer.Interval = 1500;

            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.Start();

        }

        [Obsolete]
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                output.ScrollToVerticalOffset(output.ExtentHeight);
                string log;
                acceptedShareNum.Text = Stratum.acceptShare.ToString();
                foundShareNum.Text = Stratum.foundShare.ToString();
                while (Logger.logs.TryDequeue(out log)){
                    outputText.Text += log+ Environment.NewLine;
                }

                output.ScrollToVerticalOffset(output.ExtentHeight);

            });
        }


        private void inputText_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {

        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            username.IsEnabled = false;
            servers.IsEnabled = false;
            crytoType.IsEnabled = false;
            workerName.IsEnabled = false;
            start.Visibility = Visibility.Collapsed;
            stop.Visibility = Visibility.Visible;
            stop.IsEnabled = false;
            string addr = username.Text.Trim().ToUpper();
            if(addr.Length ==0)
            {
                logger.error("Please set your wallet address.");
                addr = selectedCoin.defaultWalletAddress;
            }
            logger.info("starting..." );
            logger.info("using wallet address " + addr);
            manager = new Manager();
            manager.clientStratum.setDetails(selectedPoolConfig.url, selectedPoolConfig.port, addr.ToLower(), selectedPoolConfig.isSSL);
            manager.start();
            delayEnable(stop);
            
        }
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                username.IsEnabled = true;
                servers.IsEnabled = true;
                workerName.IsEnabled = true;
                crytoType.IsEnabled = true;
                start.Visibility = Visibility.Visible;
                start.IsEnabled = false;
                stop.Visibility = Visibility.Collapsed;
                logger.info("stopping ...");
                manager.stop();
                manager = null;
                logger.info("stopped.");
                delayEnable(start);

            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>")]
        private async void delayEnable (Button button)
        {
            await Task.Delay(3000);
            button.IsEnabled = true;
        }

        private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(servers.SelectedItem is PoolConfig)
            {
                username.IsEnabled = true;
                selectedPoolConfig = (PoolConfig)servers.SelectedItem;
                if (username.Text.Trim().Length == 0)
                {
                    username.PlaceholderText = selectedCoin.defaultWalletAddress;
                }
            }
            else
            {
                username.IsEnabled = false;
                username.PlaceholderText = "";
            }
        }

        private void crytoType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Coin.selectedCoin = selectedCoin;
            servers.ItemsSource = selectedCoin.poolConfig;
            if (username.Text.Trim().Length == 0)
            {
                username.PlaceholderText = selectedCoin.defaultWalletAddress;
            }
            selectedPoolConfig = selectedCoin.poolConfig.FirstOrDefault();
            servers.SelectedIndex = 0;

        }

    }
}
