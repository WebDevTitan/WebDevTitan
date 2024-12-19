using SeastoryServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using BetComparerServer.Controller;

namespace Seubet.Controller
{
    public class TelegramCtrler
    {
        TelegramBotClient botClient = null;
        onWriteStatusEvent m_writeStatus;
        public string _apiToken = string.Empty;
        public List<string> _keys = new List<string>();
        private SeubetScraper m_betCtrl = null;

        public TelegramCtrler(onWriteStatusEvent _onWrietStatus, string apiToken)
        {
            m_writeStatus = _onWrietStatus;
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            _apiToken = apiToken;
            botClient = new TelegramBotClient(_apiToken);
        }
        internal void sendMessage(string message)
        {
            try
            {
                _ = botClient.SendTextMessageAsync(-1002351185541, message).Result;
            }
            catch (Exception ex)

            {
                //m_betCtrl.TeleConnect();
            }
            
        }
    }
}
