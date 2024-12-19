using BetburgerServer.Constant;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BetburgerServer.Controller
{
 
    public class TelegramBotScraper
    {/*
        private onWriteStatusEvent _onWriteStatus;

        public static TelegramBotClient botClient = null;


        List<string> AlreadyProcessedIDList = new List<string>();

        public TelegramScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;
                    
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (message.Text.Split(' ').First())
            {
          
            }
        }

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task SendInlineKeyboard(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }
                });
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard
            );
        }

        static async Task SendReplyKeyboard(Message message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[][]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                },
                resizeKeyboard: true
            );

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup

            );
        }

        static async Task SendDocument(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
            await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputOnlineFile(fileStream, fileName),
                caption: "Nice Picture"
            );
        }

        static async Task RequestContactAndLocation(Message message)
        {
            var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard
            );
        }

        static async Task Usage(Message message)
        {
            const string usage = "Usage:\n" +
                                    "/inline   - send inline keyboard\n" +
                                    "/keyboard - send custom keyboard\n" +
                                    "/photo    - send a photo\n" +
                                    "/request  - request location or contact";
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }
        // Process Inline Keyboard callback data
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}"
            );

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgbotClients",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };
            await botClient.AnswerInlineQueryAsync(
                inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }

        private async void ScrapeProc()
        { 
            botClient = new TelegramBotClient("5845741672:AAH-siy0ODfKyNmL98xh6oMh4bZrAXr1myc");    //@valuebetsenderbot

            var me = await botClient.GetMeAsync();
            botClient.OnUpdate += BotClient_OnUpdate;
            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnMessageEdited += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            botClient.OnInlineQuery += BotOnInlineQueryReceived;
            botClient.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            botClient.OnReceiveError += BotOnReceiveError;

            botClient.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");

            while(GameConstants.bRun)
            {
                Thread.Sleep(500);
            }
            
            botClient.StopReceiving();

            //WTelegram.Client client = new WTelegram.Client(YOUR_API_ID, "YOUR_API_HASH"); // this constructor doesn't need a Config method
            //await DoLogin("+12025550156"); // initial call with user's phone_number

            //async Task DoLogin(string loginInfo) // (add this method to your code)
            //{
            //    while (client.User == null)
            //        switch (await client.Login(loginInfo)) // returns which config is needed to continue login
            //        {
            //            case "verification_code": Console.Write("Code: "); loginInfo = Console.ReadLine(); break;
            //            case "name": loginInfo = "John Doe"; break;    // if sign-up is required (first/last_name)
            //            case "password": loginInfo = "secret!"; break; // if user has enabled 2FA
            //            default: loginInfo = null; break;
            //        }
            //    Console.WriteLine($"We are logged-in as {client.User} (id {client.User.id})");
            //}

        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        private void BotClient_OnUpdate(object sender, UpdateEventArgs e)
        {
            try
            {
                var message = e.Update;
                if (message.Type == UpdateType.ChannelPost)
                {
                    if (message.ChannelPost.SenderChat.Type == ChatType.Channel)
                    {
                        if (message.ChannelPost.SenderChat.Title == "aaa" ||
                            message.ChannelPost.SenderChat.Title == "giannis_bot")
                        {
                            string text = message.ChannelPost.Text;

                            _onWriteStatus(getLogTitle() + $"NewTelegramMsg({message.ChannelPost.SenderChat.Title}): {text}");

                            
                            try
                            {
                                BetburgerInfo info = new BetburgerInfo();
                                
                                info.kind = PickKind.Type_2;
                                info.bookmaker = "Bet365";
                                
                                info.opbookmaker = "Unkown";


                                info.kind = PickKind.Type_10;
                                info.created = DateTime.Now.ToString();
                                info.updated = DateTime.Now.ToString();
                                info.started = DateTime.Now.ToString();

                                info.sport = "ESoccer";
                                info.league = "ESoccer";

                                text = text.Replace("⚽️", "").Replace("🔼", "").Replace("⚽", "").Replace("🕒", "").Replace("🎯", "").Replace("🔥", "").Replace("❗️", "").Replace("🤖", "").Replace("🔻", "").Replace("♻️", "");
                                string[] lines = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                                string outcomelabel = "";
                                string playerslabel = "";
                                string teamslabel = "";
                                foreach (string linet in lines)
                                {
                                    string line = linet.Trim();
                                    if (line.StartsWith("Fulltime "))
                                    {
                                        outcomelabel = line.Replace("Fulltime ", "");
                                    }                                    
                                    else if (line.StartsWith("Current Time: "))
                                    {
                                        info.period = line.Replace("Current Time: ", "");
                                    }
                                    else if (line.StartsWith("Current Score: "))
                                    {
                                        info.arbId = line.Replace("Current Score: ", "");
                                    }
                                    else if (line.StartsWith("https://"))
                                    {
                                        info.eventUrl = line;
                                    }
                                    else if (line.StartsWith("(") && line.EndsWith(")"))
                                    {
                                        teamslabel = line.Substring(1, line.Length - 2);
                                    }
                                    else
                                    {
                                        if (line.Contains(" vs "))
                                        {
                                            playerslabel = line;
                                        }
                                    }
                                }

                                string[] players = playerslabel.Split(new string[] { "vs" }, StringSplitOptions.RemoveEmptyEntries);
                                string[] teams = teamslabel.Split(new string[] { "vs" }, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < players.Count(); i++)
                                    players[i] = RemoveSpecialCharacters(players[i]).Trim();
                                for (int i = 0; i < teams.Count(); i++)
                                    teams[i] = RemoveSpecialCharacters(teams[i]).Trim();

                                info.homeTeam = $"{teams[0]} ({players[0]}) Esports";
                                info.awayTeam = $"{teams[1]} ({players[1]}) Esports";
                                info.eventTitle = $"{info.homeTeam} v {info.awayTeam}";

                                                                
                                string[] outcomeparams = outcomelabel.Split('@');

                                info.odds = Utils.FractionToDouble(outcomeparams[1]);
                                info.siteUrl = outcomeparams[0].Trim();

                                if (info.siteUrl.StartsWith("Asian Hand."))
                                {
                                    info.outcome = "AH";
                                    if (info.siteUrl.Contains(players[0]))
                                        info.outcome += "1";
                                    else if (info.siteUrl.Contains(players[1]))
                                        info.outcome += "2";
                                    else
                                        throw new Exception();
                                    string[] handis = info.siteUrl.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    info.outcome += $"({handis[handis.Length - 1]})";
                                }
                                else
                                {
                                    info.outcome = info.siteUrl.Trim();
                                }

                                

                                info.opbookmaker = "Telegram";
                                

                                List<BetburgerInfo> list = new List<BetburgerInfo>() { info };
                                GameServer.GetInstance().processValuesInfo(list);
                            }
                            catch (Exception ex)
                            {
                                _onWriteStatus(getLogTitle() + $"Exception: {ex}");
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public async Task scrape()
        {
            
            Thread thr = new Thread(ScrapeProc);
            thr.Start();
        }


        private string getLogTitle()
        {
            return "[TelegramScraper]";
        }
        */
    }
    
}
