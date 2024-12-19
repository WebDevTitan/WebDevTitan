using BetburgerServer.Model;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BetburgerServer.Constant
{
    public enum ODDTYPE
    {
        Threeway = 69,
        Points = 192,
        Totals = 47,
        Ahc = 48,
        Dnb = 112,
        Ehc = 8,
        SetHandicap = 71,
        SetOverUnder = 100,
        GameHandicap = 39,
        Doubble = 9,
        homeAway = 70,
        Moneyline = 466
    };
    public class GameConstants
    {
        public static CookieContainer container = new CookieContainer();
        public static bool bRun = false;
        public static bool bRunning = false;

        public static dynamic betsapiEvents = null;
        public static DateTime betsapiEventsRefreshTime = DateTime.MinValue;

        public static Object lockertrademateIDLists = new object();
        public static List<string> trademateIDLists = new List<string>();
        public static string trademateSID = "";
        

        public static string TradematesportsPunterId;
        
        public static Object betheaderLocker = new object();
        public static List<BetHeaderInfo> BetHeaders = new List<BetHeaderInfo>();
#if (!FORSALE)
        public static TelegramBotClient botClient = null;
#endif

        public static string tokenScript = "";
        public static string B365SimpleEncryptJS =
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
        public static string NSTTokenJS =
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

        public static JObject Tradematesports_BookieData;
        public static JObject Tradematesports_SportData;

        public static string Tradematesports_getBookieTitle(string bookieId)
        {
            if (Tradematesports_BookieData[bookieId] != null)
                return Tradematesports_BookieData[bookieId]["name"].ToString();
            return "Unknown";
        }

        public static string Tradematesports_getSportTitle(string sportId)
        {
            if (Tradematesports_SportData[sportId] != null)
                return Tradematesports_SportData[sportId]["name"].ToString();
            return "Unknown";
        }

        

        public static IDictionary<int, string> Tradematesports_eventTypeIds = new Dictionary<int, string>()
        {
            { 533 , "Whole Match"},
            { 65 , "1st Quarter"},
            { 801 , "1st Quarter"},
            { 596 , "1st Quarter"},
            { 693 , "1st Quarter"},
            { 35 , "1st Quarter"},
            { 355 , "1st Quarter"},
            { 335 , "1st Quarter"},
            { 1 , "Whole Event"},
            { 2 , "Whole Match"},
            { 3 , "Ordinary Time"},
            { 4 , "Overtime"},
            { 350 , "Whole Match"},
            { 40 , "Whole Match"},
            { 180 , "Whole Match"},
            { 5 , "1st Half (Ordinary Time)"},
            { 600 , "Whole Match"},
            { 6 , "2nd Half (Ordinary Time)"},
            { 41 , "Ordinary Time"},
            { 523 , "Rest of Match"},
            { 531 , "Whole Match"},
            { 601 , "Whole Match"},
            { 210 , "Whole Match"},
            { 320 , "Whole Match"},
            { 772 , "Whole Match"},
            { 70 , "Whole Match"},
            { 602 , "Rest of Ordinary Time"},
            { 532 , "Whole Match"},
            { 524 , "Overtime Excluding Penalty Round"},
            { 810 , "Whole Match"},
            { 100 , "Whole Match"},
            { 240 , "Whole Match"},
            { 666 , "1st Intermission"},
            { 44 , "2nd Period"},
            { 9 , "Penalty Round"},
            { 45 , "3rd Period"},
            { 43 , "1st Period"},
            { 200 , "Whole Match"},
            { 537 , "7th Set"},
            { 661 , "Ordinary Time Over"},
            { 111 , "1st Set"},
            { 330 , "Whole Match"},
            { 140 , "Whole Match"},
            { 201 , "Ordinary Time"},
            { 170 , "Whole Match"},
            { 22 , "2nd Set"},
            { 21 , "1st Set"},
            { 20,"Whole Match"},
            { 60 , "Whole Match" },
            { 63 , "1st Half (Ordinary Time)"}
        };

        public static string TrademateSports_displayOddType(JsonTrade trader)
        {
            string oddType = string.Empty;

            ODDTYPE type = Enum.IsDefined(typeof(ODDTYPE), trader.oddsType)
                                  ? (ODDTYPE)trader.oddsType
                                  : ODDTYPE.Threeway;

            switch (type)
            {
                case ODDTYPE.Threeway:
                    {
                        if (trader.participant == trader.homeTeamId && trader.output == "o1")
                            oddType = trader.homeTeam;
                        else if (trader.participant == trader.awayTeamId && trader.output == "o3")
                            oddType = trader.awayTeam;
                        else
                        {
                            //trader.runnerText = "Draw";
                            oddType = "Draw";
                        }

                        trader.market = "Full Time Result";
                    }
                    break;
                case ODDTYPE.SetOverUnder:
                    {
                        if (trader.output == "o1")
                            oddType = string.Format("Over {0} Sets", trader.oddsTypeCondition);
                        else if (trader.output == "o2")
                            oddType = string.Format("Under {0} Sets", trader.oddsTypeCondition);
                        else
                            oddType = "N/A";

                        trader.market = "Over/Under";
                    }
                    break;
                case ODDTYPE.Totals:
                    {
                        if (trader.typeId == 13)
                            oddType = string.Format("Over {0}", trader.oddsTypeCondition);
                        else if (trader.typeId == 14)
                            oddType = string.Format("Under {0}", trader.oddsTypeCondition);
                        else
                            oddType = "N/A";

                        trader.market = "Total Goals";
                    }
                    break;
                case ODDTYPE.homeAway:
                    {
                        if (trader.participant == trader.homeTeamId)
                            oddType = string.Format("{0} to win", trader.homeTeam);
                        else
                            oddType = string.Format("{0} to win", trader.awayTeam);

                        trader.market = "To Win Match/Game Lines";
                    }
                    break;
                case ODDTYPE.Moneyline:
                    {
                        if (trader.participant == trader.homeTeamId)
                            oddType = string.Format("{0} to win", trader.homeTeam);
                        else
                            oddType = string.Format("{0} to win", trader.awayTeam);

                        trader.market = "Money Line";
                    }
                    break;
                case ODDTYPE.Dnb:
                    {
                        if (trader.participant == trader.awayTeamId)
                            oddType = string.Format("Draw no bet ({0})", trader.awayTeam);
                        else
                            oddType = string.Format("Draw no bet ({0})", trader.homeTeam);

                        trader.market = "Draw No Bet";
                    }
                    break;
                case ODDTYPE.Ahc:
                    {
                        if (trader.participant == trader.homeTeamId)
                            oddType = string.Format("Asian hcp {0}{1} ({2})", (int)trader.oddsTypeCondition >= 0 ? "+" : "", trader.oddsTypeCondition, trader.homeTeam);
                        else if (trader.participant == trader.awayTeamId)
                            oddType = string.Format("Asian hcp {0}{1} ({2})", (int)trader.oddsTypeCondition >= 0 ? "+" : "", trader.oddsTypeCondition, trader.awayTeam);
                        else
                            oddType = "N/A";

                        trader.market = "Asian Handicap";
                    }
                    break;
                case ODDTYPE.Points:
                    {
                        if (trader.output == "o1")
                        {
                            if (trader.oddsTypeCondition < 0)
                                oddType = string.Format("Handicap  {0} ({1})", trader.oddsTypeCondition.ToString("N2"), trader.homeTeam);
                            else
                                oddType = string.Format("Handicap +{0} ({1})", trader.oddsTypeCondition.ToString("N2"), trader.homeTeam);
                        }
                        else
                        {
                            if (trader.oddsTypeCondition < 0)
                                oddType = string.Format("Handicap  {0} ({1})", trader.oddsTypeCondition * -1, trader.awayTeam);
                            else
                                oddType = string.Format("Handicap  {0} ({1})", trader.oddsTypeCondition * -1, trader.awayTeam);
                        }
                        trader.market = "Point Spread";
                    }
                    break;
                case ODDTYPE.Ehc:
                    {
                        string resultString = "";

                        if (trader.oddsTypeCondition >= 0)
                            resultString = string.Format("({0}-0)", trader.oddsTypeCondition);
                        else
                            resultString = string.Format("(0{0})", trader.oddsTypeCondition);

                        if (trader.output == "o1")
                            oddType = string.Format("Euro hcp {0} ({1})", resultString, trader.homeTeam);
                        else
                            oddType = string.Format("Euro hcp {0} ({1})", resultString, trader.awayTeam);

                        trader.market = "3-Way Handicap";
                    }
                    break;
                case ODDTYPE.SetHandicap:
                    break;
                case ODDTYPE.GameHandicap:
                    {
                        string typeStr = "Game";
                        if (trader.participant == trader.homeTeamId)
                            oddType = string.Format("{3} hcp {0}{1} ({2})", trader.oddsTypeCondition >= 0 ? "+" : "", trader.oddsTypeCondition, trader.homeTeam, typeStr);
                        else if (trader.participant == trader.awayTeamId)
                            oddType = string.Format("{3} hcp {0}{1} ({2})", trader.oddsTypeCondition >= 0 ? "+" : "", trader.oddsTypeCondition, trader.awayTeam, typeStr);
                        else
                            oddType = "N/A";

                        trader.market = "Total Games";
                    }
                    break;
                default:
                    oddType = "N/A";
                    break;
            };

            return oddType;
        }

        public static string TrademateSports_GetMarketName(JsonTrade trade, string leagueName)
        {
            bool isFound = false;
            string selectionId = string.Empty;
            string directLink = string.Empty;
            Dictionary<string, string> market_link_pairs = new Dictionary<string, string>();
            try
            {
                if (trade.sportName == "American football")
                {
                    if (trade.period == "Whole Match" && trade.market == "Money Line")
                    {
                        return "Money Line";
                    }
                    else if (trade.period == "Whole Match" && trade.market == "Asian Handicap")
                    {
                        return "Spread";
                        //return "Point Spread 2-Way";
                    }
                    else if (trade.period == "Whole Match" && trade.marketText == "3-Way Handicap")
                    {
                        return "Game Lines 3-Way";
                        //return "Alternative Handicap 3-Way";
                    }
                    else if (trade.period == "Whole Match" && trade.market == "Total Goals")
                    {
                        return "Game Lines";
                        //return "Alternative Total 2-Way";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Money Line")
                    {
                        return "1st Half Money Line";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.marketText == "3-Way Handicap")
                    {
                        return "1st Half Spread 3-Way";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Total Goals")
                    {
                        return "1st Half Totals";
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Money Line")
                    {
                        return "1st Quarter Lines 2-Way";
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Total Goals")
                    {
                        return "Total 1st Quarter Lines 2-Way";
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Asian Handicap")
                    {
                        return "Spread 1st Quarter Lines 2-Way";
                        //return "1st Quarter Alternative Point Spread";
                    }
                }
                else if (trade.sportName == "Basket")
                {
                    if (trade.period == "Whole Match" && trade.market == "Money Line")
                    {
                        return "Game Lines";
                        //return "Money Line";
                    }
                    else if (trade.period == "Whole Match" && trade.market == "Asian Handicap")
                    {
                        return "Game Lines";
                        //"Spread"
                        //"Point Spread"))
                    }
                    else if (trade.period == "Whole Match" && trade.marketText == "3-Way Handicap")
                    {
                        return "Game Lines 3-Way";
                    }
                    else if (trade.period == "Whole Match" && trade.market == "Total Goals")
                    {
                        return "Game Lines";
                        //return "Total";
                        //"Game Total"))
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Money Line")
                    {
                        return "1st Half Money Line";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.marketText == "3-Way Handicap")
                    {
                        return "1st Half Spread 3-Way";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.marketText == "Asian handicap")
                    {
                        return "Alternative 1st Half Point Spread";
                        //"1st Half Spread"
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Total Goals")
                    {
                        return "1st Half Totals";
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Total Goals")
                    {
                        return "1st Quarter";
                        //"1st Quarter Lines 2-Way"
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Asian Handicap")
                    {
                        return "1st Quarter Spread";
                        //"1st Quarter Point Spread"))
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "Money Line")
                    {
                        return "1st Quarter Money Line";
                    }
                    else if (trade.period == "1st Quarter" && trade.market == "3-Way Handicap")
                    {
                        return "1st Quarter 3 Way Lines";
                        //"Spread";
                    }
                }
                if (trade.sportName == "Tennis")
                {
                    if (trade.period == "Whole Match" && trade.market == "Money Line")
                    {
                        return "To Win Match";
                    }
                    else if (trade.period == "1st Set" && trade.market == "Money Line")
                    {
                        return "First Set Winner";
                    }
                    else if (trade.period == "Whole Match" && trade.marketText == "Game Handicap")
                    {
                        return "Match Handicap";
                    }
                }
                else if (trade.sportName == "Soccer")
                {
                    //핸디캡인 경우 오드아이디 찾기
                    if (trade.period == "Ordinary Time" && trade.market == "Asian Handicap")
                    {
                        return "Asian Handicap";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Asian Handicap")
                    {
                        return "1st Half Asian Handicap";
                    }
                    else if (trade.period == "Ordinary Time" && trade.market == "Total Goals")
                    {
                        return "Total Goals";
                    }
                    else if (trade.period == "1st Half (Ordinary Time)" && trade.market == "Total Goals")
                    {
                        return "1st Half Total Goals";
                    }
                    else if (trade.period == "Ordinary Time" && trade.market == "Full Time Result")
                    {
                        return "Full Time Result";
                    }
                    else if (trade.period == "Ordinary Time" && trade.market == "Draw No Bet")
                    {
                        return "Draw No Bet";
                    }
                    if (trade.period == "Ordinary Time" && trade.market == "3-Way Handicap")
                    {
                        return "3-Way Handicap Result";
                    }
                }
                else if (trade.sportName == "Ice Hockey")
                {
                    if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "Money Line")
                    {
                        return "Money Line";
                    }
                    else if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "Full Time Result")
                    {
                        return "3-Way Money Line";
                    }
                    else if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "Draw No Bet")
                    {
                        //return "Draw No Bet";
                        return "Money Line";
                    }
                    else if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "Asian Handicap")
                    {
                        return "Spread";
                        //return "Alternative Puck Line 2-Way" || marketName == "Asian Handicap")
                    }
                    else if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "3-Way Handicap")
                    {
                        return "3-Way Handicap";
                        //return "Alternative Puck Line 3-Way")
                    }
                    else if ((trade.period == "Ordinary Time" || trade.period == "Whole Match") && trade.market == "Total Goals")
                    {
                        return "Total Goals";
                        // ("Alternative Total 2-Way"))
                    }
                    else if (trade.period == "1st Period" && trade.market == "Total Goals")
                    {
                        return "1st Quarter Toal Goals";
                    }
                    else if (trade.period == "1st Period" && trade.market == "Asian Handicap")
                    {
                        return "1st Quater Point Spread";
                    }
                    else if (trade.period == "1st Period" && trade.market == "Money Line")
                    {
                        return "1st Quarter Money Line";
                    }
                    else if (trade.period == "1st Period" && trade.market == "3-Way Handicap")
                    {
                        return "Spread 1st Quarter 3 Way Lines";
                    }
                }
            }
            catch
            {
            }
            return "";
        }
    }
}
