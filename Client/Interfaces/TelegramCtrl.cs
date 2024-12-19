using System;
using System.Net;
using Telegram.Bot;

namespace Project.Interfaces
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

            botClient = new TelegramBotClient("7907556016:AAErX92E9tBCVSEsWgh-46tazKBHr2QbU9s");
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
            try
            {
                botClient.SendTextMessageAsync(-1002268973620, message);
                //botClient.SendTextMessageAsync(-1002176331353, message);
            }
            catch { }
        }

    }

}
