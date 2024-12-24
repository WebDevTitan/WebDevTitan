using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Project.Interfaces;

namespace Project.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            Loaded += (s, e) =>
            {
                if (DataContext is ICloseable)
                {
                    (DataContext as ICloseable).RequestClose += (sender, args) => Close();
                    (DataContext as ICloseable).RequestMinimize += (sender, args) => { this.WindowState = WindowState.Minimized; };
                    (DataContext as ICloseable).RequestRestore += (sender, args) => { if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized; else this.WindowState = WindowState.Normal; };
                }
            };

            ExtraPanel.Visibility = Visibility.Collapsed;
            HorsePanel.Visibility = Visibility.Collapsed;

#if (!ARB_LIMIT)
            ArbToLabel.Visibility = Visibility.Collapsed;
            ArbToText.Visibility = Visibility.Collapsed;
            ArbToLabel1.Visibility = Visibility.Collapsed;
            ArbToText1.Visibility = Visibility.Collapsed;
#endif

            Tipster2Panel.Visibility = Visibility.Collapsed;
            SoccerLivePanel.Visibility = Visibility.Collapsed;

            EnableMajorLeaguesOnlyPanel.Visibility = Visibility.Collapsed;
            EnableDailyBetCountLimitPanel.Visibility = Visibility.Collapsed;
            PlaceDoubleValuesPanel.Visibility = Visibility.Collapsed;
            SoccerEnableFirstHalfPanel.Visibility = Visibility.Collapsed;
            SoccerEnableSecondHalfPanel.Visibility = Visibility.Collapsed;
            PlaceFastModePanel.Visibility = Visibility.Collapsed;
            EnableMaxPendingBetsPanel.Visibility = Visibility.Collapsed;


#if (BET365_VM || BET365_BM || BET365_PL || BET365_CHROMEDEV || BET365_PUPPETEER || BET365_ADDON)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_bet365.png", UriKind.Relative));
            Global.Bookmaker = "bet365";
#if (HORSE)
            HorsePanel.Visibility = Visibility.Visible;
#else
            ExtraPanel.Visibility = Visibility.Visible;
#endif
            
            ValuebetPanel.Visibility = Visibility.Visible;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            //IntervalDelayPanel.Visibility = Visibility.Visible;
            EnableMajorLeaguesOnlyPanel.Visibility = Visibility.Visible;
            EnableDailyBetCountLimitPanel.Visibility = Visibility.Visible;
            PlaceDoubleValuesPanel.Visibility = Visibility.Visible;
            PlaceFastModePanel.Visibility = Visibility.Visible;
#if (VIP)

             
            Tipster2Panel.Visibility = Visibility.Visible;
            SoccerLivePanel.Visibility = Visibility.Visible;
#endif
#elif (PADDYPOWER)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_paddypower.png", UriKind.Relative));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (SISAL)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_sisal.png", UriKind.Relative));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (LEOVEGAS)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_leovegas.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 245, 123, 44));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (ELYSGAME)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_elysgame.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 21, 23, 29));
            DomainPanel.Visibility = Visibility.Collapsed;

#elif (EUROBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_eurobet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 36, 99, 158));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "eurobet";
#elif (BWIN)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_bwin.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            Global.Bookmaker = "bwin";
            //DomainPanel.Visibility = Visibility.Collapsed;
#elif (SPORTINGBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/sportingbet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 26, 138, 207));
            Global.Bookmaker = "sportingbet";
#elif (SKYBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_skybet.png", UriKind.Relative));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (TIPSPORT)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_tipsport.png", UriKind.Relative));
#elif (LOTTOMATICA)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_lottomatica.png", UriKind.Relative));
            DomainPanel.Visibility = Visibility.Collapsed;
            PlaceDoubleValuesPanel.Visibility = Visibility.Visible;
            Global.Bookmaker = "lottomatica";
#elif (BETMGM)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betmgm.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
#elif (BETWAY || BETWAY_ADDON)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betway.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ExtraPanel.Visibility = Visibility.Visible;
            SoccerLivePanel.Visibility = Visibility.Visible;
#elif (NOVIBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_novibet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 48, 53, 64));
            DomainPanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            IntervalDelayPanel.Visibility = Visibility.Visible;
            SoccerEnableFirstHalfPanel.Visibility = Visibility.Visible;
            SoccerEnableSecondHalfPanel.Visibility = Visibility.Visible;
            EnableMaxPendingBetsPanel.Visibility = Visibility.Visible;
#elif (PINNACLE)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_pinnacle.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
            ExtraPanel.Visibility = Visibility.Visible;
            SoccerLivePanel.Visibility = Visibility.Visible;
            Global.Bookmaker = "pinnacle";
#elif (STOIXIMAN || STOIXIMAN_CDP)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_stoiximan.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 102, 204));

            RequestDelayPanel.Visibility = Visibility.Visible;

            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            IntervalDelayPanel.Visibility = Visibility.Visible;
            SoccerEnableFirstHalfPanel.Visibility = Visibility.Visible;
            SoccerEnableSecondHalfPanel.Visibility = Visibility.Visible;
            EnableMaxPendingBetsPanel.Visibility = Visibility.Visible;

            Global.Bookmaker = "stoiximan";
#elif (SUPERBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_superbet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 197, 1, 2));

            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            IntervalDelayPanel.Visibility = Visibility.Visible;
            SoccerEnableFirstHalfPanel.Visibility = Visibility.Collapsed;
            SoccerEnableSecondHalfPanel.Visibility = Visibility.Collapsed;
            EnableMaxPendingBetsPanel.Visibility = Visibility.Visible;


            Global.Bookmaker = "superbet";            
#elif (BETANO || BETANO_CDP)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betano.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 102, 0));

            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            IntervalDelayPanel.Visibility = Visibility.Visible;
            SoccerEnableFirstHalfPanel.Visibility = Visibility.Visible;
            SoccerEnableSecondHalfPanel.Visibility = Visibility.Visible;
            EnableMaxPendingBetsPanel.Visibility = Visibility.Visible;

            Global.Bookmaker = "betano";            
#elif (BETFAIR_FETCH || BETFAIR || BETFAIR_PL || BETFAIR_NEW)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betfair.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 184, 12));
            Global.Bookmaker = "betfair";

#elif (FANDUEL)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_fanduel.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 26, 130, 195));
            ExtraPanel.Visibility = Visibility.Collapsed;
            SoccerLivePanel.Visibility = Visibility.Visible;
#elif (PLANETWIN)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_planetwin.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (SNAI)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_snai.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 45, 56, 68));
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (BETPREMIUM)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betpremium.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 118, 3));
            DomainPanel.Visibility = Visibility.Collapsed;            
#elif (SPORTPESA)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_sportpesa.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 2, 11, 19));
            DomainPanel.Visibility = Visibility.Collapsed;            
#elif (BETALAND)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betaland.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 46, 139, 75));
            //DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "betaland";
#elif (DOMUSBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_domusbet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            //DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "domusbet";
#elif (CHANCEBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_chancebet.png", UriKind.Relative));            
            DomainPanel.Visibility = Visibility.Collapsed;
#elif (REPLATZ)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_replatz.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "sportpesa";
#elif (GOLDBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_goldbet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 10, 27, 78));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "goldbet";
#elif (TONYBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_tonybet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 10, 27, 78));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "tonybet";
#elif (UNIBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_unibet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 20, 123, 69));
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;

           
#elif (_888SPORT)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_888sport.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 252, 98, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
#elif (BETPLAY)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_betplay.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 35, 97));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "betplay";
#elif (RUSHBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_rushbet.png", UriKind.Relative));
            
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "rushbet";
#elif (WINAMAX)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_winamax.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 31, 31, 31));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            BIRTH_Text.Visibility = Visibility.Visible;
            BIR_Label.Visibility = Visibility.Visible;
            Global.Bookmaker = "winamax";
#elif (ESTRELABET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_estrelabet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 28, 60, 94));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
#elif (PLAYPIX)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_playpix.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 48, 141, 240));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "playpix";
#elif (FORTUNA)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/fortuna.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 219, 1));
            DomainPanel.Visibility = Visibility.Visible;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "fortuna";
#elif (KTO)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_kto.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "KTO";
#elif (WPLAY)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo_wplay.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 27, 39, 65));
            DomainPanel.Visibility = Visibility.Collapsed;
            RangeStakePanel.Visibility = Visibility.Visible;
            //FixedStakePanel.Visibility = Visibility.Collapsed;
            RequestDelayPanel.Visibility = Visibility.Visible;
            WaitTimeLabelGrid.Visibility = Visibility.Visible;
            Global.Bookmaker = "WPLAY";
#elif (SEUBET)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo-seubet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "SEUBET";

#elif (BET365_QRAPI)
            logoImage.Source = new BitmapImage(new Uri(@"../Images/logo-seubet.png", UriKind.Relative));
            TitlePanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            DomainPanel.Visibility = Visibility.Collapsed;
            Global.Bookmaker = "BET365_QRAPI";
#endif
#if (CHRISTIAN)
            //logoImage1.Source = new BitmapImage(new Uri(@"../Images/christian_logo.jpg", UriKind.Relative));
            chkMode1.Content = "Live";
            chkMode2.Content = "Prematch";
            chkMode3.Visibility = Visibility.Collapsed;

#endif

#if (PRE)
            chkMode1.Visibility = Visibility.Collapsed;
#endif

            string Log = $"App Started Verision: {Global.Bookmaker}({Global.Version})";
#if (BET365_ADDON)
            logoImage1.Source = new BitmapImage(new Uri(@"../Images/christian_logo.jpg", UriKind.Relative));
#if (CHROME)
            Log += " Chrome";
#elif (EDGE)
            Log += " Edge";
#elif (FIREFOX)
            Log += " Firefox";
#endif
#endif

            LogMng.Instance.onWriteStatus(Log);

            VersionLabel.Text = string.Format("ValueBot v{0}", Global.Version);

#if (VIP)
            VersionLabel.Text += "(VIP)";
#endif

#if OXYLABS

            ProxyLabelGrid.Visibility = Visibility.Visible;
            ProxyPanelGrid.Visibility = Visibility.Visible;
            //try
            //{
            //    Global.ProxySessionID = File.ReadAllText("proxySession");
            //}
            //catch
            //{
                Global.ProxySessionID = new Random().Next().ToString();
            //}
#else
            ProxyLabelGrid.Visibility = Visibility.Collapsed;
            ProxyPanelGrid.Visibility = Visibility.Collapsed;
#endif
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    this.DragMove();
            }
            catch { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
#if OXYLABS
            File.WriteAllText("proxySession", Global.ProxySessionID);
#endif
        }

        private void SoccerCheckBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SoccerTooltip.IsOpen = true;

        }

        private void SoccerTooltip_MouseLeave(object sender, MouseEventArgs e)
        {
            SoccerTooltip.IsOpen = false;
        }

        private void chkChrome_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkChrome_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
