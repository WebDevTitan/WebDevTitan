using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Protocol;

namespace Project.Helphers
{
    public class Utils
    {
        private static NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;
        private static CultureInfo culture = CultureInfo.CreateSpecificCulture("es");

        public static HttpClient getHttpClient(string proxy = "")
        {
            HttpClientHandler handler;

            if (!string.IsNullOrEmpty(proxy) && proxy != null)
            {
                var proxyURI = new Uri(string.Format("{0}{1}", "http://", proxy));
                WebProxy proxyItem;
                var useAuth = false;
                /*
                var credentials = new NetworkCredential(Setting.instance.proxyUsername, Setting.instance.proxyPassword);
                proxyItem = new WebProxy(proxyURI, true, null, credentials);
                useAuth = true;
                */
                proxyItem = new WebProxy(proxyURI, false);
                handler = new HttpClientHandler()
                {
                    Proxy = proxyItem,
                    UseProxy = true,
                    PreAuthenticate = useAuth,
                    UseDefaultCredentials = !useAuth,
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            HttpClient httpClientEx = new HttpClient(handler);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            httpClientEx.Timeout = new TimeSpan(0, 0, 5);
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, image/webp, */*; q=0.8");
            httpClientEx.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36");
            httpClientEx.DefaultRequestHeaders.ExpectContinue = false;
            return httpClientEx;
        }
        public static string generateGuid()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString().ToLower();
        }
        public static string DecodeBase64(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;
                var valueBytes = System.Convert.FromBase64String(value);
                return System.Text.Encoding.UTF8.GetString(valueBytes);
            }
            catch
            {
                return string.Empty;
            }

        }
        public static int GetUIDofSlip()
        {
            int nResult = 100 + random.Next(0, 4096);
            return nResult;
        }
        public static string LaunchBrowser()
        {
            try
            {
                HttpClient httpClient = getHttpClient();
                HttpResponseMessage httpResp = null;
                string puppeteerUrl = string.Empty;

                httpResp = httpClient.GetAsync("http://127.0.0.1:35000/api/v1/profile/start?automation=true&profileId=" + Setting.Instance.vmLoginProfile).Result;
                string strContent = httpResp.Content.ReadAsStringAsync().Result;
                LogMng.Instance.onWriteStatus(strContent);
                dynamic jsContent = JsonConvert.DeserializeObject<dynamic>(strContent);
                puppeteerUrl = jsContent.value.ToString();
                Thread.Sleep(3 * 1000);

                return puppeteerUrl;
            }
            catch
            {

            }
            return string.Empty;
        }
        public static string CloseBrowser()
        {
            try
            {

                HttpClient httpClient = getHttpClient();
                HttpResponseMessage httpResp = null;

                httpResp = httpClient.GetAsync("http://127.0.0.1:35000/api/v1/profile/stop?profileId=" + Setting.Instance.vmLoginProfile).Result;
                string strContent = httpResp.Content.ReadAsStringAsync().Result;
                dynamic jsContent = JsonConvert.DeserializeObject<dynamic>(strContent);

            }
            catch
            {
            }
            return string.Empty;
        }
        public static Rectangle ParseRectFromJson(string json)
        {
            Rectangle result = new Rectangle();
            try
            {
                string refinedStr = json.Replace("\\", "").Replace("\"", "");
                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(refinedStr);

                string x = jsonContent.x.ToString();
                result.X = (int)Utils.ParseToDouble(x);
                result.Y = (int)Utils.ParseToDouble(jsonContent.y.ToString());
                result.Width = (int)Utils.ParseToDouble(jsonContent.width.ToString());
                result.Height = (int)Utils.ParseToDouble(jsonContent.height.ToString());
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static Rect ParseRect(string json)
        {
            Rect result = new Rect();
            try
            {
                string refinedStr = json.Replace("\\", "").Replace("\"", "");
                dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(refinedStr);

                string x = jsonContent.x.ToString();
                result.X = Utils.ParseToDouble(x);
                result.Y = Utils.ParseToDouble(jsonContent.y.ToString());
                result.Width = Utils.ParseToDouble(jsonContent.width.ToString());
                result.Height = Utils.ParseToDouble(jsonContent.height.ToString());
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static string ParseOpenBet(string json)
        {
            int nPendingbetCount = 0;
            double lPendingbetBalance = 0;

            List<OpenBet_Bet365> curBetList = Utils.ParseBet365OpenBets(json);

            foreach (OpenBet_Bet365 bet in curBetList)
            {
                nPendingbetCount++;
                lPendingbetBalance += bet.stake;

                foreach (BetData_Bet365 betData in bet.betData)
                {
                    if (betData.eachway)
                    {
                        lPendingbetBalance += bet.stake;
                        break;
                    }
                }
            }

            return $"Current Balance: {Global.balance} Pending: {lPendingbetBalance}({nPendingbetCount} bets) Total: {Global.balance + lPendingbetBalance}";
        }

        public static double GetOddFromAddbetReply(string param)
        {
            double result = 0;
            dynamic jsonRes = JsonConvert.DeserializeObject<dynamic>(param);
            result = Utils.FractionToDouble(jsonRes.bt[0].od.ToString());
            return result;
        }

        public static CookieCollection GetAllCookies(CookieContainer cookieJar)
        {
            CookieCollection cookieCollection = new CookieCollection();

            Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            cookieJar,
                                                                            new object[] { });

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string)listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }


        public static string encode(string encodeStr)
        {
            string decodeStr = string.Empty;
            try
            {
                string v = string.Format("#{0:X}", encodeStr.Length);
                int len = 4 - v.Length;
                decodeStr = String.Concat(Enumerable.Repeat("0", len)) + v + encodeStr;
            }
            catch (Exception e)
            {

            }
            return decodeStr;

        }
        public static int GetSportsID(string sports)
        {
            if (sports.ToLower() == "soccer")
                return 1;
            else if (sports.ToLower() == "volleyball")
                return 91;
            else if (sports.ToLower() == "baseball")
                return 16;
            else if (sports.ToLower() == "basketball")
                return 18;
            else if (sports.ToLower() == "american football")
                return 12;
            else if (sports.ToLower() == "hockey")
                return 17;
            else if (sports.ToLower() == "tennis")
                return 13;
            else if (sports.ToLower() == "table tennis")
                return 92;
            else if (sports.ToLower() == "horse racing")
                return 2;
            else if (sports.ToLower() == "rugby")
                return 19;
            return 1;
        }
        public static long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }


        public static bool parseToBool(string str)
        {
            bool val = false;
            bool.TryParse(str, out val);
            return val;
        }
        public static decimal ParseToDecimal(string str)
        {
            decimal value = 0;
            decimal.TryParse(str, style, culture, out value);
            return value;
        }
        public static int parseToInt(string str)
        {
            int val = 0;
            int.TryParse(str, out val);
            return val;
        }

        public static double ParseToDouble(string str)
        {
            if (str.Contains(",") && str.Contains("."))
                str = str.Replace(".", string.Empty).Replace(",", ".");

            str = str.Replace("\"", "").Replace(",", ".").Replace(" ", "");
            double value = 0;
            try
            {
                double.TryParse(str, style, CultureInfo.InvariantCulture, out value);
            }
            catch { }
            return value;
        }

        public static decimal parseToDecimal(string str)
        {
            decimal val = 0;
            decimal.TryParse(str, out val);
            return val;
        }

        public static long parseToLong(string str)
        {
            long val = 0;
            long.TryParse(str.Replace(",", ""), out val);
            return val;
        }

        public static double FractionToDoubleOfEachway(string fraction, int ed)
        {
            double result;

            if (double.TryParse(fraction, out result))
            {
                return (result - 1) / ed + 1;
            }

            string[] split = fraction.Split(new char[] { ' ', '/' });

            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;

                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        if (b == 0 || ed == 0)
                            return 0;
                        return 1 + (double)a / b / ed;
                    }

                    int c;

                    if (int.TryParse(split[2], out c))
                    {
                        if (c == 0)
                            return 0;
                        return a + (double)b / c;
                    }
                }
            }

            throw new FormatException("Not a valid fraction. => " + fraction);
        }
        public static double FractionToDouble(string fraction)
        {
            double result;

            if (double.TryParse(fraction, out result))
            {
                return result;
            }

            string[] split = fraction.Split(new char[] { ' ', '/' });

            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;

                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        if (b == 0)
                            return 0;
                        return 1 + (double)a / b;
                    }

                    int c;

                    if (int.TryParse(split[2], out c))
                    {
                        if (c == 0)
                            return 0;
                        return a + (double)b / c;
                    }
                }
            }

            throw new FormatException("Not a valid fraction. => " + fraction);
        }

        public static string GetString(byte[] param, ENCODINGFMT encFMT)
        {
            switch (encFMT)
            {
                case ENCODINGFMT.UNICODE:
                    return Encoding.Unicode.GetString(param);

                case ENCODINGFMT.UTF7:
                    return Encoding.UTF7.GetString(param);

                case ENCODINGFMT.UTF8:
                    return Encoding.UTF8.GetString(param);

                case ENCODINGFMT.UTF32:
                    return Encoding.UTF32.GetString(param);
            }
            return Encoding.ASCII.GetString(param);
        }

        public static byte[] GetBytes(string strParam, ENCODINGFMT encFMT)
        {
            switch (encFMT)
            {
                case ENCODINGFMT.UNICODE:
                    return Encoding.Unicode.GetBytes(strParam);

                case ENCODINGFMT.UTF7:
                    return Encoding.UTF7.GetBytes(strParam);

                case ENCODINGFMT.UTF8:
                    return Encoding.UTF8.GetBytes(strParam);

                case ENCODINGFMT.UTF32:
                    return Encoding.UTF32.GetBytes(strParam);
            }
            return Encoding.ASCII.GetBytes(strParam);
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }


        public static OpenBet_Bet365 parseBet365BsString2OpenBet(string bs)
        {

            OpenBet_Bet365 result = null;
            try
            {
                LogMng.Instance.onWriteStatus($"parseBsString2OpenBet {bs}");

                string ns = Between(bs, "ns:", "&");
                string[] bsArray = ns.Split(new string[] { "||" }, StringSplitOptions.None);
                foreach (string bsVal in bsArray)
                {
                    if (string.IsNullOrEmpty(bsVal.Trim()))
                        continue;

                    BetData_Bet365 betData = new BetData_Bet365();
                    MatchCollection fM = Regex.Matches(bsVal, @"#f=(?<VAL>[^\#]*)#");
                    MatchCollection fpM = Regex.Matches(bsVal, @"#fp=(?<VAL>[^\#]*)#");
                    MatchCollection oddM = Regex.Matches(bsVal, @"#o=(?<VAL>[^\#]*)#");
                    MatchCollection clM = Regex.Matches(bsVal, @"#c=(?<VAL>[^\#]*)#");
                    MatchCollection lineM = Regex.Matches(bsVal, @"#ln=(?<VAL>[^\#]*)#");


                    if (fM.Count > 0)
                    {
                        betData.fd = Between(fM[0].Value, "#f=", "#");
                    }
                    if (fpM.Count > 0)
                    {
                        betData.i2 = Between(fpM[0].Value, "#fp=", "#");
                    }
                    if (oddM.Count > 0)
                    {
                        betData.oddStr = Between(oddM[0].Value, "#o=", "#");
                    }
                    if (clM.Count > 0)
                    {
                        betData.cl = Between(clM[0].Value, "#c=", "#");
                    }
                    if (lineM.Count > 0)
                    {
                        betData.ht = Between(lineM[0].Value, "#ln=", "#");
                    }


                    if (result == null)
                    {
                        result = new OpenBet_Bet365();
                    }

                    if (string.IsNullOrEmpty(result.id))
                        result.id = $"BS{betData.fd}-{betData.i2}";

                    result.betData.Add(betData);
                }


                MatchCollection TPM = Regex.Matches(bs, @"TP=(?<VAL>[^\#]*)#");
                MatchCollection ustM = Regex.Matches(bs, @"#ust=(?<VAL>[^\#]*)#");
                MatchCollection ewM = Regex.Matches(bs, @"#ew=(?<VAL>[^\#]*)#");

                LogMng.Instance.onWriteStatus($"parseBsString2OpenBet  TPM.Count {TPM.Count}");

                for (int i = 0; i < TPM.Count; i++)
                {
                    string tpmValue = Between(TPM[i].Value, "TP=", "#");
                    if (tpmValue.Contains("x"))
                    {
                        tpmValue = tpmValue.Substring(0, tpmValue.IndexOf("x"));
                    }

                    LogMng.Instance.onWriteStatus($"parseBsString2OpenBet  tpmValue {tpmValue}");


                    if (ustM.Count > 0)
                    {
                        result.stake = Utils.ParseToDouble(Between(ustM[0].Value, "#ust=", "#"));
                    }

                    if (ewM.Count > 0)
                    {
                        foreach (var betdata in result.betData)
                        {
                            if (betdata.cl == "2")    //Horse Racing
                                betdata.eachway = true;
                        }
                    }
                    break;

                }

            }
            catch (Exception ex)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"parseBet365BsString2OpenBet {bs} {ex}");
#endif
            }
            return result;
        }

        public static List<OpenBet_Bet365> ParseBet365OpenBets(string jsonStr)
        {
            List<OpenBet_Bet365> result = new List<OpenBet_Bet365>();
            if (jsonStr.StartsWith("\""))
                jsonStr = jsonStr.Substring(1, jsonStr.Length - 2);
            jsonStr = jsonStr.Replace("\\\"", "\"");
            dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(jsonStr);
            foreach (dynamic betContent in jsonContent)
            {
                OpenBet_Bet365 openBet = new OpenBet_Bet365();
                openBet.id = betContent.id.ToString();
                openBet.stake = Utils.ParseToDouble(betContent.st.ToString());

                try
                {
                    foreach (dynamic participant in betContent.data)
                    {
                        BetData_Bet365 betData = new BetData_Bet365();
                        betData.id = participant.id.ToString();
                        betData.oddStr = participant.od.ToString();
                        betData.odd = Utils.FractionToDouble(betData.oddStr);
                        betData.eachway = betContent.ew.ToString() == "1";
                        betData.explanation = participant.ex.ToString();
                        betData.name = participant.na.ToString();
                        betData.fd = participant.fd.ToString();
                        betData.i2 = participant.i2.ToString();
                        betData.ht = participant.ht.ToString();
                        betData.cl = participant.cl.ToString();
                        openBet.betData.Add(betData);
                    }
                    result.Add(openBet);
                }
                catch { }
            }
            return result;
        }

        public static OpenBet_Sisal ConvertBetburgerPick2OpenBet_Sisal(BetburgerInfo info)
        {
            try
            {
                OpenBet_Sisal openBet = new OpenBet_Sisal();

                string[] linkArray = info.direct_link.Split('|');
                if (linkArray.Count() == 7)
                {
                    for (int i = 0; i < 6; i++)
                        openBet.eventIds.Add(linkArray[i]);
                    openBet.sublink = linkArray[6];
                    return openBet;
                }

            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public static OpenBet_Betfair ConvertBetburgerPick2OpenBet_Betfair(BetburgerInfo info)
        {
            OpenBet_Betfair openBet = new OpenBet_Betfair();
            try
            {

                string paramArray = info.direct_link + "&";

                string bseId = Utils.Between(paramArray, "bseId=", "&");
                string bsmId = Utils.Between(paramArray, "bsmId=", "&");
                string bssId = Utils.Between(paramArray, "bssId=", "&");

                if (!string.IsNullOrEmpty(bseId) && !string.IsNullOrEmpty(bsmId) && !string.IsNullOrEmpty(bssId))
                    return new OpenBet_Betfair(bseId, bsmId, bssId, bseId);

            }
            catch (Exception ex)
            {
            }
            return null;
        }


        public static OpenBet_Bet365 ConvertBetburgerPick2OpenBet_365(BetburgerInfo info)
        {
            OpenBet_Bet365 openBet = new OpenBet_Bet365();
            try
            {
                if (info.kind == PickKind.Type_2 || info.kind == PickKind.Type_6)
                {
                    return null;
                }
                if (info.kind == PickKind.Type_5)
                {
                    try
                    {
                        openBet = JsonConvert.DeserializeObject<OpenBet_Bet365>(info.eventTitle);
                    }
                    catch
                    {
                        openBet = null;
                    }
                    return openBet;
                }

                BetData_Bet365 betData = new BetData_Bet365();

                string[] linkArray = info.direct_link.Split('|');
                if (linkArray.Count() == 3 || linkArray.Count() == 4)
                {
                    betData.fd = linkArray[2];
                    betData.i2 = linkArray[0];
                    betData.oddStr = linkArray[1];

                    betData.ht = "";
                    try
                    {
                        betData.ht = Utils.Between(info.outcome, "(", ")");
                    }
                    catch (Exception ex)
                    {
#if (TROUBLESHOT)
                        //LogMng.Instance.onWriteStatus($"ConvertBetburgerPick2OpenBet_365 ht parse exception {info.outcome}");
#endif
                    }
                }
                else
                {
                    try
                    {
                        dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(info.direct_link);

                        betData.fd = jsonResResp["f"]["FI"].ToString();
                        betData.i2 = jsonResResp["f"]["ID"].ToString();
                        betData.oddStr = jsonResResp["f"]["OD"].ToString();

                        try
                        {
                            betData.ht = jsonResResp["f"]["HA"].ToString();
                        }
                        catch
                        {
                            betData.ht = "";
                        }

                        try
                        {
                            info.outcome = jsonResResp["f"]["GTL"].ToString();

                            if (!string.IsNullOrEmpty(betData.ht))
                                info.outcome += "(" + betData.ht + ")";
                        }
                        catch { }
                    }
                    catch
                    {
                        if (info.eventUrl.Contains("PA;") && !string.IsNullOrEmpty(info.siteUrl))
                        {
                            betData.fd = Utils.Between(info.siteUrl, "#E", "#");
                            betData.i2 = Utils.Between(info.eventUrl, "PA;");
                            betData.oddStr = info.odds.ToString();

                            betData.ht = "";

                        }
                        else
                        {
                            return null;
                        }
                    }
                }


                betData.cl = GetSportsID(info.sport).ToString();

                openBet.betData.Add(betData);

                openBet.stake = info.stake;
                openBet.id = $"BS{betData.fd}-{betData.i2}";
                return openBet;
            }
            catch (Exception ex)
            {
#if (TROUBLESHOT)
                LogMng.Instance.onWriteStatus($"ConvertPick Exception {info.direct_link} {ex}");
#endif
            }

            try
            {
                if (info.kind == PickKind.Type_4)
                {
                    openBet = Utils.parseBet365BsString2OpenBet(info.eventTitle);

                    openBet.stake = Setting.Instance.stakePerTipster2;
                    return openBet;
                }
                else if (info.kind == PickKind.Type_5)
                {

                    openBet = JsonConvert.DeserializeObject<OpenBet_Bet365>(info.eventTitle);
                    openBet.stake = Setting.Instance.stakePerTipster2;
                    if (openBet.betData.Count > 0)
                    {
                        info.eventTitle = "Bet365 Tipster pick";
                        info.odds = openBet.betData[0].odd;
                    }
                    return openBet;
                }
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus($"ConvertPick Exception {info} {ex}");
            }

            return null;
        }

        public static OpenBet_Leovegas ConvertBetburgerPick2OpenBet_Leovegas(BetburgerInfo info)
        {
            OpenBet_Leovegas openBet = new OpenBet_Leovegas();

            string[] linkArray = info.direct_link.Split('|');

            if (linkArray.Count() != 2)
                return null;

            openBet.betOfferId = linkArray[0];
            openBet.outcomeId = linkArray[1];

            return openBet;
        }

        public static OpenBet_Goldbet ConvertBetburgerPick2OpenBet_Goldbet(BetburgerInfo info)
        {//live
            OpenBet_Goldbet openBet = new OpenBet_Goldbet();

            string[] linkArray = info.direct_link.Split('|');

            if (info.kind == PickKind.Type_1)
            {
                openBet.market_mn = WebUtility.UrlDecode(Between(info.direct_link, "marketName=", "&"));
                openBet.market_mi = Between(info.direct_link, "marketId=", "&");
                openBet.market_mti = Between(info.direct_link, "marketTypeId=", "&");
                openBet.market_oc = Between(info.direct_link, "outcomeName=", "&");
                openBet.market_sn = Between(info.direct_link, "tabId=", "&");
                openBet.market_si = Between(info.direct_link, "selectionId=", "&");
                openBet.market_oi = Between(info.direct_link, "outcomeId=", "&");

                openBet.isLive = true;
                return openBet;
            }
            //if (linkArray.Count() == 8)
            //{

            //    //openBet.market_mn = linkArray[0];
            //    //openBet.market_mi = linkArray[1];
            //    //openBet.market_mti = linkArray[2];
            //    //openBet.market_oc = linkArray[3];
            //    //openBet.market_sn = linkArray[5];
            //    //openBet.market_si = linkArray[6];
            //    //openBet.market_oi = linkArray[7];   //not use

            //    openBet.market_mn = WebUtility.UrlDecode(Between(info.direct_link, "marketName=", "&"));
            //    openBet.market_mi = Between(info.direct_link, "marketId=", "&");
            //    openBet.market_mti = Between(info.direct_link, "marketTypeId=", "&");
            //    openBet.market_oc = Between(info.direct_link, "outcomeName=", "&");
            //    openBet.market_sn = Between(info.direct_link, "tabId=", "&");
            //    openBet.market_si = Between(info.direct_link, "selectionId=", "&");
            //    openBet.market_oi = Between(info.direct_link, "outcomeId=", "&");

            //    openBet.isLive = true;
            //    return openBet;
            //}
            else if (linkArray.Count() == 1)
            {
                openBet.market_oi = linkArray[0];   //not use

                openBet.isLive = false;
                return openBet;
            }
            return null;
        }
        public static OpenBet_Eurobet ConvertBetburgerPick2OpenBet_Eurobet(BetburgerInfo info)
        {
            OpenBet_Eurobet openBet = new OpenBet_Eurobet();

            string[] linkArray = info.direct_link.Split('|');

            if (linkArray.Count() != 5)
                return null;

            openBet.marketId = Utils.parseToInt(linkArray[2].Replace("[", "").Replace("]", "").Split(',')[0]);

            openBet.betCode = Utils.parseToInt(linkArray[0]);
            openBet.resultCode = Utils.parseToInt(linkArray[1]);
            openBet.eventCode = Utils.parseToInt(linkArray[3]);
            openBet.programCode = Utils.parseToInt(linkArray[4]);
            return openBet;
        }
        public static OpenBet_Paddypower ConvertBetburgerPick2OpenBet_Paddypower(BetburgerInfo info)
        {
            OpenBet_Paddypower openBet = new OpenBet_Paddypower();

            string[] linkArray = info.direct_link.Split('|');

            if (linkArray.Count() != 4)
                return null;


            openBet.marketId = linkArray[0];
            openBet.selectionId = linkArray[1];
            return openBet;
        }

        public static OpenBet_Unibet ConvertBetburgerPick2OpenBet_Unibet(BetburgerInfo info)
        {
            OpenBet_Unibet openBet = new OpenBet_Unibet();
            try
            {
                string marketId = "";
                string offerId = "";

                if (info.direct_link.Contains("outcomeId="))
                {
                    marketId = Utils.Between(info.direct_link, "outcomeId=", "&betOfferId");
                    offerId = Utils.Between(info.direct_link + "xx", "betOfferId=", "xx");
                }
                else
                {
                    offerId = info.direct_link.Split('|')[0];
                    marketId = info.direct_link.Split('|')[1];
                }

                int oddVal = (int)(info.odds * 1000);
                return new OpenBet_Unibet(marketId, offerId, "", oddVal.ToString());
            }
            catch (Exception ex)
            {
                LogMng.Instance.onWriteStatus("ConvertBetburgerPick2OpenBet_Unibet " + ex.ToString().ToString());
            }
            return null;
        }
        public static string Between(string STR, string FirstString, string LastString = null)
        {
            string FinalString;
            if (STR.IndexOf(FirstString) < 0)
            {
                return string.Empty;
            }

            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            if (LastString != null)
            {
                STR = STR.Substring(Pos1);
                int Pos2 = STR.IndexOf(LastString);
                if (Pos2 < 0)
                {
                    return STR;
                }
                FinalString = STR.Substring(0, Pos2);
            }
            else
            {
                FinalString = STR.Substring(Pos1);
            }

            return FinalString;
        }

        private static Random random = new Random();
        public static string RandomHexString(int length)
        {
            const string chars = "ABCDEF0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string Javascript_Math_Random()
        {
            string res = "0." + ((long)(random.NextDouble() * 10000000000000)).ToString();
            return res;
        }
        public static string Base64Encode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in Base64Encode: " + e.Message);
            }
        }

        public static string ReplaceStr(string STR, string ReplaceSTR, string FirstString, string LastString)
        {
            if (!STR.Contains(FirstString))
            {
                return STR;
            }
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            if (LastString != null)
            {
                string text = STR.Substring(0, Pos1);
                int Pos2 = STR.IndexOf(LastString, Pos1);
                return text + ReplaceSTR + STR.Substring(Pos2);
            }
            return STR.Substring(0, Pos1) + ReplaceSTR;
        }

        public static bool IsValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static double ConvertOddFromAmericaToDecimal(string odd)
        {
            double result = 0;
            if (!IsCorrectAmericanOdd(odd))
                return -1;

            double value = double.Parse(odd);
            if (odd.StartsWith("-"))
            {
                result = 1 - (100 / value);
            }
            else
            {
                result = 1 + (value / 100);
            }
            return result;

        }

        public static bool IsCorrectAmericanOdd(string odd)
        {
            try
            {
                MatchCollection mc = Regex.Matches(odd, "^(\\+|-)(?<odd>\\d+)$");
                if (mc.Count == 1)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
        public static int GetRandValue(int minValue, int maxValue, bool pon = false)
        {
            int c = maxValue - minValue + 1;
            Random random = new Random();
            return (int)Math.Floor(random.NextDouble() * c + minValue) * (pon ? _pon() : 1);
        }
        public static int _pon()
        {
            return GetRandValue(10) >= 5 ? 1 : -1;
        }
        public static int GetRandValue(int maxValue)
        {
            Random random = new Random();
            return random.Next(0, maxValue);
        }
        public static double getDistance(string team1, string team2)
        {
            double distance = 0;
            distance = JaroWinklerDistance.distance(team1, team2);
            return distance;
        }
        public static string CreateFormDataBoundary()
        {
            return DateTime.Now.Ticks.ToString("x");
        }

        internal static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("x2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("x");
        }
    }
}
