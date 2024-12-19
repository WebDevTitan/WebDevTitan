using Bet365LiveAgent.Data.Soccer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp.Server;

namespace Bet365LiveAgent
{
    public enum LOGLEVEL : byte
    {
        FILE = 0,
        NOTICE,
        FULL
    }

    public enum LOGTYPE : byte
    {
        INDATA = 0,
        OUTDATA
    }

    public enum BET365CLIENT_STATUS : byte
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    public enum BET365AGENT_STATUS : byte
    {
        Stoped = 0,
        Started
    }

    public enum BET365MEDIA_FORMAT : byte
    {
        FLASH = 1,
        HLS = 2,
        MPEG_DASH = 3,
        MP4 = 4
    }

    public enum CORNERCARD
    {
        CORNER = 1,
        CARDS = 2,
        NOTHING = 3,
    }

    public enum OVERUNDER
    {
        CORNER_OVER = 1,
        CORNER_UNDER = 2,
        CARDS_OVER = 3,
        CARDS_UNDER = 4,
        GOALS_OVER = 5,
        GOALS_UNDER = 6,
        NOTHING = 7
    }

    public enum BET365MEDIA_PROVIDER : byte
    {
        MEDIA_ON_DEMAND_MOBILE = 0,
        RUK_MOBILE = 6,
        WATCH_AND_BET_MOBILE = 7,
        DUBAI_RACING_MOBILE = 9,
        UNAS_MOBILE = 10,
        VIRTUAL_MOBILE = 12,
        ATR_MOBILE = 15,
        IMG_MOBILE = 20,
        BET_RADAR_MOBILE = 22,
        TELSTRA_MOBILE = 24,
        SIS_MOBILE = 27,
        AWS_TELEIPICCA_MOBILE = 29,
        SPORT_LEVEL_MOBILE = 31,
        BET_GENIUS_MOBILE = 33
    }

    public delegate void WriteLogDelegate(LOGLEVEL logLevel, LOGTYPE logType, string strLog);

    public delegate void LiveAgentNotify(string name, string param1, string param2, string param3, string param4);

    public delegate void WriteResultDelegate(List<PickResultData> results);

    public delegate void WriteMatcheDelegate(List<SoccerMatchData> matches);

    public delegate void Bet365ClientHandler();

    public delegate void Bet365DataSendHandler(string strData);

    public delegate void ProcessBet365DataDelegate(JArray jArrData);

    public delegate void Bet365DataReceivedHandler(string strData);

    public delegate void Bet365RequestDelegate(string key, object value);

    static class Global
    {
        public static WebSocketServer socketServer;

        public const string ConfigFileName = "config.xml";

        public static string LogFilePath = $"{Application.StartupPath}\\logs\\";

        public static string LogFileName = "b365log";

        public static LiveAgentNotify LiveResultNotifier = null;
        public static WriteLogDelegate WriteLog = null;
        public static WriteMatcheDelegate matchEvent = null;
        public static WriteResultDelegate resultEvent = null;

        public static List<PickResultData> resultHistory = new List<PickResultData>();

        public static string LANG_ID = "1";
        public static string ZONE_ID = "3";
        public static string ZONE
        {
            get
            {
                string zone = ZONE_ID;
                switch (LANG_ID)
                {
                    case "1":
                        zone = "1";
                        break;
                    case "3":
                        zone = "0";
                        break;
                    case "32":
                        zone = "0";
                        break;
                }
                return zone;
            }
        }

        public const string DELIM_RECORD = "\u0001";
        public const string DELIM_FIELD = "\u0002";
        public const string DELIM_HANDSHAKE_MSG = "\u0003";
        public const string DELIM_MSG = "\u0008";
        public const char CLIENT_CONNECT = (char)0;
        public const char CLIENT_POLL = (char)1;
        public const char CLIENT_SEND = (char)2;
        public const char CLIENT_CONNECT_FAST = (char)3;
        public const char INITIAL_TOPIC_LOAD = (char)20;
        public const char DELTA = (char)21;
        public const char CLIENT_SUBSCRIBE = (char)22;
        public const char CLIENT_UNSUBSCRIBE = (char)23;
        public const char CLIENT_SWAP_SUBSCRIPTIONS = (char)26;
        public const char NONE_ENCODING = (char)0;
        public const char ENCRYPTED_ENCODING = (char)17;
        public const char COMPRESSED_ENCODING = (char)18;
        public const char BASE64_ENCODING = (char)19;
        public const char SERVER_PING = (char)24;
        public const char CLIENT_PING = (char)25;
        public const char CLIENT_ABORT = (char)28;
        public const char CLIENT_CLOSE = (char)29;
        public const char ACK_ITL = (char)30;
        public const char ACK_DELTA = (char)31;
        public const char ACK_RESPONSE = (char)32;
        public const char HANDSHAKE_PROTOCOL = (char)35;//'#';
        public const char HANDSHAKE_VERSION = (char)3;
        public const char HANDSHAKE_CONNECTION_TYPE = (char)80;//'P';
        public const char HANDSHAKE_CAPABILITIES_FLAG = (char)1;
        public const string HANDSHAKE_STATUS_CONNECTED = "100";
        public const string HANDSHAKE_STATUS_REJECTED = "111";

        public const string B365SimpleEncryptJS =
            @"function B365SimpleEncrypt() { }
            B365SimpleEncrypt.encrypt = function(t) {
                var n, i = '',
                    r = t.length,
                    o = 0,
                    s = 0;
                for (o = 0; r > o; o++) {
                    for (n = t.substr(o, 1), s = 0; s < B365SimpleEncrypt.MAP_LEN; s++)
                        if (n == B365SimpleEncrypt.charMap[s][0])
                        {
                            n = B365SimpleEncrypt.charMap[s][1];
                            break
                        }
                        i += n
                }
                return i
            }, B365SimpleEncrypt.decrypt = function(t) {
                var n, i = '',
                    r = t.length,
                    o = 0,
                    s = 0;
                for (o = 0; r > o; o++) {
                    for (n = t.substr(o, 1), s = 0; s < B365SimpleEncrypt.MAP_LEN; s++) {
                        if (':' == n && ':|~' == t.substr(o, 3)) {
                            n = '\n', o += 2;
                            break
                        }
                        if (n == B365SimpleEncrypt.charMap[s][1]) {
                            n = B365SimpleEncrypt.charMap[s][0];
                            break
                        }
                    }
                    i += n
                }
                return i
            }, B365SimpleEncrypt.MAP_LEN = 64, B365SimpleEncrypt.charMap = [
                ['A', 'd'],
			    ['B', 'e'],
			    ['C', 'f'],
			    ['D', 'g'],
			    ['E', 'h'],
			    ['F', 'i'],
			    ['G', 'j'],
			    ['H', 'k'],
			    ['I', 'l'],
			    ['J', 'm'],
			    ['K', 'n'],
			    ['L', 'o'],
			    ['M', 'p'],
			    ['N', 'q'],
			    ['O', 'r'],
			    ['P', 's'],
			    ['Q', 't'],
			    ['R', 'u'],
			    ['S', 'v'],
			    ['T', 'w'],
			    ['U', 'x'],
			    ['V', 'y'],
			    ['W', 'z'],
			    ['X', 'a'],
			    ['Y', 'b'],
			    ['Z', 'c'],
			    ['a', 'Q'],
			    ['b', 'R'],
			    ['c', 'S'],
			    ['d', 'T'],
			    ['e', 'U'],
			    ['f', 'V'],
			    ['g', 'W'],
			    ['h', 'X'],
			    ['i', 'Y'],
			    ['j', 'Z'],
			    ['k', 'A'],
			    ['l', 'B'],
			    ['m', 'C'],
			    ['n', 'D'],
			    ['o', 'E'],
			    ['p', 'F'],
			    ['q', '0'],
			    ['r', '1'],
			    ['s', '2'],
			    ['t', '3'],
			    ['u', '4'],
			    ['v', '5'],
			    ['w', '6'],
			    ['x', '7'],
			    ['y', '8'],
			    ['z', '9'],
			    ['0', 'G'],
			    ['1', 'H'],
			    ['2', 'I'],
			    ['3', 'J'],
			    ['4', 'K'],
			    ['5', 'L'],
			    ['6', 'M'],
			    ['7', 'N'],
			    ['8', 'O'],
			    ['9', 'P'],
			    ['\n', ':|~'],
			    ['\r', '']
		    ];";
        public const string NSTTokenJS =
            @"var boot = {}, ue = [], de = [];
            boot.ef = (function() {
                var e = 0
                    , t = 0
                    , n = 0;
                return function(o) {
                    e % 2 != 0 && (2 > t ? ue[t++] = o : 3 > n && (de[n++] = o)),
                    e++
                }
            })();
            boot.gh = (function() {
                var e = 0
                    , t = 0
                    , n = 0;
                return function(o) {
                    e > 0 && e % 2 == 0 && (2 > t ? ue[t++] = o : 3 > n && (de[n++] = o)),
                    e++
                }
            })();
            /***nstTokenLib***/
            B365SimpleEncrypt.decrypt(ue.join('') + String.fromCharCode(46) + de.join(''));";
    }
}
