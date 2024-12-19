using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Globalization;
using System.Net;

namespace BetburgerServer.Constant
{
    public class Utils
    {
        public const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
        public static string prev;
        public static int seed;
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

        public static long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            return dtDateTime;
        }

        public static string get_yeast()
        {
            string yeast = string.Empty;
            try
            {
                decimal unixTimestamp = Math.Round((decimal)(DateTime.UtcNow.AddSeconds(40).Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
                yeast = encode(unixTimestamp);
                if (yeast != prev)
                {
                    prev = yeast;
                    seed = 0;
                }
                else
                    yeast = yeast + "." + encode(++seed);

            }
            catch (Exception)
            {

            }
            return yeast;
        }

        public static string encode(decimal num)
        {
            decimal length = chars.Length;
            string encodedStr = string.Empty;
            try
            {
                do
                {
                    encodedStr = chars[Utils.ParseToInt((num % length).ToString())].ToString() + encodedStr;
                    num = Math.Floor(num / length);
                }
                while (num > 0);

            }
            catch (Exception e)
            {

            }
            return encodedStr;
        }
        public static string GetMyIPAddress()
        {
#if DEBUG
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            IPAddress[] addr = ipEntry.AddressList;

            return addr[addr.Length - 1].ToString();
#else
            
            return "176.223.142.38";
            
#endif

        }
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static int ParseToInt(string str)
        {
            int val = 0;
            int.TryParse(str, out val);
            return val;
        }

        public static UInt32 ParseToUInt(string str)
        {
            UInt32 val = 0;
            UInt32.TryParse(str, out val);
            return val;
        }

        public static double ParseToDouble(string str)
        {
            double val = 0;
            try
            {
                string temp = str.Replace(',', '.');
                double.TryParse(temp, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
            }
            catch(Exception e)
            {
            }
            return val;
        }

        public static long ParseToLong(string str)
        {
            long val = 0;
            long.TryParse(str, out val);
            return val;
        }

        public static bool isExistDB(string[][] dbList, string dbName)
        {
            foreach(string[] db in dbList)
            {
                if (db == null || db.Length < 1)
                    continue;

                if (db[0] == dbName)
                    return true;
            }

            return false;
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
        public static string Base64Encoding(string EncodingText, System.Text.Encoding oEncoding = null)
        {
            if (string.IsNullOrEmpty(EncodingText))
                return string.Empty;
            if (oEncoding == null)
                oEncoding = System.Text.Encoding.UTF8;

            byte[] arr = oEncoding.GetBytes(EncodingText);
            return System.Convert.ToBase64String(arr);
        }

        public static string Base64Decoding(string DecodingText, System.Text.Encoding oEncoding = null)
        {
            if (string.IsNullOrEmpty(DecodingText))
                return string.Empty;
            if (oEncoding == null)
                oEncoding = System.Text.Encoding.UTF8;

            byte[] arr = System.Convert.FromBase64String(DecodingText);
            return oEncoding.GetString(arr);
        }

        public static string ToHexString(string param)
        {
            return ByteArrayToString(StringToByte(param));
        }

        public static string ToLiteralString(string param)
        {
            return ByteToString(StringToByteArray(param));
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private static string ByteToString(byte[] strByte)
        {
            string str = Encoding.UTF8.GetString(strByte);
            return str;
        }

        private static byte[] StringToByte(string str)
        {
            byte[] StrByte = Encoding.UTF8.GetBytes(str);
            return StrByte;
        }

        private static string convertDoubleTeam(string team)
        {
            string[] teams = team.Split(new char[] { ',' });
            if (teams == null || teams.Length < 2)
                return team;

            return string.Format("{0} {1}", teams[1].Trim(), teams[0].Trim());
        }

        public static bool isSameMatch(string mHome, string mAway, string eHome, string eAway, bool bAdvanced = false)
        {
            if (mHome.Contains(",") && mAway.Contains(","))
                return isSameMatch(convertDoubleTeam(mHome), convertDoubleTeam(mAway), eHome, eAway, true);

            if (mHome == eHome && mAway == eAway)
                return true;

            if (mHome == eHome && (mAway.Contains(eAway) || eAway.Contains(mAway) || JaroWinklerDistance.distance(mAway, eAway) < 0.25))
                return true;

            if (mAway == eAway && (mHome.Contains(eHome) || eHome.Contains(mHome) || JaroWinklerDistance.distance(mHome, eHome) < 0.25))
                return true;

            if (bAdvanced)
            {
                if ((mHome.Contains(eHome) || eHome.Contains(mHome) || JaroWinklerDistance.distance(mHome, eHome) < 0.15) && (mAway.Contains(eAway) || eAway.Contains(mAway) || JaroWinklerDistance.distance(mAway, eAway) < 0.15))
                    return true;
            }

            return false;
        }

        private static bool isSameTeam(string team1, string team2)
        {
            if (team1.Contains(team2))
                return true;

            if (team2.Contains(team1))
                return true;

            if (JaroWinklerDistance.distance(team1, team2) < 0.15 || JaroWinklerDistance.distance(team2, team1) < 0.15)
                return true;

            return false;
        }

        public static string Between(string STR, string FirstString, string LastString, string[] contains)
        {
            int nStartIndex = 0;
            string FinalString = "";
            while (true)
            {
                int nFindIndex = STR.IndexOf(FirstString, nStartIndex);
                if (nFindIndex == -1)
                    break;
                int Pos1 = nFindIndex + FirstString.Length;

                if (LastString != null)
                {
                    string subSTR = STR.Substring(Pos1);
                    int Pos2 = subSTR.IndexOf(LastString);
                    FinalString = subSTR.Substring(0, Pos2);
                    foreach (string contain in contains)
                    { 
                        if (FinalString.Contains(contain))
                            return FinalString;
                    }
                    nStartIndex = Pos1;
                }
                else
                {
                    break;
                }
            }

            return "";
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
    }
}
