using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BetburgerServer.Controller
{
    public class TelegramCtrl
    {
        private static TelegramCtrl _instance = null;

        TelegramBotClient botClient = null;
        public static TelegramCtrl Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TelegramCtrl();
                return _instance;
            }
        }

        public TelegramCtrl()
        {
            ServicePointManager.DefaultConnectionLimit = 2;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            botClient = new TelegramBotClient("7737833570:AAE-4CN9N_q7s42szmc9LFjMCl4lop1ibbg");
            //botClient.SendTextMessageAsync(-1002489828537, "werwerwe");
        }

        private void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            try
            {
                Console.WriteLine(e.Message.Text);
                if (e.Message.Chat.Title == "365 sure")
                {
                }
            }
            catch { }
        }
        public void sendMessage(string message)
        {            
           _=botClient.SendTextMessageAsync(-2351185541, message).Result;              
           
        }

    }

}
