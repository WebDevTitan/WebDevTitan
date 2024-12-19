using BetburgerServer.Constant;
using Newtonsoft.Json;
using Protocol;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TL;

namespace BetburgerServer.Controller
{
    class TelegramScraper
    {
        private onWriteStatusEvent _onWriteStatus;

        //Bet365EV Channel (Liew)
        private string ApiID = "26297013";
        private string ApiHash = "284490e5624ccbb7255b0f7afda8044b";
        private string Phone = "6583376235";

        //Eurobet Channel (Nicola)
        //private string ApiID = "26112416";
        //private string ApiHash = "ab7c2958610f2d28cecdf65299bd588b";
        //private string Phone = "393662765771";
        public TelegramScraper(onWriteStatusEvent onWriteStatus)
        {
            _onWriteStatus = onWriteStatus;

            WTelegram.Helpers.Log = (l, s) => Debug.WriteLine(s);

        }

        public TelegramScraper()
        {
            WTelegram.Helpers.Log = (l, s) => Debug.WriteLine(s);
        }

        static readonly Dictionary<long, User> Users = new Dictionary<long, User>();
        static readonly Dictionary<long, ChatBase> Chats = new Dictionary<long, ChatBase>();

        private async Task Client_OnUpdate(UpdatesBase updates)
        {            
            updates.CollectUsersChats(Users, Chats);
            if (updates is UpdateShortMessage usm && !Users.ContainsKey(usm.user_id))
                (await Global._client.Updates_GetDifference(usm.pts - usm.pts_count, usm.date, 0)).CollectUsersChats(Users, Chats);
            else if (updates is UpdateShortChatMessage uscm && (!Users.ContainsKey(uscm.from_id) || !Chats.ContainsKey(uscm.chat_id)))
                (await Global._client.Updates_GetDifference(uscm.pts - uscm.pts_count, uscm.date, 0)).CollectUsersChats(Users, Chats);
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm: await HandleMessage(unm.message); break;
                    case UpdateEditMessage uem: await HandleMessage(uem.message, true); break;
                    // Note: UpdateNewChannelMessage and UpdateEditChannelMessage are also handled by above cases
                    //case UpdateDeleteChannelMessages udcm: Console.WriteLine($"{udcm.messages.Length} message(s) deleted in {Chat(udcm.channel_id)}"); break;
                    case UpdateDeleteMessages udm: Console.WriteLine($"{udm.messages.Length} message(s) deleted"); break;
                    //case UpdateUserTyping uut: Console.WriteLine($"{TL.User(uut.user_id)} is {uut.action}"); break;
                    //case UpdateChatUserTyping ucut: Console.WriteLine($"{Peer(ucut.from_id)} is {ucut.action} in {Chat(ucut.chat_id)}"); break;
                    //case UpdateChannelUserTyping ucut2: Console.WriteLine($"{Peer(ucut2.from_id)} is {ucut2.action} in {Chat(ucut2.channel_id)}"); break;
                    //case UpdateChatParticipants { participants: ChatParticipants cp }: Console.WriteLine($"{cp.participants.Length} participants in {Chat(cp.chat_id)}"); break;
                    //case UpdateUserStatus uus: Console.WriteLine($"{User(uus.user_id)} is now {uus.status.GetType().Name[10..]}"); break;
                    //case UpdateUserName uun: Console.WriteLine($"{User(uun.user_id)} has changed profile name: {uun.first_name} {uun.last_name}"); break;
                    //case UpdateUser uu: Console.WriteLine($"{User(uu.user_id)} has changed infos/photo"); break;
                    default: Console.WriteLine(update.GetType().Name); break; // there are much more update types than the above example cases
                }
        }

        List<int> alreadyProcessedMessage = new List<int>();
        private Task HandleMessage(MessageBase messageBase, bool edit = false)
        {
            if (edit) Console.Write("(Edit): ");
            //switch (messageBase)
            //{
            //    //case TL.Message m: Console.WriteLine($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}"); break;
            //    //case MessageService ms: Console.WriteLine($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]"); break;
            //}

            if (messageBase is TL.Message msg)
            {
                //var from = messages.UserOrChat(messageBase.From ?? messageBase.Peer); // from can be User/Chat/Channel
                //if (msgBase is TL.Message msg)
                //    listBox.Items.Add($"{from}> {msg.message} {msg.media}");
                //else if (msgBase is MessageService ms)
                //    listBox.Items.Add($"{from} [{ms.action.GetType().Name[13..]}]");
                
                if (msg.From != null && (msg.From.ID == 644451800 || msg.From.ID == 6171068723))
                {
                    if (edit)
                    {
                        if(_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg Edit Message: {msg.message}");
                    }
                    else
                    {
                        if (_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg New Message: {msg.message}");
                    }

                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);

                        ProcessPickEurobetPick(msg.message);

                    }
                }
                //Ev ++
                else if (msg.From != null && msg.From.ID == 2123072822)
                {
                    if (edit)
                    {
                        if (_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg Edit Message: {msg.message}");
                    }
                    else
                    {
                        if (_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg New Message: {msg.message}");
                    }

                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);
                        ProcessPickEvPlusPick(msg.message);
                    }
                }
                // Winamax
                else if (msg.From != null && (msg.From.ID == 1528568808 || msg.From.ID == 2142350078))
                {
                    if (edit)
                    {
                        if (_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg Edit Message: {msg.message}");
                    }
                    else
                    {
                        if (_onWriteStatus != null)
                            _onWriteStatus($"Tg Edit Message: {msg.message}");
                        else
                            Debug.WriteLine($"Tg New Message: {msg.message}");
                    }

                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);
                        ProcessWinamaxPick();
                    }
                }
                //Ev ++ 
                else if(msg.peer_id != null && msg.peer_id.ID == 2123072822)
                {
                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);
                        ProcessPickEvPlusPick(msg.message);
                    }
                }
                //Winamax
                else if (msg.peer_id != null && (msg.peer_id.ID == 1528568808 || msg.peer_id.ID == 2142350078))
                {
                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);
                        ProcessWinamaxPick();
                    }
                }

                else if (msg.peer_id != null && (msg.peer_id.ID == 2075901023 || msg.peer_id.ID == 2116370071))
                {
                    if (!alreadyProcessedMessage.Contains(msg.ID))
                    {
                        alreadyProcessedMessage.Add(msg.ID);

                        ProcessPickEvChannelPick(msg.message);

                    }
                }
            }
            return Task.CompletedTask;
        }

        private void ProcessWinamaxPick()
        {
            try
            {
                _onWriteStatus("Winamax New Pick!");
                BetburgerInfo info = new BetburgerInfo();

                info.arbId = Utils.getTick().ToString();
                info.kind = PickKind.Type_12;
                info.bookmaker = "bet365";
                info.percent = 10;
                info.created = DateTime.Now.ToString();
                info.updated = DateTime.Now.ToString();

                string payload = JsonConvert.SerializeObject(info);
                CustomEndpoint.sendNewTips(payload);
            }
            catch { }
        }
        private void ProcessPickEvPlusPick(string message)
        {
            string[] lines = message.Split('\n');
            try
            {
                BetburgerInfo info = new BetburgerInfo();

                info.arbId = Utils.getTick().ToString();
                info.kind = PickKind.Type_12;
                info.bookmaker = "bet365";
                info.percent = 10;
                info.created = DateTime.Now.ToString();
                info.updated = DateTime.Now.ToString();

                foreach (string line in lines)
                {
                    if (line.Contains("Unidades"))
                    {
                       string unit = Utils.Between(line + "xx", "Unidades:", "xx");
                       info.stake = Utils.ParseToDouble(unit);
                    }
                    else if (line.Contains("www.bet365.com/dl/sportsbookredirect"))
                    {
                        string[] sub_arr = line.Split('#');
                        info.eventUrl = sub_arr[1].Trim();
                        info.siteUrl = line;
                        info.direct_link = Utils.Between(line, "bs=", "#").Trim();
                    }
                }

                List<BetburgerInfo> betburgerInfoPair = new List<BetburgerInfo> { info };

                if (_onWriteStatus != null)
                    _onWriteStatus(getLogTitle() + $"Ev++ pick Unit : {info.stake} event Url : {info.eventUrl} Direct Link : {info.direct_link}");
                else
                    Debug.WriteLine(getLogTitle() + $"Ev++ pick Unit : {info.stake} event Url : {info.eventUrl} Direct Link : {info.direct_link}");

                if (betburgerInfoPair.Count > 0)
                    GameServer.GetInstance().processValuesInfo(betburgerInfoPair);
            }
            catch { }
        }
        private void ProcessPickEvChannelPick(string message)
        {
            string[] lines = message.Split('\n');
            try
            {
                BetburgerInfo info = new BetburgerInfo();
                

                info.arbId = Utils.getTick().ToString();
                info.kind = PickKind.Type_12;
                info.bookmaker = "bet365";
                info.opbookmaker = "EV_TG";

                info.percent = 10;

                info.sport = lines[0].Replace("⚽", "").Replace(" ", "").ToLower();
                info.eventTitle = lines[8];
                info.league = lines[9];

                string[] names = info.eventTitle.Split(new string[] { " v " }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length != 2)
                {
                    return;
                }
                info.homeTeam = names[0];
                info.awayTeam = names[1];
                string odd = Utils.Between(lines[4], ": ", "(").Trim();
                info.odds = Utils.ParseToDouble(odd);


                foreach (string line in lines)
                {
                    if (line.Contains("Bet365 Link:"))
                    {
                        info.siteUrl = line.Replace("Bet365 Link:", "").Trim();
                        break;
                    }
                }

                string[] starttimeparams = lines[10].Split(' ');
                if (starttimeparams.Length > 3)
                {
                    string[] month = starttimeparams[1].Split('-');
                    int dayVal = Convert.ToInt32(month[0]);
                    int monthVal = 1;
                    if (month[1].ToLower() == "jan")
                        monthVal = 1;
                    if (month[1].ToLower() == "feb")
                        monthVal = 2;
                    if (month[1].ToLower() == "mar")
                        monthVal = 3;
                    if (month[1].ToLower() == "apr")
                        monthVal = 4;
                    if (month[1].ToLower() == "may")
                        monthVal = 5;
                    if (month[1].ToLower() == "jun")
                        monthVal = 6;
                    if (month[1].ToLower() == "jly")
                        monthVal = 7;
                    if (month[1].ToLower() == "aug")
                        monthVal = 8;
                    if (month[1].ToLower() == "sep")
                        monthVal = 9;
                    if (month[1].ToLower() == "oct")
                        monthVal = 10;
                    if (month[1].ToLower() == "nov")
                        monthVal = 11;
                    if (month[1].ToLower() == "dec")
                        monthVal = 12;

                    string[] times = starttimeparams[2].Split(':');
                    int hour = Convert.ToInt32(times[0]);
                    int min = Convert.ToInt32(times[1]);

                    DateTime startTime = DateTime.SpecifyKind(new DateTime(DateTime.Now.Year, monthVal, dayVal, hour, min, 0), DateTimeKind.Utc).AddHours(-1);
                    info.started = startTime.ToString("MM/dd/yyyy HH:mm:ss");
                }

                string outcome = "";
                if (lines[2].StartsWith("OVER") || lines[2].StartsWith("UNDER"))
                {
                    outcome = lines[2].Replace("OVER", "Over").Replace("UNDER", "Under");
                }
                else
                {
                    if (lines[2].Trim().ToLower() == info.homeTeam.ToLower())
                    {
                        outcome = "1";
                    }
                    else if (lines[2].Trim().ToLower() == info.awayTeam.ToLower())
                    {
                        outcome = "2";
                    }
                    else
                    {
                        string[] outcomesplits = lines[2].Split(' ');
                        string handicap = outcomesplits[outcomesplits.Length - 1].Trim();
                        if (outcomesplits.Length > 1 && (handicap.StartsWith("+") || handicap.StartsWith("-")))
                        {
                            outcome = "AH";
                            Array.Resize(ref outcomesplits, outcomesplits.Length - 1);
                            string teamname = string.Join(" ", outcomesplits).Trim();
                            if (teamname.ToLower() == info.homeTeam.ToLower())
                            {
                                outcome += "1";
                            }
                            else if (teamname.ToLower() == info.awayTeam.ToLower())
                            {
                                outcome += "2";
                            }
                            else
                            {
                                return;
                            }
                            outcome += $"({handicap})";
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                info.outcome = outcome;

                info.created = DateTime.Now.ToString();

                info.updated = DateTime.Now.ToString();


                List<BetburgerInfo> betburgerInfoPair = new List<BetburgerInfo> { info};

                if(_onWriteStatus != null)
                    _onWriteStatus(getLogTitle() + $"BS pick sport: {info.sport} league: {info.league} event: {info.eventTitle} outcome: {info.outcome} odd: {info.odds} upto {info.profit} ");
                else
                    Debug.WriteLine(getLogTitle() + $"BS pick sport: {info.sport} league: {info.league} event: {info.eventTitle} outcome: {info.outcome} odd: {info.odds} upto {info.profit} ");
                
                if (betburgerInfoPair.Count > 0)
                    GameServer.GetInstance().processValuesInfo(betburgerInfoPair);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception6 {ex.StackTrace} {ex.Message}");
            }
        }
        private void ProcessPickEurobetPick(string message)
        {
            string[] lines = message.Split('\n');
            try
            {
                BetburgerInfo info = new BetburgerInfo();
                
                info.arbId = lines[0].Replace("Eurobet|", "").Trim();
                info.kind = PickKind.Type_11;
                info.bookmaker = "telegramgtip";

                info.opbookmaker = "---";

                info.percent = 10;
 

                //if (string.IsNullOrEmpty(info.bookmaker))
                //    continue;

                info.eventTitle = lines[3];


                info.league = lines[2];
                string ItalianSport = Utils.Between(lines[1], "(", ")");
                string[] teams = info.eventTitle.Split(new string[] { " - ", " – " }, StringSplitOptions.RemoveEmptyEntries);
                if (teams.Length != 2)
                {
                    if(_onWriteStatus != null)
                        _onWriteStatus(getLogTitle() + $"Team name parsing error : {info.eventTitle}");
                    else
                        Debug.WriteLine(getLogTitle() + $"Team name parsing error : {info.eventTitle}");

                    return;
                }
                if (ItalianSport == "Calcio")
                {
                    info.sport = "soccer";
                    
                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];                    
                }
                else if (ItalianSport == "Basket" || ItalianSport == "Pallacanestro")
                {
                    info.sport = "basketball";
                    
                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];
                }
                else if (ItalianSport == "Tennis")
                {
                    info.sport = "tennis";
                    
                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];
                }
                else if (ItalianSport == "Hockey")
                {
                    info.sport = "hockey";
                    
                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];
                }
                else if (ItalianSport == "Football americano")
                {
                    info.sport = "football";

                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];
                }
                else if (ItalianSport == "Pallamano")
                {
                    info.sport = "handball";

                    info.homeTeam = teams[0];
                    info.awayTeam = teams[1];
                }
                else
                {
                    return;
                }

                string[] outcomeodd = lines[4].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                info.started = DateTime.Now.ToString();

                info.created = DateTime.Now.ToString();

                info.updated = Utils.Between(lines[1], ")").Trim();

                for (int i = 0; i < outcomeodd.Length - 1; i++)
                    info.outcome += outcomeodd[i] + " ";
                info.outcome = info.outcome.Trim();
                info.odds = Utils.ParseToDouble(outcomeodd[outcomeodd.Length - 1]);

                info.profit = Utils.ParseToDouble(Utils.Between(lines[7], "QUOTA MIN:").Trim());
                info.extra = lines[6].Replace("PUNTATA", "").Replace(":", "").Replace(" ", "");
                
                                
                List<BetburgerInfo> betburgerInfoPair = new List<BetburgerInfo> { info};

                if(_onWriteStatus != null)
                    _onWriteStatus(getLogTitle() + $"BS pick sport: {info.sport} league: {info.league} event: {info.eventTitle} outcome: {info.outcome} odd: {info.odds} upto {info.profit} ");
                else
                    Debug.WriteLine(getLogTitle() + $"BS pick sport: {info.sport} league: {info.league} event: {info.eventTitle} outcome: {info.outcome} odd: {info.odds} upto {info.profit} ");

                if (betburgerInfoPair.Count > 0)
                    GameServer.GetInstance().processValuesInfo(betburgerInfoPair);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception6 {ex.StackTrace} {ex.Message}");
            }
        }
        private async Task DoLogin(string loginInfo)
        {
            string what = await Global._client.Login(loginInfo);
            if (what != null)
            {
                if(_onWriteStatus != null)
                    _onWriteStatus($"A {what} is required...");  
                else
                    Debug.WriteLine($"A {what} is required...");

                return;
            }

            if (_onWriteStatus != null)
                _onWriteStatus($"We are now connected as {Global._client.User}");
            else
                Debug.WriteLine($"We are now connected as {Global._client.User}");

        }

        private void ScrapeProc()
        {
            if(Global._client != null)
                Global._client.OnUpdate += Client_OnUpdate;
            //await DoLogin(Phone);
        }

        private async void ConnectToTelegram()
        {
            Global._client = new WTelegram.Client(int.Parse(ApiID), ApiHash);
            Global._client.OnUpdate += Client_OnUpdate;
            await DoLogin(Phone);
        }
        
        public async Task scrape()
        {
            Thread thr = new Thread(ScrapeProc);
            thr.Start();
        }

        private string getLogTitle()
        {
            return "[TelegramPick]";
        }
                
    }
}
