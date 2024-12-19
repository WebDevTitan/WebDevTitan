using BetburgerServer.Constant;
using SeastoryServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BetburgerServer
{
    //Only possible Activation:1 and expired date is larger than current time
#if (TGACCOUNTMANAGE)
    public class TelegramUser
    {
        public long UserId;
        public string UserName;

        public TelegramUser(long id, string name)
        {
            UserId = id;
            UserName = name;
        }
    }
    public enum REQUESTSTEP
    {
        Init,
        Browse,
        Add_SelPackage,
        Add,
        Add_1,
        Add_2,
        Add_3,
        Extend,
        Extend_1,
        Extend_2,
        Extend_3,
        ChangePackage,
        Change_SelPackage,
        Remove
    }
    public class TGAccountManager
    {        
        private bool botIsReceiving = false;
        private List<TelegramUser> MemberList = new List<TelegramUser>();
        Dictionary<long, REQUESTSTEP> ReqStep = new Dictionary<long, REQUESTSTEP>();

        private UInt32 add_package_id = 0;
        private string change_pachage_license = "";
        Thread thrRunning = null;
        public TGAccountManager()
        {
            MemberList.Add(new TelegramUser(644451800, "developer"));
            //MemberList.Add(new TelegramUser(664305168, "simon")); 
        }
        public void Start()
        {
            botIsReceiving = true;

            thrRunning = new Thread(Run);
            thrRunning.Start();
        }

        public void Stop()
        {
            botIsReceiving = false;
            Thread.Sleep(1000);
            if (thrRunning != null)
            {
                thrRunning.Abort();
                thrRunning = null;
            }
        }

        private string getUserName(long id)
        {
            foreach (var user in MemberList)
            {
                if (user.UserId == id)
                    return user.UserName;
            }
            return string.Empty;
        }
        private void Run()
        {
            int offset = 0;
            while (botIsReceiving)
            {
                try
                {
                    Update[] updates = GameConstants.botClient.GetUpdatesAsync(offset).Result;

                    foreach (var update in updates)
                    {
                        try
                        {
                            if (update.Type == UpdateType.Message && update.Message != null)
                            {
                                if (update.Message.Type == MessageType.Text)
                                {
                                    var message = update.Message;

                                    if (message.Text == null)
                                        continue;

                                    var chat = message.Chat;
                                    if (chat.Type != ChatType.Private) //only accept private messages from individual users.
                                        continue;

                                    string operator_name = getUserName(update.Message.Chat.Id);
                                    if (string.IsNullOrEmpty(operator_name))
                                        continue;

                                    Trace.WriteLine("From : bot@" + chat.Username, message.Text);

                                   
                                    string reply = string.Empty;
                                    InlineKeyboardMarkup replyMarkup = null;
                                    
                                    if (message.Text.Trim().StartsWith("/manage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        reply = "What will you do, my friend?";
                                        replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Init);
                                        ReqStep[update.Message.Chat.Id] = REQUESTSTEP.Init;
                                    }
                                    else if (message.Text.Trim().StartsWith("/history", StringComparison.InvariantCultureIgnoreCase))                                        
                                    {
                                        if (operator_name == "developer")
                                        {
                                            string[] names = message.Text.Trim().Split('_');
                                            if (names.Count() == 2)
                                                reply = GetActivityHistory(names[1]);
                                        }
                                        else
                                        {
                                            reply = "Only developer can see history, sorry";
                                        }
                                    }
                                    else
                                    {
                                        if (ReqStep.ContainsKey(update.Message.Chat.Id))
                                        {
                                            if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.Browse)
                                            {
                                                reply = BrowseLicense(operator_name, message.Text.Trim());
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                            else if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.Extend_1)
                                            {
                                                reply = ExtendLicense(operator_name, message.Text.Trim(), 7);
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                            else if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.Extend_2)
                                            {
                                                reply = ExtendLicense(operator_name, message.Text.Trim(), 14);
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                            else if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.Extend_3)
                                            {
                                                reply = ExtendLicense(operator_name, message.Text.Trim(), 30);
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                            else if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.ChangePackage)
                                            {
                                                reply = ChangePackagePrepare(operator_name, message.Text.Trim());
                                                if (string.IsNullOrEmpty(change_pachage_license))
                                                {
                                                    ReqStep.Remove(update.Message.Chat.Id);
                                                }
                                                else
                                                {
                                                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Change_SelPackage);
                                                    ReqStep[update.Message.Chat.Id] = REQUESTSTEP.Change_SelPackage;
                                                }
                                            }
                                            else if (ReqStep[update.Message.Chat.Id] == REQUESTSTEP.Remove)
                                            {
                                                reply = RemoveLicense(operator_name, message.Text.Trim());
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                            else
                                            {
                                                reply = "Wrong Command";
                                                ReqStep.Remove(update.Message.Chat.Id);
                                            }
                                        }
                                        else
                                        {
                                            reply = "Wrong Command";
                                            ReqStep.Remove(update.Message.Chat.Id);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(reply))
                                    {
                                        Telegram.Bot.Types.Message retmsg = SendMessageToUser(update.Message.Chat.Id, reply, replyMarkup).Result;
                                    }
                                }                                
                            }
                            else if (update.Type == UpdateType.CallbackQuery)
                            {
                                KeyboardCallback(update);
                            }
                        }
                        catch (Exception e)
                        {
                            // MessageBox.Show(e.Message, "Error");
                            Trace.WriteLine("Exception", e.Message);                            
                        }
                        finally
                        {
                            offset = update.Id + 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Exception", e.Message);                    
                }
            }
            thrRunning = null;
        }

        private async void KeyboardCallback(Update update)
        {
            if (update.CallbackQuery == null || update.CallbackQuery.From == null)
                return;

            string name = getUserName(update.CallbackQuery.From.Id);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!ReqStep.ContainsKey(update.CallbackQuery.From.Id))
            {
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Init;
                //return;
            }

            REQUESTSTEP Step = ReqStep[update.CallbackQuery.From.Id];
            string data = update.CallbackQuery.Data;                              

            string reply = string.Empty;
            InlineKeyboardMarkup replyMarkup = null;


            if (Step == REQUESTSTEP.Init)
            {
                change_pachage_license = "";
                add_package_id = 0;
            }

            if (data == "browse" && Step == REQUESTSTEP.Init)
            {
                reply = "Please input license for checking.";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Browse;
            }
            else if (data == "add" && Step == REQUESTSTEP.Init)
            {
                reply = "Please select package.";
                add_package_id = 0;
                replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add_SelPackage);
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add_SelPackage;
            }
            else if (data == "package_1")
            {
                if (Step == REQUESTSTEP.Add_SelPackage)
                {
                    add_package_id = 1;
                    reply = "Select period";
                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add);
                    ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add;
                }
                else if(Step == REQUESTSTEP.Change_SelPackage)
                {
                    reply = ChangePackage(name, 1);
                    ReqStep.Remove(update.CallbackQuery.From.Id);
                }
            }
            else if (data == "package_2")
            {
                if (Step == REQUESTSTEP.Add_SelPackage)
                {
                    add_package_id = 2;
                    reply = "Select period";
                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add);
                    ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add;
                }
                else if (Step == REQUESTSTEP.Change_SelPackage)
                {
                    reply = ChangePackage(name, 2);
                    ReqStep.Remove(update.CallbackQuery.From.Id);
                }
            }
            else if (data == "package_3")
            {
                if (Step == REQUESTSTEP.Add_SelPackage)
                {
                    add_package_id = 3;
                    reply = "Select period";
                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add);
                    ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add;
                }
                else if (Step == REQUESTSTEP.Change_SelPackage)
                {
                    reply = ChangePackage(name, 3);
                    ReqStep.Remove(update.CallbackQuery.From.Id);
                }
            }
            else if (data == "package_4")
            {
                if (Step == REQUESTSTEP.Add_SelPackage)
                {
                    add_package_id = 4;
                    reply = "Select period";
                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add);
                    ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add;
                }
                else if (Step == REQUESTSTEP.Change_SelPackage)
                {
                    reply = ChangePackage(name, 4);
                    ReqStep.Remove(update.CallbackQuery.From.Id);
                }
            }
            else if (data == "package_5")
            {
                if (Step == REQUESTSTEP.Add_SelPackage)
                {
                    add_package_id = 5;
                    reply = "Select period";
                    replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Add);
                    ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Add;
                }
                else if (Step == REQUESTSTEP.Change_SelPackage)
                {
                    reply = ChangePackage(name, 5);
                    ReqStep.Remove(update.CallbackQuery.From.Id);
                }
            }
            else if (data == "add_1" && Step == REQUESTSTEP.Add)
            {
                reply = AddLicense(name, 7, add_package_id);
                ReqStep.Remove(update.CallbackQuery.From.Id);
                add_package_id = 0;
            }
            else if (data == "add_2" && Step == REQUESTSTEP.Add)
            {
                reply = AddLicense(name, 14, add_package_id);
                ReqStep.Remove(update.CallbackQuery.From.Id);
                add_package_id = 0;
            }
            else if (data == "add_3" && Step == REQUESTSTEP.Add)
            {
                reply = AddLicense(name, 30, add_package_id);
                ReqStep.Remove(update.CallbackQuery.From.Id);
                add_package_id = 0;
            }
            else if (data == "extend" && Step == REQUESTSTEP.Init)
            {
                reply = "Please select period.";
                replyMarkup = GetAskReplyMarkup(REQUESTSTEP.Extend);
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Extend;
            }
            else if (data == "extend_1" && Step == REQUESTSTEP.Extend)
            {
                reply = "Please input license to extend";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Extend_1;
            }
            else if (data == "extend_2" && Step == REQUESTSTEP.Extend)
            {
                reply = "Please input license to extend";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Extend_2;
            }
            else if (data == "extend_3" && Step == REQUESTSTEP.Extend)
            {
                reply = "Please input license to extend";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Extend_3;
            }
            else if (data == "changepackage" && Step == REQUESTSTEP.Init)
            {
                reply = "Please input license to change package";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.ChangePackage;
            }
            else if (data == "remove" && Step == REQUESTSTEP.Init)
            {
                reply = "Please input license to remove";
                ReqStep[update.CallbackQuery.From.Id] = REQUESTSTEP.Remove;
            }
            else
            {
                reply = "Wrong Command.";

                change_pachage_license = "";
                add_package_id = 0;
                ReqStep.Remove(update.CallbackQuery.From.Id);
            }

            if (!string.IsNullOrEmpty(reply))
            {
                Telegram.Bot.Types.Message retmsg = await SendMessageToUser(update.CallbackQuery.From.Id, reply, replyMarkup);                  
            }            
        }

        private InlineKeyboardMarkup GetAskReplyMarkup(REQUESTSTEP Step)
        {
            InlineKeyboardMarkup markup = null;
            switch (Step)
            {
                case REQUESTSTEP.Init:
                    { 
                        InlineKeyboardButton[][] temp = new InlineKeyboardButton[5][];
                        InlineKeyboardButton button = new InlineKeyboardButton { Text = "Browse License", CallbackData = "browse" };
                        temp[0] = new InlineKeyboardButton[1] { button };
                        InlineKeyboardButton button1 = new InlineKeyboardButton { Text = "Add New License", CallbackData = "add" };
                        temp[1] = new InlineKeyboardButton[1] { button1 };
                        InlineKeyboardButton button2 = new InlineKeyboardButton { Text = "Extend License", CallbackData = "extend" };
                        temp[2] = new InlineKeyboardButton[1] { button2 };
                        InlineKeyboardButton button3 = new InlineKeyboardButton { Text = "Change Package", CallbackData = "changepackage" };
                        temp[3] = new InlineKeyboardButton[1] { button3 };
                        InlineKeyboardButton button4 = new InlineKeyboardButton { Text = "Remove License", CallbackData = "remove" };
                        temp[4] = new InlineKeyboardButton[1] { button4 };

                        markup = new InlineKeyboardMarkup(temp);
                    }
                    break;
                case REQUESTSTEP.Add_SelPackage:
                case REQUESTSTEP.Change_SelPackage:
                    {
                        InlineKeyboardButton[][] temp = new InlineKeyboardButton[5][];
                        InlineKeyboardButton button = new InlineKeyboardButton { Text = $"{cServerSettings.GetInstance().Package1_Price}€ Package", CallbackData = "package_1" };
                        temp[0] = new InlineKeyboardButton[1] { button };
                        InlineKeyboardButton button1 = new InlineKeyboardButton { Text = $"{cServerSettings.GetInstance().Package2_Price}€ Package", CallbackData = "package_2" };
                        temp[1] = new InlineKeyboardButton[1] { button1 };
                        InlineKeyboardButton button2 = new InlineKeyboardButton { Text = $"{cServerSettings.GetInstance().Package3_Price}€ Package", CallbackData = "package_3" };
                        temp[2] = new InlineKeyboardButton[1] { button2 };
                        InlineKeyboardButton button3 = new InlineKeyboardButton { Text = $"{cServerSettings.GetInstance().Package4_Price}€ Package", CallbackData = "package_4" };
                        temp[3] = new InlineKeyboardButton[1] { button3 };
                        InlineKeyboardButton button4 = new InlineKeyboardButton { Text = $"{cServerSettings.GetInstance().Package5_Price}€ Package", CallbackData = "package_5" };
                        temp[4] = new InlineKeyboardButton[1] { button4 };

                        markup = new InlineKeyboardMarkup(temp);
                    }
                    break;
                case REQUESTSTEP.Add: //Add New
                    {
                        InlineKeyboardButton[][] temp = new InlineKeyboardButton[3][];
                        InlineKeyboardButton button = new InlineKeyboardButton { Text = "Add 7 days License", CallbackData = "add_1" };
                        temp[0] = new InlineKeyboardButton[1] { button };
                        InlineKeyboardButton button1 = new InlineKeyboardButton { Text = "Add 14 days License", CallbackData = "add_2" };
                        temp[1] = new InlineKeyboardButton[1] { button1 };
                        InlineKeyboardButton button2 = new InlineKeyboardButton { Text = "Add 1 month License", CallbackData = "add_3" };
                        temp[2] = new InlineKeyboardButton[1] { button2 };

                        markup = new InlineKeyboardMarkup(temp);
                    }
                    break;
                case REQUESTSTEP.Extend: //Extend 
                    {
                        InlineKeyboardButton[][] temp = new InlineKeyboardButton[3][];
                        InlineKeyboardButton button = new InlineKeyboardButton { Text = "Extend 7 days License", CallbackData = "extend_1" };
                        temp[0] = new InlineKeyboardButton[1] { button };
                        InlineKeyboardButton button1 = new InlineKeyboardButton { Text = "Extend 14 days License", CallbackData = "extend_2" };
                        temp[1] = new InlineKeyboardButton[1] { button1 };
                        InlineKeyboardButton button2 = new InlineKeyboardButton { Text = "Extend 1 month License", CallbackData = "extend_3" };
                        temp[2] = new InlineKeyboardButton[1] { button2 };

                        markup = new InlineKeyboardMarkup(temp);
                    }
                    break;
                default:
                    break;
            }

            return markup;
        }
        private void SendMessageToEveryUser(long IdChat, string message)
        {
            if (IdChat != 0)
                GameConstants.botClient.SendTextMessageAsync(IdChat, message);            
        }

        public IEnumerable<string> SplitString(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
        private async Task<Telegram.Bot.Types.Message> SendMessageToUser(long IdChat, string message, InlineKeyboardMarkup markup = null)
        {
            Telegram.Bot.Types.Message retmsg = null;
            try
            {
                if (IdChat != 0)
                {
                    Trace.WriteLine("To : @" + IdChat, message);

                    foreach (string splitMsg in SplitString(message, 4095))
                    {
                        try
                        {
                            if (markup == null)
                                retmsg = await GameConstants.botClient.SendTextMessageAsync(IdChat, splitMsg, ParseMode.Html);
                            else
                                retmsg = await GameConstants.botClient.SendTextMessageAsync(IdChat, splitMsg, ParseMode.Html, replyMarkup: markup);
                        }
                        catch (Exception e)
                        {
                            // MessageBox.Show(e.Message, "Error");
                            Trace.WriteLine("Exception", e.Message);
                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error");
                Trace.WriteLine("Exception", e.Message);
            }
            return retmsg;
        }

        private string BrowseLicense(string name, string license)
        {
            string reply = "";

            string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", license, name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                reply = $"{license}    ";
                if (result[0][3].ToLower() != "true")
                    reply += "Deactivated ";
                UInt32 nPackageid = Utils.ParseToUInt(result[0][4]);
                string price = GetPackageLabel(nPackageid);
                
                reply += $"({price}€ package) ";
                
                reply += $"Expire at {result[0][9]}";
            }
            else
            {
                reply = $"{license}    doesn't exist. Please check license again.";
            }
            return reply;
        }

        private string ExtendLicense(string name, string license, int days)
        {
            string reply = "";

            string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", license, name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                DateTime dupdate = DateTime.Parse(result[0][9]);
                if (dupdate > DateTime.Now)
                {
                    dupdate = dupdate.AddDays(days);
                }
                else
                {
                    dupdate = DateTime.Now.AddDays(days);
                }

                UInt32 packageid = Utils.ParseToUInt(result[0][4]);
                string price = GetPackageLabel(packageid);

                query = string.Format("UPDATE users SET activation=1, updated_at='{1}' WHERE license='{0}' AND name='{2}'", license, dupdate.ToString("yyyy-MM-dd HH:mm:ss"), name);
                MYSqlMng.GetInstance().UpdateQuery(query);

                reply = $"{license} ({price}€ package)   added {days}days  Expire at {dupdate.ToString("yyyy-MM-dd")}";

                AddHistory(name, $"extend ({price}€ package) {days}days", $"{license}");
            }
            else
            {
                reply = $"{license}    doesn't exist. Please check license again.";
            }

            return reply;
        }
        private string ChangePackagePrepare(string name, string license)
        {
            string reply = "";

            string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", license, name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                reply = $"{license}    ";

                UInt32 nPackageid = Utils.ParseToUInt(result[0][4]);

                string price = GetPackageLabel(nPackageid);
                
                reply += $"({price}€ package) ";
                        
                reply += $"Expire at {result[0][9]}";

                if (result[0][3].ToLower() != "true")
                {
                    reply += " already Deactivated, Can't change package ";
                    return reply;
                }
                reply += "\n Please select new package type!";
                change_pachage_license = license;
            }
            else
            {
                reply = $"{license}    doesn't exist. Please check license again.";
            }

            return reply;
        }

        private string ChangePackage(string name, UInt32 newPackageID)
        {
            string reply = "";

            if (string.IsNullOrEmpty(change_pachage_license))
            {
                change_pachage_license = "";
                return "license is incorrect";
            }

            if (newPackageID < 1 || newPackageID > 5)
            {
                change_pachage_license = "";
                return "New Package type is incorrect";
            }

            string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", change_pachage_license, name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                reply = $"{change_pachage_license}    ";

                UInt32 OrigPackageid = Utils.ParseToUInt(result[0][4]);
                string origprice = GetPackageLabel(OrigPackageid);
                reply += $"({origprice}€ package)   ";
                if (OrigPackageid == newPackageID)
                {
                    reply += "Error: New package is same as before.";
                    change_pachage_license = "";
                    return reply;
                }

                query = string.Format("UPDATE users SET group_number={1} WHERE license='{0}' AND name='{1}'", change_pachage_license, newPackageID, name);
                MYSqlMng.GetInstance().UpdateQuery(query);

                string newprice = GetPackageLabel(newPackageID);
                reply += $"is changed to ({newprice}€ package)";

                AddHistory(name, $"change package ({origprice}€ -> {newprice}€)", change_pachage_license);
            }
            else
            {
                reply = $"{change_pachage_license}    doesn't exist. Please check license again.";
            }
            change_pachage_license = "";
            return reply;
        }

        private string GetPackageLabel(UInt32 packageid)
        {
            string price = "###";
            switch (packageid)
            {
                case 1:
                    price = cServerSettings.GetInstance().Package1_Price;
                    break;
                case 2:
                    price = cServerSettings.GetInstance().Package2_Price;
                    break;
                case 3:
                    price = cServerSettings.GetInstance().Package3_Price;
                    break;
                case 4:
                    price = cServerSettings.GetInstance().Package4_Price;
                    break;
                case 5:
                    price = cServerSettings.GetInstance().Package5_Price;
                    break;
            }
            return price;
        }
        private string RemoveLicense(string name, string license)
        {
            string reply = "";

            string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", license, name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                query = string.Format("UPDATE users SET activation=0 WHERE license='{0}' AND name='{1}'", license, name);
                MYSqlMng.GetInstance().UpdateQuery(query);
                

                UInt32 packageid = Utils.ParseToUInt(result[0][4]);

                string price = GetPackageLabel(packageid);
                reply = $"{license} ({price}€ package) is removed successflly.";

                AddHistory(name, $"remove ({price}€ package) license", license);
            }
            else
            {
                reply = $"{license}    doesn't exist. Please check license again.";
            }

            return reply;
        }
        private string AddLicense(string name, int days, UInt32 packageid)
        {
            string reply = "Adding license Error!";
            if (packageid < 1 || packageid > 5)
            {
                reply += $" Unkown package id {packageid}";
                return reply;
            }

            int nRetry = 100;
            while (--nRetry > 0)
            {
                string license = RandomHexString(10).ToLower();

                string query = string.Format("SELECT * FROM users WHERE license='{0}' AND name='{1}'", license, name);
                string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
                if (result != null && result.Length > 0)
                {
                    continue;
                }
                else
                {
                    query = string.Format("INSERT INTO users(license, name, activation, group_number, ip, first_login_at, period, created_at, updated_at) VALUES ('{0}', '{1}', {2}, {3}, '{4}', '{5}', {6}, '{7}', '{8}')",
                        license, name, 1, packageid, "telegram", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), days, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.AddDays(days).ToString("yyyy-MM-dd HH:mm:ss"));

                    MYSqlMng.GetInstance().InsertQuery(query);
                    reply = $"{license}";

                    string price = GetPackageLabel(packageid);
                    
                    AddHistory(name, $"add ({price}€ package) {days}days", $"{license}");
                    break;
                }
            }
            
            return reply;
        }

        private void AddHistory(string name, string method, string description)
        {
            string query = string.Format("INSERT INTO managehistory(name, time, method, description, processed) VALUES ('{0}', '{1}', '{2}', '{3}', {4})",
                name, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), method, description, 0);

            MYSqlMng.GetInstance().InsertQuery(query);
        }

        private string GetActivityHistory(string name)
        {
            string reply = "";
            string query = string.Format("SELECT * FROM managehistory WHERE processed=0 AND name='{0}'", name);
            string[][] result = MYSqlMng.GetInstance().SelectQuery(query);
            if (result != null && result.Length > 0)
            {
                foreach (string[] line in result)
                {
                    reply += line[1] + "  " + line[2] + "  " + line[3] + "   " + line[4] + Environment.NewLine;
                }

                query = string.Format("UPDATE managehistory SET processed=1");
                MYSqlMng.GetInstance().UpdateQuery(query);
            }
            else
            {
                reply = "No activity";
            }
            
            return reply;
        }

        private Random random = new Random();
        public string RandomHexString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
#endif
}
