using Newtonsoft.Json;
using Project.Models;
using Project.Server;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project.Helphers
{
    public class Utils
    {
        private static NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;
        private static CultureInfo culture = CultureInfo.CreateSpecificCulture("es");

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
            if (sports == "Soccer")
                return 1;
            else if (sports == "Volleyball")
                return 91;
            else if (sports == "Baseball")
                return 16;
            else if (sports == "Basketball")
                return 18;
            else if (sports == "American Football")
                return 12;
            else if (sports == "Hockey")
                return 17;
            else if (sports == "Tennis")
                return 13;
            else if (sports == "Table Tennis")
                return 92;
            else if (sports == "Horse Racing")
                return 2;
            else if (sports == "Rugby")
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

        public static int parseToInt(string str)
        {
            int val = 0;
            int.TryParse(str, out val);
            return val;
        }

        public static double ParseToDouble(string str)
        {
            str = str.Replace("\"", "").Replace(",", ".").Replace(" ", "");
            double value = 0;
            double.TryParse(str, style, CultureInfo.InvariantCulture, out value);
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
            return result;
        }
        public static OpenBet_Bet365 ConvertBetburgerPick2OpenBet_365(BetburgerInfo info)
        {
            OpenBet_Bet365 openBet = new OpenBet_Bet365();
            BetData_Bet365 betData = new BetData_Bet365();

            string[] linkArray = info.direct_link.Split('|');
            if (linkArray.Count() == 3)
            {
                betData.fd = linkArray[2];
                betData.i2 = linkArray[0];
                betData.oddStr = linkArray[1];
            }
            else
            {
                try
                {
                    dynamic jsonResResp = JsonConvert.DeserializeObject<dynamic>(info.direct_link);

                    betData.fd = jsonResResp["f"]["FI"].ToString();
                    betData.i2 = jsonResResp["f"]["ID"].ToString();
                    betData.oddStr = jsonResResp["f"]["OD"].ToString();
                }
                catch
                {
                    return null;
                }
            }

            
            betData.cl = GetSportsID(info.sport).ToString(); 
            try
            {
                betData.ht = Utils.Between(info.outcome, "(", ")").Replace("+", "");
            }
            catch { betData.ht = ""; }

            openBet.betData.Add(betData);

            openBet.stake = info.stake;
            openBet.id = $"BS{betData.fd}-{betData.i2}";
            
            return openBet;
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
        {
            OpenBet_Goldbet openBet = new OpenBet_Goldbet();

            string[] linkArray = info.direct_link.Split('|');

            if (linkArray.Count() != 8)
                return null;

            openBet.market_mn = linkArray[0];
            openBet.market_mi = linkArray[1];
            openBet.market_mti = linkArray[2];
            openBet.market_oc = linkArray[3];
            openBet.market_sn = linkArray[5];
            openBet.market_si = linkArray[6];
            openBet.market_oi = linkArray[7];   //not use

            return openBet;
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
                string marketId = info.direct_link.Split('|')[1];
                string offerId = info.direct_link.Split('|')[0];
                int oddVal = (int)(info.odds * 1000);
                return new OpenBet_Unibet(marketId, offerId, "", oddVal.ToString());
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public static string Between(string STR, string FirstString, string LastString = null)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            if (LastString != null)
            {
                STR = STR.Substring(Pos1);
                int Pos2 = STR.IndexOf(LastString);
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
    }
}
