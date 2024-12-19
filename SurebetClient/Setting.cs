using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Project
{
    public class Setting
    {
        string confName = "setting.conf";
        private static Setting _instance = null;

        public static Setting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Setting();
                }

                return _instance;
            }
        }

        public void saveSetting()
        {
            string jsonSetting = JsonConvert.SerializeObject(_instance);
            File.WriteAllText(confName, jsonSetting);
        }

        public void loadSetting()
        {

            if (!File.Exists(confName))
                return;

            string jsonSetting = File.ReadAllText(confName);
            _instance = JsonConvert.DeserializeObject<Setting>(jsonSetting);

         
            if (_instance.percentage == 0)
                _instance.percentage = 2;
            if (_instance.minOdds == 0)
                _instance.minOdds = 1.2;
            if (_instance.maxOdds == 0)
                _instance.maxOdds = 10;
            if (_instance.stake == 0)
                _instance.stake = 100;

            if (betInterval < 20)
                _instance.betInterval = 20;
#if (UNIBET)
            if (requestDelay < 4)
                _instance.requestDelay = 4;
#endif
        }

        public string username_bet365;
        public string password_bet365;
        public string domain_bet365;

        public string username_luckia;
        public string password_luckia;
        

        public double percentage;        //arbitrage percentage
        public double minOdds;
        public double maxOdds;
        public double stake;

        public int betInterval;

        public int requestDelay;
        
        public string ServerIP;
        public ushort ServerPort;
        public string license;

        public int StartHour;
        public int EndHour;

        public string ProxyRegion;

        public bool bSoccer;
        public bool bBasketBall;
        public bool bVolleyBall;
        public bool bBaseBall;
        public bool bTennis;
        public bool bTableTenis;
        public bool bHockey;
        public bool bRugby;
        public bool bESoccer;
        public bool bHorseRacing;

    }
}
