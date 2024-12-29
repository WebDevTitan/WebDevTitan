using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Protocol
{
    public class BetData_Bet365
    {
        public string id;
        public double odd;
        public string oddStr;
        public bool eachway;
        public string explanation;
        public string name;
        public string fd;
        public string i2;
        public string ht;
        public string cl;

        public string sa;       //temporary value for betting 
        public bool oc;
        public string oo;
        public bool ea;
        public int ed;
    }

    public class ParseBet_Bet365
    {
        public double stake;
        public string Sport;
        public string League;
        public string Hometeam;
        public string Awayteam;

        public string TabLabel;
        public string MarketLabel;

        public string TableHeader;
        public string RowHeader;
        public string ColHeader;
        public string ParticipantName;

        public double odd;

        public ParseBet_Bet365()
        {
            Sport = "";
            League = "";
            Hometeam = "";
            Awayteam = "";

            TabLabel = "";
            MarketLabel = "";

            TableHeader = "";
            RowHeader = "";
            ColHeader = "";
            ParticipantName = "";
        }

        public ParseBet_Bet365(ParseBet_Bet365 init)
        {
            Sport = init.Sport;
            League = init.League;
            Hometeam = init.Hometeam;
            Awayteam = init.Awayteam;
            stake = init.stake;
            odd = init.odd;

            TabLabel = "";
            MarketLabel = "";

            TableHeader = "";
            RowHeader = "";
            ColHeader = "";
            ParticipantName = "";
        }

        public static bool IsCorrectParseBet(ParseBet_Bet365 parsebet)
        {
            if (parsebet == null || string.IsNullOrEmpty(parsebet.Hometeam) || string.IsNullOrEmpty(parsebet.Awayteam))
                return false;

            if (string.IsNullOrEmpty(parsebet.TabLabel) || string.IsNullOrEmpty(parsebet.MarketLabel))
                return false;

            if (string.IsNullOrEmpty(parsebet.RowHeader) && string.IsNullOrEmpty(parsebet.ColHeader) && string.IsNullOrEmpty(parsebet.ParticipantName))
                return false;
            return true;
        }

        public static ParseBet_Bet365 ConvertBetburgerPick2ParseBet_365(BetburgerInfo info, out ParseBet_Bet365 secondResult)
        {
            secondResult = null;
            ParseBet_Bet365 parseBet = new ParseBet_Bet365();

            parseBet.Sport = info.sport;
            if (string.IsNullOrEmpty(info.league))
                return null;
            parseBet.League = info.league;

            if (string.IsNullOrEmpty(info.homeTeam))
                return null;
            parseBet.Hometeam = info.homeTeam;

            if (string.IsNullOrEmpty(info.awayTeam))
                return null;
            parseBet.Awayteam = info.awayTeam;

            parseBet.stake = info.stake;
            parseBet.odd = info.odds;
            if (info.kind == PickKind.Type_9 || info.kind == PickKind.Type_12 || info.kind == PickKind.Type_13)
            {
                switch (info.outcome)
                {
                    case "1":
                    case "X":
                    case "2":
                    case "1 1st period":
                    case "X 1st period":
                    case "2 1st period":
                        {
                            if (info.sport == "hockey")
                            {
                                if (info.outcome.Contains("1st period"))
                                {

                                }
                                else
                                {
                                    parseBet.MarketLabel = "3-Way";
                                    parseBet.TabLabel = "Game Betting";
                                    parseBet.RowHeader = "Money Line";
                                    if (info.outcome.StartsWith("1"))
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (info.outcome.StartsWith("X"))
                                    {
                                        parseBet.ColHeader = "Tie";
                                    }
                                    else if (info.outcome.StartsWith("2"))
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }
                                }
                            }
                            else
                            {
                                if (info.outcome.Contains("1st period"))
                                {
                                    parseBet.MarketLabel = "Half Time Result";
                                    parseBet.TabLabel = "Half";
                                }
                                else
                                {
                                    parseBet.MarketLabel = "Full Time Result";
                                    parseBet.TabLabel = "Popular";
                                }

                                if (info.outcome.StartsWith("1"))
                                {
                                    parseBet.ParticipantName = "*home*";
                                }
                                else if (info.outcome.StartsWith("X"))
                                {
                                    parseBet.ParticipantName = "Draw";
                                }
                                else if (info.outcome.StartsWith("2"))
                                {
                                    parseBet.ParticipantName = "*away*";
                                }
                            }
                            break;
                        }
                    case "1X":
                    case "12":
                    case "X2":
                    case "1X 1st period":
                    case "12 1st period":
                    case "X2 1st period":
                        {
                            if (info.outcome.Contains("1st period"))
                            {
                                parseBet.MarketLabel = "Half Time Double Chance";
                                parseBet.TabLabel = "Half";
                            }
                            else
                            {
                                parseBet.MarketLabel = "Double Chance";
                                parseBet.TabLabel = "Popular";
                            }

                            if (info.outcome.StartsWith("1X"))
                            {
                                parseBet.ParticipantName = "*home* or Draw";
                            }
                            else if (info.outcome.StartsWith("12"))
                            {
                                parseBet.ParticipantName = "*home* or *away*";
                            }
                            else if (info.outcome.StartsWith("X2"))
                            {
                                parseBet.ParticipantName = "Draw or *away*";
                            }
                            break;
                        }
                    case "Score draw":
                        {
                            if (info.sport == "soccer")
                            {
                                parseBet.TabLabel = "Goals";
                                parseBet.MarketLabel = "Winning Margin";
                                parseBet.RowHeader = "Score Draw";
                            }
                        }
                        break;
                    default:
                        {
                            //Both to score - No 2nd period
                            //Soccer
                            MatchCollection mc = Regex.Matches(info.outcome, "^Both to score(?<no>( - No)?)((?<period> \\w+) period)?");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "soccer")
                                {
                                    parseBet.TabLabel = "Goals";
                                    parseBet.MarketLabel = "Both Teams to Score";
                                    if (m.Groups["no"].Value != "")
                                    {
                                        parseBet.ParticipantName = "No";
                                    }
                                    else
                                    {
                                        parseBet.ParticipantName = "Yes";
                                    }

                                    if (m.Groups["period"].Value != "")
                                    {
                                        parseBet.MarketLabel += $" in {m.Groups["period"].Value.ToString().Trim()} Half";
                                    }
                                }
                                break;
                            }

                            //21-2 1st set
                            //Tennis
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d)1-2(?<set>( 1st set)?)(?<ot>( OT)?)(?<qualify>( - qualify)?)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "tennis")
                                {
                                    if (m.Groups["set"].Value != "")
                                    {
                                        parseBet.MarketLabel = "First Set Winner";
                                        parseBet.TabLabel = "Set";
                                    }
                                    else
                                    {
                                        parseBet.MarketLabel = "To Win Match";
                                        parseBet.TabLabel = "Main";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ParticipantName = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ParticipantName = "*away*";
                                    }
                                }
                                else if (info.sport == "table tennis")
                                {
                                    if (m.Groups["set"].Value != "")
                                    {
                                    }
                                    else
                                    {
                                        parseBet.MarketLabel = "Match Lines";
                                        parseBet.TabLabel = "Main";
                                        parseBet.RowHeader = "To Win";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }
                                }
                                else if (info.sport == "basketball")
                                {
                                    if (m.Groups["ot"].Value != "")
                                    {
                                        parseBet.MarketLabel = "Game Lines";
                                        parseBet.TabLabel = "Main Markets";
                                        parseBet.RowHeader = "Money Line";
                                    }
                                    else
                                    {
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                }
                                else if (info.sport == "volleyball")
                                {
                                    if (m.Groups["ot"].Value != "")
                                    {
                                    }
                                    else
                                    {
                                        parseBet.MarketLabel = "Game Lines";
                                        parseBet.TabLabel = "Main";
                                        parseBet.RowHeader = "To Win";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                }
                                else if (info.sport == "hockey")
                                {
                                    if (m.Groups["ot"].Value != "")
                                    {//OT+SO

                                        parseBet.MarketLabel = "Game Lines";
                                        parseBet.TabLabel = "Game Betting";
                                        parseBet.RowHeader = "Money Line";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                }
                                else if (info.sport == "soccer")
                                {
                                    if (m.Groups["qualify"].Value != "")
                                    {
                                        parseBet.TabLabel = "Popular";
                                        parseBet.MarketLabel = "To Qualify";
                                        if (m.Groups["team"].Value == "1")
                                        {
                                            parseBet.ParticipantName = "*home*";
                                        }
                                        else if (m.Groups["team"].Value == "2")
                                        {
                                            parseBet.ParticipantName = "*away*";
                                        }
                                    }
                                }
                                break;
                            }

                            //W1
                            //Basketball
                            mc = Regex.Matches(info.outcome, "^W(?<team>\\d)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "basketball")
                                {
                                    parseBet.TabLabel = "Main Markets";

                                    parseBet.MarketLabel = "Game Lines";
                                    parseBet.RowHeader = "Money Line";

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }
                                }
                                break;
                            }

                            //AH2(0)
                            //Soccer
                            mc = Regex.Matches(info.outcome, "^AH(?<team>\\d)\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)(?<iscorner>( - corners)?)(?<ot>( OT)?)(?<period>(( \\w+ period)|( \\w+ half)|( \\w+ set))?)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];

                                if (parseBet.Sport == "soccer")
                                {
                                    parseBet.TabLabel = "Asian Lines";


                                    if (m.Groups["iscorner"].Value != "")
                                    {
                                        if (m.Groups["period"].Value != "")
                                        {
                                            parseBet.MarketLabel = "1st Half Asian Handicap Corners";
                                        }
                                        else
                                        {
                                            parseBet.MarketLabel = "Asian Handicap Corners";
                                        }
                                    }
                                    else
                                    {
                                        if (m.Groups["period"].Value != "")
                                        {
                                            parseBet.MarketLabel = "1st Half Asian Handicap";
                                        }
                                        else
                                        {
                                            parseBet.MarketLabel = "Asian Handicap";
                                        }
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;
                                }
                                else if (parseBet.Sport == "basketball")
                                {
                                    if (m.Groups["ot"].Value != "")
                                    {
                                        parseBet.TabLabel = "Main Markets";
                                        parseBet.MarketLabel = "Game Lines";
                                        parseBet.RowHeader = "Spread";
                                    }
                                    else if (m.Groups["period"].Value == " 1st half")
                                    {
                                        parseBet.TabLabel = "Main Markets";
                                        parseBet.MarketLabel = "1st Half";
                                        parseBet.RowHeader = "Spread";
                                    }
                                    else if (m.Groups["period"].Value == " 1st period")
                                    {
                                        parseBet.TabLabel = "Main Markets";
                                        parseBet.MarketLabel = "1st Quarter";
                                        parseBet.RowHeader = "Spread";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;

                                }
                                else if (parseBet.Sport == "hockey")
                                {
                                    parseBet.TabLabel = "Main";

                                    parseBet.MarketLabel = "Asian Handicap";
                                    if (m.Groups["iscorner"].Value != "")
                                    {
                                        parseBet.MarketLabel = " Corners";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;

                                }
                                else if (parseBet.Sport == "volleyball")
                                {
                                    parseBet.TabLabel = "Main";
                                    parseBet.MarketLabel = "Game Lines";
                                    parseBet.RowHeader = "Handicap - Sets";


                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;

                                }
                                else if (parseBet.Sport == "handball")
                                {
                                    parseBet.TabLabel = "Main Markets";
                                    parseBet.MarketLabel = "Game Lines";
                                    parseBet.RowHeader = "Handicap";


                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;

                                }
                                else if (parseBet.Sport == "tennis")
                                {

                                    if (m.Groups["period"].Value != "")
                                    {
                                        parseBet.TabLabel = "Set";
                                        parseBet.MarketLabel = "First Set Handicap";
                                    }
                                    else
                                    {
                                        parseBet.TabLabel = "Main";
                                        parseBet.MarketLabel = "Match Handicap (Games)";
                                    }



                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }


                                    parseBet.ParticipantName = m.Groups["handicap"].Value;

                                }
                                break;
                            }

                            //H2(+2.5) - corners
                            //Soccer
                            mc = Regex.Matches(info.outcome, "^H(?<team>\\d)\\((?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))\\)(?<iscorner>( - corners)?)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "soccer")
                                {
                                    parseBet.TabLabel = "Asian Lines";

                                    parseBet.MarketLabel = "Asian Handicap";
                                    if (m.Groups["iscorner"].Value != "")
                                    {
                                        parseBet.MarketLabel = " Corners";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                    parseBet.ParticipantName = m.Groups["handicap"].Value;
                                }
                                break;
                            }

                            //1(1:0) - cards
                            //multiSports
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d)\\((?<handicap1>(\\d+))\\:(?<handicap2>(\\d+))\\)(?<option>(( - corners)|( - cards))?)(?<period>(( \\w+ period)|( \\w+ half))?)");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];

                                    if (info.sport == "soccer")
                                    {
                                        if (m.Groups["option"].Value == " - cards")
                                        {
                                            parseBet.MarketLabel = "Card Handicap";
                                            parseBet.TabLabel = "Cards";
                                        }
                                        else if (m.Groups["option"].Value == " - corners")
                                        {
                                            parseBet.MarketLabel = "Corner Handicap";
                                            parseBet.TabLabel = "Corners";
                                        }
                                        else if (m.Groups["option"].Value == "")
                                        {
                                            if (m.Groups["period"].Value != "")
                                            {
                                                parseBet.MarketLabel = "1st Half Handicap";
                                                parseBet.TabLabel = "Half";
                                            }
                                            else
                                            {
                                                parseBet.MarketLabel = "Handicap Result";
                                                parseBet.TabLabel = "Popular";
                                            }
                                        }
                                        int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                        int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                        if (m.Groups["team"].Value == "1")
                                        {
                                            parseBet.ColHeader = "*home*";

                                            parseBet.ParticipantName = (handicap1 - handicap2).ToString();
                                        }
                                        else if (m.Groups["team"].Value == "2")
                                        {
                                            parseBet.ColHeader = "*away*";

                                            parseBet.ParticipantName = (handicap2 - handicap1).ToString();
                                        }

                                    }
                                    else if (info.sport == "basketball")
                                    {
                                        if (m.Groups["option"].Value == "")
                                        {
                                            if ((m.Groups["period"].Value == ""))
                                            {
                                                parseBet.MarketLabel = "Game Lines 3-Way";
                                                parseBet.TabLabel = "Main Markets";
                                                parseBet.RowHeader = "Spread";
                                            }
                                            else if (m.Groups["period"].Value == " 1st half")
                                            {
                                                parseBet.MarketLabel = "1st Half Spread 3-Way";
                                                parseBet.TabLabel = "Half Props";
                                            }
                                            int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                            int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                            if (m.Groups["team"].Value == "1")
                                            {
                                                parseBet.ColHeader = "*home*";

                                                parseBet.ParticipantName = (handicap1 - handicap2).ToString();
                                            }
                                            else if (m.Groups["team"].Value == "2")
                                            {
                                                parseBet.ColHeader = "*away*";

                                                parseBet.ParticipantName = (handicap2 - handicap1).ToString();
                                            }
                                        }
                                    }
                                    else if (info.sport == "handball")
                                    {//**
                                        if (m.Groups["option"].Value == "")
                                        {
                                            parseBet.MarketLabel = "Game Lines 3-Way";
                                            parseBet.TabLabel = "Main Markets";
                                            parseBet.RowHeader = "Handicap";
                                        }

                                        int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                        int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                        if (m.Groups["team"].Value == "1")
                                        {
                                            parseBet.ColHeader = "*home*";

                                            parseBet.ParticipantName = (handicap1 - handicap2).ToString();
                                        }
                                        else if (m.Groups["team"].Value == "2")
                                        {
                                            parseBet.ColHeader = "*away*";

                                            parseBet.ParticipantName = (handicap2 - handicap1).ToString();
                                        }
                                    }
                                    else if (info.sport == "hockey")
                                    {//**
                                        if (m.Groups["option"].Value == "")
                                        {
                                            parseBet.MarketLabel = "3-Way";
                                            parseBet.TabLabel = "Game Betting";
                                            parseBet.RowHeader = "Line";
                                        }

                                        int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                        int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                        if (m.Groups["team"].Value == "1")
                                        {
                                            parseBet.ColHeader = "*home*";

                                            parseBet.ParticipantName = (handicap1 - handicap2).ToString();
                                        }
                                        else if (m.Groups["team"].Value == "2")
                                        {
                                            parseBet.ColHeader = "*away*";

                                            parseBet.ParticipantName = (handicap2 - handicap1).ToString();
                                        }
                                    }
                                }
                                catch { }
                                break;
                            }

                            //3:0
                            //tennis
                            mc = Regex.Matches(info.outcome, "^(?<handicap1>^(\\d+)):(?<handicap2>(\\d+))(?<period>( 1st period)?)");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];
                                    if (info.sport == "tennis")
                                    {
                                        parseBet.TabLabel = "Set";
                                        parseBet.MarketLabel = "Set Betting";

                                        int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                        int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                        if ((handicap1 == 0 && handicap2 == 0) || (handicap1 != 0 && handicap2 != 0))
                                            break;

                                        if (handicap1 == 0)
                                        {
                                            parseBet.ColHeader = "*away*";
                                            parseBet.RowHeader = $"{handicap2}-{handicap1}";
                                        }
                                        else if (handicap2 == 0)
                                        {
                                            parseBet.ColHeader = "*home*";
                                            parseBet.RowHeader = $"{handicap1}-{handicap2}";
                                        }
                                    }
                                    else if (info.sport == "soccer")
                                    {
                                        if (m.Groups["period"].Value == " 1st period")
                                        {
                                            parseBet.TabLabel = "Half";
                                            parseBet.MarketLabel = "Half Time Correct Score";

                                            int handicap1 = int.Parse(m.Groups["handicap1"].Value.ToString());
                                            int handicap2 = int.Parse(m.Groups["handicap2"].Value.ToString());

                                            parseBet.ParticipantName = $"{handicap1}-{handicap2}";
                                            // only 0:0 exists..
                                            if (handicap1 > handicap2)
                                            {
                                                parseBet.ColHeader = "*away*";
                                            }
                                            else if (handicap1 < handicap2)
                                            {
                                                parseBet.ColHeader = "*home*";
                                            }
                                            else
                                            {
                                                parseBet.ColHeader = "Draw";
                                            }
                                        }
                                    }
                                }
                                catch { }
                                break;
                            }

                            //1 / 1 half / match
                            //Soccer
                            mc = Regex.Matches(info.outcome, "^(?<team1>\\d) / (?<team2>\\d) half / match");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                parseBet.TabLabel = "Popular";
                                parseBet.MarketLabel = "Half Time/Full Time";
                                parseBet.ParticipantName = $"{m.Groups["team1"].Value} - {m.Groups["team2"].Value}";

                                parseBet.ParticipantName = parseBet.ParticipantName.Replace("1", "*home*");
                                parseBet.ParticipantName = parseBet.ParticipantName.Replace("2", "*away*");


                                break;
                            }

                            //1 to win to nil
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) to win to nil");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                parseBet.TabLabel = "Specials";
                                parseBet.MarketLabel = "Specials";
                                parseBet.RowHeader = "To Win to Nil";
                                parseBet.ColHeader = m.Groups["team"].Value;
                                break;
                            }

                            //1 - win
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) - win");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];

                                secondResult = new ParseBet_Bet365(parseBet);

                                parseBet.TabLabel = "Popular";
                                secondResult.TabLabel = "Popular";

                                parseBet.MarketLabel = "Method of Victory";
                                secondResult.MarketLabel = "Method Of Qualification";

                                parseBet.RowHeader = "90 Mins";
                                secondResult.RowHeader = "90 Mins";

                                if (m.Groups["team"].Value == "1")
                                {
                                    parseBet.ColHeader = "*home*";
                                    secondResult.ColHeader = "*home*";
                                }
                                else if (m.Groups["team"].Value == "2")
                                {
                                    parseBet.ColHeader = "*away*";
                                    secondResult.ColHeader = "*away*";
                                }


                                break;
                            }

                            //2 1st half 
                            //basketball
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) 1st half$");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                parseBet.TabLabel = "Half Props";
                                parseBet.MarketLabel = "1st Half Money Line 3-Way";

                                if (m.Groups["team"].Value == "1")
                                {
                                    parseBet.ParticipantName = "*home*";
                                }
                                else if (m.Groups["team"].Value == "2")
                                {
                                    parseBet.ParticipantName = "*away*";
                                }
                                break;
                            }

                            //1 / DNB 1st corner
                            //Soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) / DNB(?<is1stcorner>(( 1st corner)|( 1st half))?)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];

                                if (info.sport == "soccer")
                                {
                                    if (m.Groups["is1stcorner"].Value != "")
                                    {
                                        parseBet.MarketLabel = "First Match Corner";
                                        parseBet.TabLabel = "Corner";
                                    }
                                    else
                                    {
                                        parseBet.MarketLabel = "Draw No Bet";
                                        parseBet.TabLabel = "Popular";
                                    }

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ParticipantName = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ParticipantName = "*away*";
                                    }
                                }
                                else if (info.sport == "hockey")
                                {
                                    parseBet.MarketLabel = "Draw No Bet";
                                    parseBet.TabLabel = "Main";

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ParticipantName = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ParticipantName = "*away*";
                                    }
                                }
                                else if (info.sport == "handball")
                                {
                                    parseBet.MarketLabel = "Draw No Bet";
                                    parseBet.TabLabel = "Main";

                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ParticipantName = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ParticipantName = "*away*";
                                    }
                                }
                                else if (info.sport == "basketball")
                                {
                                    //Originally it's Draw No Bet, but in this parsing, it is not containing draw.
                                    parseBet.TabLabel = "Half Props";
                                    parseBet.MarketLabel = "1st Half Money Line 3-Way";


                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.ParticipantName = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.ParticipantName = "*away*";
                                    }

                                }
                                break;
                            }

                            //1(≥21) OT
                            //handball
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d)\\(≥(?<handicap>(\\d+))\\)(?<period>(( OT)|( 1st half)|( 1st period))?)");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];
                                    if (info.sport == "basketball")
                                    {
                                        if (m.Groups["period"].Value == " 1st period")
                                        {
                                            parseBet.MarketLabel = "1st Quarter Margin Of Victory";
                                            parseBet.TabLabel = "Quarter Props";
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value} or more";

                                            if (m.Groups["team"].Value == "1")
                                            {
                                                parseBet.ColHeader = "*home*";
                                            }
                                            else if (m.Groups["team"].Value == "2")
                                            {
                                                parseBet.ColHeader = "*away*";
                                            }
                                        }
                                        if (m.Groups["period"].Value == " 1st half")
                                        {
                                            parseBet.MarketLabel = "1st Half Winning Margin";
                                            parseBet.TabLabel = "Half Props";
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value}+";

                                            if (m.Groups["team"].Value == "1")
                                            {
                                                parseBet.ColHeader = "*home*";
                                            }
                                            else if (m.Groups["team"].Value == "2")
                                            {
                                                parseBet.ColHeader = "*away*";
                                            }
                                        }
                                        else if (m.Groups["period"].Value == " OT")
                                        {
                                            parseBet.MarketLabel = "Winning Margin";
                                            parseBet.TabLabel = "Main Props";
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value}+";

                                            if (m.Groups["team"].Value == "1")
                                            {
                                                parseBet.ColHeader = "*home*";
                                            }
                                            else if (m.Groups["team"].Value == "2")
                                            {
                                                parseBet.ColHeader = "*away*";
                                            }
                                        }
                                    }
                                    else if (info.sport == "handball")
                                    {
                                        if (m.Groups["period"].Value == "")
                                        {
                                            parseBet.MarketLabel = "Winning Margin";
                                            parseBet.TabLabel = "Main Props";
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value}+";

                                            if (m.Groups["team"].Value == "1")
                                            {
                                                parseBet.ColHeader = "*home*";
                                            }
                                            else if (m.Groups["team"].Value == "2")
                                            {
                                                parseBet.ColHeader = "*away*";
                                            }
                                        }
                                    }
                                }
                                catch { }
                                break;
                            }

                            //Total ≥40 1st half 1st team - No
                            //basketball
                            mc = Regex.Matches(info.outcome, "^Total ≥(?<handicap>(\\d+))(?<period>(( \\w+ period)|( \\w+ half))?)(?<option>(( - corners)|( - cards))?)(?<team>(( 1st team)|( 2nd team)|( both teams))?)(?<no>( - No)?)");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];
                                    if (info.sport == "basketball")
                                    {
                                        if (m.Groups["period"].Value == " 1st period")
                                        {
                                            if (m.Groups["team"].Value == " both teams")
                                            {
                                                parseBet.MarketLabel = "1st Quarter Both Teams to Score X Points";
                                                parseBet.TabLabel = "Quarter Props";

                                                if (m.Groups["no"].Value != "")
                                                {
                                                    parseBet.ColHeader = $"No";
                                                }
                                                else
                                                {
                                                    parseBet.ColHeader = $"Yes";
                                                }
                                                parseBet.RowHeader = $"{m.Groups["handicap"].Value}";
                                            }
                                        }
                                        else if (m.Groups["period"].Value == " 1st half")
                                        {
                                            parseBet.MarketLabel = "1st Half Team to Score X Points";
                                            parseBet.TabLabel = "Half Props";
                                            if (m.Groups["team"].Value == " 1st team")
                                            {
                                                parseBet.TableHeader = "*home*";
                                            }
                                            else if (m.Groups["team"].Value == " 2nd team")
                                            {
                                                parseBet.TableHeader = "*away*";
                                            }

                                            if (m.Groups["no"].Value != "")
                                            {
                                                parseBet.ColHeader = $"No";
                                            }
                                            else
                                            {
                                                parseBet.ColHeader = $"Yes";
                                            }
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value}";
                                        }

                                    }
                                    else if (info.sport == "soccer")
                                    {
                                        if (m.Groups["period"].Value == "")
                                        {
                                            if (m.Groups["option"].Value == " - cards" && m.Groups["team"].Value == " both teams")
                                            {
                                                parseBet.MarketLabel = $"Both Teams to Receive {m.Groups["handicap"].Value}+ Cards";
                                                parseBet.TabLabel = "Cards";


                                                if (m.Groups["no"].Value != "")
                                                {
                                                    parseBet.ParticipantName = $"No";
                                                }
                                                else
                                                {
                                                    parseBet.ParticipantName = $"Yes";
                                                }
                                            }
                                        }

                                    }
                                }
                                catch { }
                                break;
                            }

                            //Goals: Yes 1st period 1st team
                            //soccer
                            mc = Regex.Matches(info.outcome, "^Goals: (?<yes>(\\w+)) (?<period>\\w+) period (?<team>\\w+) team");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];
                                    if (info.sport == "soccer")
                                    {
                                        parseBet.MarketLabel = "To Score in Half";
                                        parseBet.TabLabel = "Half";
                                        parseBet.RowHeader = $"{m.Groups["period"].Value} Half";
                                        parseBet.ColHeader = $"{m.Groups["yes"].Value}";

                                        if (m.Groups["team"].Value == "1st")
                                        {
                                            parseBet.TableHeader = "*home*";    //should be checked in table name
                                        }
                                        else if (m.Groups["team"].Value == "2nd")
                                        {
                                            parseBet.TableHeader = "*away*";
                                        }
                                    }
                                }
                                catch { }
                                break;
                            }

                            //Under 5 - corners 1st period
                            mc = Regex.Matches(info.outcome, "^(?<side>(Over|Under)) (?<handicap>((-?|\\+?)\\d*\\.{0,1}\\d+))(?<iscorner>( - corners)?)(?<ot>(( OT)|( OT+SO))?)(?<period>(( \\w+ period)|( \\w+ half))?)(?<team>( \\w+ team)?)");
                            if (mc.Count == 1)
                            {
                                try
                                {
                                    Match m = mc[0];

                                    if (info.sport == "basketball")
                                    {
                                        if (m.Groups["iscorner"].Value == "")
                                        {
                                            if (m.Groups["ot"].Value != "")
                                            {
                                                if (m.Groups["team"].Value != "")
                                                {
                                                    parseBet.TabLabel = "Team Props";
                                                    parseBet.MarketLabel = "Team Totals";

                                                    if (m.Groups["team"].Value == " 1st team")
                                                    {
                                                        parseBet.ColHeader = "*home*";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ColHeader = "*away*";
                                                    }

                                                    if (m.Groups["side"].Value == "Over")
                                                    {
                                                        parseBet.ParticipantName = $"Over {m.Groups["handicap"].Value}";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ParticipantName = $"Under {m.Groups["handicap"].Value}";
                                                    }
                                                }
                                                else
                                                {
                                                    parseBet.TabLabel = "Main Markets";
                                                    parseBet.MarketLabel = "Game Lines";
                                                    parseBet.RowHeader = "Total";

                                                    if (m.Groups["side"].Value == "Over")
                                                    {
                                                        parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*home*";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*away*";
                                                    }
                                                }
                                            }
                                            else if (m.Groups["period"].Value == " 1st period")
                                            {
                                                parseBet.TabLabel = "Main Markets";
                                                parseBet.MarketLabel = "1st Quarter";
                                                parseBet.RowHeader = "Total";


                                                if (m.Groups["side"].Value == "Over")
                                                {
                                                    parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*home*";
                                                }
                                                else
                                                {
                                                    parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*away*";
                                                }
                                            }
                                            else if (m.Groups["period"].Value == " 1st half")
                                            {
                                                if (m.Groups["team"].Value != "")
                                                {
                                                    parseBet.TabLabel = "Half Props";
                                                    parseBet.MarketLabel = "1st Half Team Totals";
                                                    if (m.Groups["team"].Value == " 1st team")
                                                    {
                                                        parseBet.ParticipantName = $"{m.Groups["side"].Value} {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*home*";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ParticipantName = $"{m.Groups["side"].Value} {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*away*";
                                                    }
                                                }
                                                else
                                                {
                                                    parseBet.TabLabel = "Main Markets";
                                                    parseBet.MarketLabel = "1st Half";
                                                    parseBet.RowHeader = "Total";

                                                    if (m.Groups["side"].Value == "Over")
                                                    {
                                                        parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*home*";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                        parseBet.ColHeader = "*away*";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (info.sport == "rugby")
                                    {
                                        if (m.Groups["period"].Value != "")
                                        {
                                            parseBet.TabLabel = "Game Betting";
                                            parseBet.MarketLabel = "1st Half Betting 3-Way";
                                            parseBet.RowHeader = "Total";

                                            if (m.Groups["side"].Value == "Over")
                                            {
                                                parseBet.ColHeader = $"*home*";
                                                parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                            }
                                            else
                                            {
                                                parseBet.ColHeader = $"*away*";
                                                parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                            }

                                        }
                                        else
                                        {
                                            parseBet.TabLabel = "Game Betting";
                                            parseBet.MarketLabel = "Game Betting 3-Way";
                                            parseBet.RowHeader = "Total";

                                            if (m.Groups["side"].Value == "Over")
                                            {
                                                parseBet.ColHeader = $"*home*";
                                                parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                            }
                                            else
                                            {
                                                parseBet.ColHeader = $"*away*";
                                                parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                            }
                                        }
                                    }
                                    else if (info.sport == "soccer")
                                    {
                                        if (m.Groups["iscorner"].Value == "")
                                        {
                                            if (m.Groups["team"].Value == "")
                                            {
                                                if (m.Groups["period"].Value != "")
                                                {
                                                    parseBet.TabLabel = "Half";
                                                    parseBet.MarketLabel = "1st Half Goal Line";
                                                    parseBet.RowHeader = m.Groups["handicap"].Value;
                                                    parseBet.ColHeader = m.Groups["side"].Value;
                                                }
                                                else
                                                {
                                                    parseBet.TabLabel = "Popular";
                                                    parseBet.MarketLabel = "Goal Line";
                                                    parseBet.RowHeader = m.Groups["handicap"].Value;
                                                    parseBet.ColHeader = m.Groups["side"].Value;

                                                    secondResult = new ParseBet_Bet365(parseBet);
                                                    secondResult.TabLabel = "Popular";
                                                    secondResult.MarketLabel = "Goals Over/Under";
                                                    secondResult.RowHeader = m.Groups["handicap"].Value;
                                                    secondResult.ColHeader = m.Groups["side"].Value;
                                                }
                                            }
                                            else
                                            {
                                                if (m.Groups["period"].Value == "")
                                                {
                                                    parseBet.TabLabel = "Goals";
                                                    parseBet.MarketLabel = "Team Total Goals";
                                                    if (m.Groups["team"].Value == " 1st team")
                                                    {
                                                        parseBet.ColHeader = "*home*";
                                                    }
                                                    else
                                                    {
                                                        parseBet.ColHeader = "*away*";
                                                    }

                                                    parseBet.ParticipantName = $"{m.Groups["side"].Value} {m.Groups["handicap"].Value}";

                                                }
                                            }
                                        }
                                        else if (m.Groups["iscorner"].Value == " - corners")
                                        {
                                            if (m.Groups["period"].Value == "")
                                            {
                                                parseBet.TabLabel = "Asian Lines";
                                                parseBet.MarketLabel = "Asian Total Corners";
                                                parseBet.RowHeader = m.Groups["handicap"].Value;
                                                parseBet.ColHeader = m.Groups["side"].Value;
                                            }
                                            else if (m.Groups["period"].Value == " 1st period")
                                            {
                                                parseBet.TabLabel = "Asian Lines";
                                                parseBet.MarketLabel = "1st Half Asian Corners";
                                                parseBet.RowHeader = m.Groups["handicap"].Value;
                                                parseBet.ColHeader = m.Groups["side"].Value;
                                            }
                                        }
                                    }
                                    else if (info.sport == "hockey")
                                    {
                                        if (m.Groups["iscorner"].Value == "")
                                        {
                                            if (m.Groups["period"].Value != "")
                                            {
                                                parseBet.TabLabel = "1st Period";
                                                parseBet.MarketLabel = "1st Period Asian Goal Line";
                                                parseBet.RowHeader = m.Groups["handicap"].Value;
                                                parseBet.ColHeader = m.Groups["side"].Value;
                                            }
                                            else
                                            {
                                                //if (m.Groups["ot"].Value != "")  same as with no OT
                                                //{
                                                //    parseBet.TabLabel = "Game Betting";
                                                //    parseBet.MarketLabel = "3-Way";
                                                //    parseBet.RowHeader = "Total";

                                                //    if (m.Groups["side"].Value == "Over")
                                                //    {
                                                //        parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                //        parseBet.ColHeader = "*home*";
                                                //    }
                                                //    else
                                                //    {
                                                //        parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                //        parseBet.ColHeader = "*away*";
                                                //    }
                                                //}
                                                //else
                                                //{
                                                parseBet.TabLabel = "Game Betting";
                                                parseBet.MarketLabel = "Game Lines";
                                                parseBet.RowHeader = "Total";

                                                if (m.Groups["side"].Value == "Over")
                                                {
                                                    parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*home*";
                                                }
                                                else
                                                {
                                                    parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*away*";
                                                }

                                                secondResult = new ParseBet_Bet365(parseBet);
                                                secondResult.TabLabel = "Game Betting";
                                                secondResult.MarketLabel = "3-Way";
                                                secondResult.RowHeader = "Total";

                                                if (m.Groups["side"].Value == "Over")
                                                {
                                                    secondResult.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                    secondResult.ColHeader = "*home*";
                                                }
                                                else
                                                {
                                                    secondResult.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                    secondResult.ColHeader = "*away*";
                                                }
                                                //}
                                            }
                                        }
                                    }
                                    else if (info.sport == "handball")
                                    {
                                        if (m.Groups["iscorner"].Value == "")
                                        {
                                            if (m.Groups["period"].Value == "")
                                            {
                                                parseBet.TabLabel = "Main Markets";
                                                parseBet.MarketLabel = "Game Lines";
                                                parseBet.RowHeader = "Total";

                                                if (m.Groups["side"].Value == "Over")
                                                {
                                                    parseBet.ParticipantName = $"O {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*home*";
                                                }
                                                else
                                                {
                                                    parseBet.ParticipantName = $"U {m.Groups["handicap"].Value}";
                                                    parseBet.ColHeader = "*away*";
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (m.Groups["iscorner"].Value == "")
                                        {
                                            //parseBet.TabLabel = "Goals";
                                            //parseBet.MarketLabel = "Goals Over/Under";
                                            parseBet.TabLabel = "Asian Lines";  //soccer
                                            parseBet.MarketLabel = "Goal Line";
                                            parseBet.RowHeader = $"{m.Groups["handicap"].Value}";
                                            parseBet.ColHeader = $"{m.Groups["side"].Value}";
                                        }
                                        else
                                        {
                                            if (m.Groups["period"].Value == "" && m.Groups["team"].Value == "")
                                            {
                                                parseBet.TabLabel = "Corners";
                                                parseBet.MarketLabel = "Corners";   //Asian Total Corners   (sometimes it changed as Asian Total Corners)
                                                parseBet.RowHeader = $"{m.Groups["handicap"].Value}";
                                                parseBet.ColHeader = $"{m.Groups["side"].Value}";
                                            }
                                            else if (m.Groups["period"].Value != "" && m.Groups["team"].Value == "")
                                            {
                                                parseBet.TabLabel = "Corners";
                                                parseBet.MarketLabel = "First Half Corners";
                                                parseBet.RowHeader = $"{m.Groups["handicap"].Value}";
                                                parseBet.ColHeader = $"{m.Groups["side"].Value}";
                                            }
                                            else if (m.Groups["period"].Value == "" && m.Groups["team"].Value != "")
                                            {
                                                parseBet.TabLabel = "Corners";
                                                parseBet.MarketLabel = "Team Corners";
                                                parseBet.ParticipantName = $"{m.Groups["side"].Value} {m.Groups["handicap"].Value}";
                                                parseBet.ColHeader = $"{m.Groups["team"].Value}";
                                            }
                                        }
                                    }
                                }
                                catch { }
                                break;
                            }

                            //1 - corners
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) - corners");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];

                                parseBet.MarketLabel = "Corner Match Bet";
                                parseBet.TabLabel = "Corners";

                                if (m.Groups["team"].Value == "1")
                                {
                                    parseBet.ParticipantName = "*home*";
                                }
                                else if (m.Groups["team"].Value == "2")
                                {
                                    parseBet.ParticipantName = "*away*";
                                }

                                break;
                            }

                            //2nd to keep clean sheet
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\w+) to keep[ a]* clean sheet(?<no>( - No)?)");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "soccer")
                                {

                                    parseBet.MarketLabel = "Clean Sheet";
                                    parseBet.TabLabel = "Goals";
                                    if (m.Groups["no"].Value == "")
                                    {
                                        parseBet.ParticipantName = "Yes";
                                    }
                                    else
                                    {
                                        parseBet.ParticipantName = "No";
                                    }
                                    if (m.Groups["team"].Value == "1st")
                                    {
                                        parseBet.ColHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2nd")
                                    {
                                        parseBet.ColHeader = "*away*";
                                    }

                                }
                                break;
                            }

                            //2 and Both to score
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<team>\\d) and Both to score");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "soccer")
                                {
                                    parseBet.MarketLabel = "Result/Both Teams to Score";
                                    parseBet.TabLabel = "Popular";

                                    parseBet.ColHeader = "Yes";
                                    if (m.Groups["team"].Value == "1")
                                    {
                                        parseBet.RowHeader = "*home*";
                                    }
                                    else if (m.Groups["team"].Value == "2")
                                    {
                                        parseBet.RowHeader = "*away*";
                                    }
                                }
                                break;
                            }

                            //Odd (Even)
                            //soccer
                            mc = Regex.Matches(info.outcome, "^(?<odd>((Odd)|(Even))?)$");
                            if (mc.Count == 1)
                            {
                                Match m = mc[0];
                                if (info.sport == "soccer")
                                {
                                    parseBet.MarketLabel = "Goals Odd/Even";
                                    parseBet.TabLabel = "Goals";
                                    parseBet.ParticipantName = m.Groups["odd"].Value;
                                }
                                break;
                            }
                            break;
                        }
                }
            }
            else if (info.kind == PickKind.Type_6)
            {
                string[] splits = info.direct_link.Split('|');
                if (splits.Count() != 5)
                    return null;

                string trade_market = splits[0];
                string trade_period = splits[1];
                string trade_runnerText = splits[2];
                string trade_oddsTypeCondition = splits[3];
                string trade_marketText = splits[4];
                if (info.sport.ToLower() == "american football")
                {
                    if (trade_period == "Whole Match" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";                        
                        parseBet.RowHeader = "Money Line";
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }                         
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";                            
                        }
                        else
                        {
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Main Props";
                        secondResult.MarketLabel = "Alternative Point Spread 2-Way";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "Whole Match" && trade_marketText == "3-Way Handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines 3-Way";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else
                        {
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Main Props";
                        secondResult.MarketLabel = "Alternative Handicap 3-Way";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Total";
                        
                        if (trade_runnerText == "Over")
                        {
                            parseBet.ParticipantName = "O " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ParticipantName = "U " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Main Props";
                        secondResult.MarketLabel = "Alternative Total 2-Way";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Over")
                        {                            
                            secondResult.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Money Line")
                    {
                        //if (marketName == "1st Half")
                        //{
                        //    if (odd_obj["name"].ToString() == "Money Line")
                        //    {
                        //        if ((odd_obj["header"].ToString() == "1" && trade_runnerText == "Home") || (odd_obj["header"].ToString() == "2" && trade_runnerText == "Away"))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_marketText == "3-Way Handicap")
                    {
                        //if (marketName == "1st Half Spread 3-Way")
                        //{
                        //    double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //    double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //    if (hand_val1 == hand_val2)
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Total Goals")
                    {
                        //if (marketName == "1st Half" || marketName == "1st Half Totals")
                        //{
                        //    double hand_val1 = -100;
                        //    string header = string.Empty;
                        //    if (odd_obj["name"].ToString() == "Total")
                        //    {
                        //        string handicap_Str = odd_obj["handicap"].ToString();
                        //        header = handicap_Str.Contains("O") ? "Over" : "Under";
                        //        hand_val1 = Utils.ParseToDouble(handicap_Str.Replace("O", string.Empty).Replace("U", string.Empty).Trim());
                        //    }
                        //    else
                        //    {
                        //        hand_val1 = Utils.getHandicap(odd_obj["name"].ToString());
                        //        header = odd_obj["header"].ToString();
                        //    }
                        //    double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //    if (hand_val1 == hand_val2 && header == trade_runnerText)
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Money Line")
                    {
                        //if (marketName == "1st Quarter Lines 2-Way")
                        //{
                        //    if (odd_obj["name"].ToString() == "Money Line")
                        //    {
                        //        if ((odd_obj["header"].ToString() == "1" && trade_runnerText == "Home") || (odd_obj["header"].ToString() == "2" && trade_runnerText == "Away"))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Total Goals")
                    {
                        //if (marketName == "1st Quarter Lines 2-Way")
                        //{
                        //    double hand_val1 = -100;
                        //    string header = string.Empty;
                        //    if (odd_obj["name"].ToString() == "Total")
                        //    {
                        //        string handicap_Str = odd_obj["handicap"].ToString();
                        //        header = handicap_Str.Contains("O") ? "Over" : "Under";
                        //        hand_val1 = Utils.ParseToDouble(handicap_Str.Replace("O", string.Empty).Replace("U", string.Empty).Trim());
                        //    }
                        //    else
                        //    {
                        //        hand_val1 = Utils.getHandicap(odd_obj["name"].ToString());
                        //        header = odd_obj["header"].ToString();
                        //    }
                        //    double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //    if (hand_val1 == hand_val2 && header == trade_runnerText)
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Asian Handicap")
                    {
                    //    if (marketName == "1st Quarter Lines 2-Way")
                    //    {
                    //        if (odd_obj["name"].ToString() == "Spread")
                    //        {
                    //            double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                    //            double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                    //            if (hand_val1 == hand_val2)
                    //            {
                    //                trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                    //                selectionId = odd_obj["id"].ToString();
                    //                isFound = true;
                    //                break;
                    //            }

                    //        }
                    //    }
                    //    else if (marketName == "1st Quarter Alternative Point Spread")
                    //    {
                    //        double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                    //        double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                    //        if (hand_val1 == hand_val2)
                    //        {
                    //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                    //            selectionId = odd_obj["id"].ToString();
                    //            isFound = true;
                    //            break;
                    //        }
                    //    }
                    }

                }
                else if (info.sport.ToLower() == "basketball")
                {
                    if (trade_period == "Whole Match" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Money Line";
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else
                        {
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Main Props";
                        secondResult.MarketLabel = "Alternative Point Spread";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else
                        {
                            secondResult.ColHeader = "*away*";
                        }                                                
                    }
                    else if (trade_period == "Whole Match" && trade_marketText == "3-Way Handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines 3-Way";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Total";
                        
                        
                        if (trade_runnerText == "Over")
                        {
                            parseBet.ParticipantName = "O " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ParticipantName = "U " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*away*";
                        }

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Main Props";
                        secondResult.MarketLabel = "Alternative Game Total";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Over")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Half";
                        parseBet.RowHeader = "Money Line";
                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_marketText == "3-Way Handicap")
                    {
                        parseBet.TabLabel = "Half Props";
                        parseBet.MarketLabel = "1st Half Spread 3-Way";                        
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_marketText == "Asian handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Half";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else
                        {
                            parseBet.ColHeader = "*away*";
                        }                       

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Half Props";
                        secondResult.MarketLabel = "Alternative 1st Half Point Spread";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Half";
                        parseBet.RowHeader = "Total";
                        
                        if (trade_runnerText == "Over")
                        {
                            parseBet.ParticipantName = "O " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*home*";
                        }
                        else if(trade_runnerText == "Under")
                        {
                            parseBet.ParticipantName = "U " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*away*";
                        }

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Half Props";
                        secondResult.MarketLabel = "Alternative 1st Half Totals";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Over")
                        {
                            secondResult.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Quarter";
                        parseBet.RowHeader = "Total";

                        if (trade_runnerText == "Over")
                        {
                            parseBet.ParticipantName = "O " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ParticipantName = "U " + trade_oddsTypeCondition.ToString();
                            parseBet.ColHeader = "*away*";
                        }

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Half Props";
                        secondResult.MarketLabel = "Alternative 1st Quarter Totals";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Over")
                        {
                            secondResult.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Quarter";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();

                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else
                        {
                            parseBet.ColHeader = "*away*";
                        }

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Quarter Props";
                        secondResult.MarketLabel = "Alternative 1st Quarter Point Spread";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main Markets";
                        parseBet.MarketLabel = "1st Quarter";
                        parseBet.RowHeader = "Money Line";
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Quarter" && trade_market == "3-Way Handicap")
                    {
                        parseBet.TabLabel = "Quarter Props";
                        parseBet.MarketLabel = "1st Quarter 3 Way Lines";
                        parseBet.RowHeader = "Spread";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";

                        }                       
                    }
                }
                else if (info.sport.ToLower() == "tennis")
                {
                    if (trade_period == "Whole Match" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main";
                        parseBet.MarketLabel = "To Win Match";

                        if (trade_runnerText == "Home")
                            parseBet.ParticipantName = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ParticipantName = "*away*";
                    }
                    else if (trade_period == "1st Set" && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Main";
                        parseBet.MarketLabel = "First Set Winner";

                        if (trade_runnerText == "Home")
                            parseBet.ParticipantName = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ParticipantName = "*away*";
                       
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main";
                        parseBet.MarketLabel = "Total Games 2-Way";
                        parseBet.RowHeader = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Over")
                        {
                            parseBet.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "1st Set" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Main";
                        parseBet.MarketLabel = "1st Set Total Games";
                        parseBet.RowHeader = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Over")
                        {
                            parseBet.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "Whole Match" && trade_market == "Total Games")
                    {
                        parseBet.TabLabel = "Main";
                        parseBet.MarketLabel = "Match Handicap (Games)";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                            parseBet.ColHeader = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ColHeader = "*away*";
                    }
                    else if (trade_period == "1st Set" && trade_market == "Total Games")
                    {
                        parseBet.TabLabel = "Set";
                        parseBet.MarketLabel = "First Set Handicap";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        if (trade_runnerText == "Home")
                            parseBet.ColHeader = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ColHeader = "*away*";
                    }                    
                }
                else if (info.sport.ToLower() == "soccer")
                {
                    //핸디캡인 경우 오드아이디 찾기
                    if (trade_period == "Ordinary Time" && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Asian Lines";
                        parseBet.MarketLabel = "Asian Handicap";                        
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();
                        
                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Asian Lines";
                        secondResult.MarketLabel = "Alternative Asian Handicap";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Asian Lines";
                        parseBet.MarketLabel = "1st Half Asian Handicap";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Asian Lines";
                        secondResult.MarketLabel = "Alternative 1st Half Asian Handicap";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                    else if (trade_period == "Ordinary Time" && trade_market == "Total Goals")
                    {//  14/7/fixed
                        parseBet.TabLabel = "Popular";
                        parseBet.MarketLabel = "Goals Over/Under";
                        parseBet.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Over")
                        {
                            parseBet.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ColHeader = "Under";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Goals";
                        secondResult.MarketLabel = "Alternative Total Goals";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Over")
                        {
                            secondResult.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "1st Half (Ordinary Time)" && trade_market == "Total Goals")
                    {
                        parseBet.TabLabel = "Half";
                        parseBet.MarketLabel = "1st Half Goal Line";
                        parseBet.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Over")
                        {
                            parseBet.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            parseBet.ColHeader = "Under";
                        }


                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Goals";
                        secondResult.MarketLabel = "First Half Goals";
                        secondResult.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Over")
                        {
                            secondResult.ColHeader = "Over";
                        }
                        else if (trade_runnerText == "Under")
                        {
                            secondResult.ColHeader = "Under";
                        }
                    }
                    else if (trade_period == "Ordinary Time" && trade_market == "Full Time Result")
                    {
                        parseBet.TabLabel = "Popular";
                        parseBet.MarketLabel = "Full Time Result";
                        if (trade_runnerText == "Home")
                            parseBet.ParticipantName = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ParticipantName = "*away*";
                        else if (trade_runnerText == "Draw")
                            parseBet.ParticipantName = "Draw";
                    }                
                    else if (trade_period == "Ordinary Time" && trade_market == "Draw No Bet")
                    {
                        parseBet.TabLabel = "Popular";
                        parseBet.MarketLabel = "Draw No Bet";
                        if (trade_runnerText == "Home")
                            parseBet.ParticipantName = "*home*";
                        else if (trade_runnerText == "Away")
                            parseBet.ParticipantName = "*away*";
                    }
                    else if (trade_period == "Ordinary Time" && trade_market == "3-Way Handicap")
                    {
                        parseBet.TabLabel = "Popular";
                        parseBet.MarketLabel = "Handicap Result";
                        parseBet.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }

                        secondResult = new ParseBet_Bet365(parseBet);
                        secondResult.TabLabel = "Popular";
                        secondResult.MarketLabel = "Alternative Handicap Result";
                        secondResult.ParticipantName = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            secondResult.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            secondResult.ColHeader = "*away*";
                        }
                    }
                }
                else if (info.sport.ToLower() == "ice hockey")
                {
                    if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "Money Line")
                    {
                        parseBet.TabLabel = "Game Betting";
                        parseBet.MarketLabel = "Game Lines";
                        parseBet.RowHeader = "Money Line";

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "Full Time Result")
                    {
                        parseBet.TabLabel = "Game Betting";
                        parseBet.MarketLabel = "3-Way";
                        parseBet.RowHeader = "Money Line";

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "Draw No Bet")
                    { 
                        parseBet.TabLabel = "Game Betting";
                        parseBet.MarketLabel = "Draw No Bet";

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }
                    }
                    else if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "Asian Handicap")
                    {
                        parseBet.TabLabel = "Game Betting";
                        parseBet.MarketLabel = "Asian Handicap";
                        parseBet.RowHeader = trade_oddsTypeCondition.ToString();

                        if (trade_runnerText == "Home")
                        {
                            parseBet.ColHeader = "*home*";
                        }
                        else if (trade_runnerText == "Away")
                        {
                            parseBet.ColHeader = "*away*";
                        }

                    }
                    else if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "3-Way Handicap")
                    {
                        //if (marketName == "3-Way")
                        //{
                        //    if (odd_obj["name"].ToString() == "Spread")
                        //    {
                        //        double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //        double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //        string header = odd_obj["header"].ToString();
                        //        if (leagueName == "NBA" || leagueName == "NCAAB" || leagueName == "NCAAF" || leagueName == "NFL" || leagueName == "CFL" || leagueName == "NCAAB Extra Games" || leagueName == "NHL")
                        //        {
                        //            if (header == "1")
                        //                header = "2";
                        //            else if (header == "2")
                        //                header = "1";
                        //        }
                        //        if (hand_val1 == hand_val2 && ((trade_runnerText == "Home" && header == "1") || (trade_runnerText == "Away" && header == "2")))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                        //else if (marketName == "Alternative Puck Line 3-Way")
                        //{
                        //    double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //    double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //    string header = odd_obj["header"].ToString();
                        //    if (leagueName == "NBA" || leagueName == "NCAAB" || leagueName == "NCAAF" || leagueName == "NFL" || leagueName == "CFL" || leagueName == "NCAAB Extra Games" || leagueName == "NHL")
                        //    {
                        //        if (header == "1")
                        //            header = "2";
                        //        else if (header == "2")
                        //            header = "1";
                        //    }
                        //    if (hand_val1 == hand_val2 && ((trade_runnerText == "Home" && header == "1") || (trade_runnerText == "Away" && header == "2")))
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }
                        //}
                    }
                    else if ((trade_period == "Ordinary Time" || trade_period == "Whole Match") && trade_market == "Total Goals")
                    {
                        //if (marketName == "Game Lines")
                        //{
                        //    if (odd_obj["name"].ToString() == "Total")
                        //    {
                        //        string handicap_Str = odd_obj["handicap"].ToString();
                        //        string header = handicap_Str.Contains("O") ? "Over" : "Under";
                        //        double hand_val = Utils.ParseToDouble(handicap_Str.Replace("O", string.Empty).Replace("U", string.Empty).Trim());

                        //        if (header == trade_runnerText && hand_val == trade_oddsTypeCondition)
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                        //else if (marketName.Contains("Alternative Total 2-Way"))
                        //{
                        //    string header = odd_obj["header"].ToString();
                        //    double hand_val = Utils.ParseToDouble(odd_obj["name"].ToString());
                        //    if (header == trade_runnerText && hand_val == trade_oddsTypeCondition)
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }

                        //}
                    }
                    else if (trade_period == "1st Period" && trade_market == "Total Goals")
                    {
                        //if (marketName == "1st Quarter")
                        //{
                        //    string header = odd_obj["header"].ToString();
                        //    if (leagueName == "NBA" || leagueName == "NCAAB" || leagueName == "NCAAF" || leagueName == "NFL" || leagueName == "CFL" || leagueName == "NCAAB Extra Games" || leagueName == "NHL")
                        //    {
                        //        if (header == "1")
                        //            header = "2";
                        //        else if (header == "2")
                        //            header = "1";
                        //    }

                        //    if (odd_obj["name"].ToString() == "Money Line")
                        //    {
                        //        if ((header == "1" && trade_runnerText == "Home") || (header == "2" && trade_runnerText == "Away"))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }

                        //}

                    }
                    else if (trade_period == "1st Period" && trade_market == "Asian Handicap")
                    {
                        //if (marketName == "1st Quarter")
                        //{
                        //    if (odd_obj["name"].ToString() == "Spread")
                        //    {
                        //        double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //        double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //        if (hand_val1 == hand_val2)
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                        //else if (marketName.Contains("1st Quarter Point Spread"))
                        //{
                        //    double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //    double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //    if (hand_val1 == hand_val2)
                        //    {
                        //        trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //        selectionId = odd_obj["id"].ToString();
                        //        isFound = true;
                        //        break;
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Period" && trade_market == "Money Line")
                    {
                        //if (marketName == "1st Quarter")
                        //{
                        //    if (odd_obj["name"].ToString() == "Money Line")
                        //    {
                        //        string header = odd_obj["header"].ToString();
                        //        if (leagueName == "NBA" || leagueName == "NCAAB" || leagueName == "NCAAF" || leagueName == "NFL" || leagueName == "CFL" || leagueName == "NCAAB Extra Games" || leagueName == "NHL")
                        //        {
                        //            if (header == "1")
                        //                header = "2";
                        //            else if (header == "2")
                        //                header = "1";
                        //        }

                        //        if ((header == "1" && trade_runnerText == "Home") || (header == "2" && trade_runnerText == "Away"))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else if (trade_period == "1st Period" && trade_market == "3-Way Handicap")
                    {
                        //if (marketName == "1st Quarter 3 Way Lines")
                        //{
                        //    if (odd_obj["name"].ToString() == "Spread")
                        //    {
                        //        double hand_val1 = Utils.getHandicap(odd_obj["handicap"].ToString());
                        //        double hand_val2 = Utils.getHandicap(trade_oddsTypeCondition.ToString());
                        //        string header = odd_obj["header"].ToString();
                        //        if (leagueName == "NBA" || leagueName == "NCAAB" || leagueName == "NCAAF" || leagueName == "NFL" || leagueName == "CFL" || leagueName == "NCAAB Extra Games" || leagueName == "NHL")
                        //        {
                        //            if (header == "1")
                        //                header = "2";
                        //            else if (header == "2")
                        //                header = "1";
                        //        }
                        //        if (hand_val1 == hand_val2 && ((trade_runnerText == "Home" && header == "1") || (trade_runnerText == "Away" && header == "2")))
                        //        {
                        //            trade.bet365Link = GetDirectLink(market_link_pairs, marketId);
                        //            selectionId = odd_obj["id"].ToString();
                        //            isFound = true;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                }
            }

            if (string.IsNullOrEmpty(parseBet.TabLabel) || string.IsNullOrEmpty(parseBet.MarketLabel))
                return null;
            return parseBet;
        }
    }
    public class OpenBet_Bet365
    {
        public string id;
        public double stake;
        public bool doubleBet;
        public List<BetData_Bet365> betData;

        public string betGuid;     //temporary value for betting 
        public string cc;
        public string pc;
        public bool sl;

        public string tr;       //bet id in bet365 for checking result.
        public OpenBet_Bet365()
        {
            doubleBet = false;
            id = string.Empty;
            betData = new List<BetData_Bet365>();

            tr = string.Empty;
        }
    }

    public class OpenBet_Leovegas
    {
        public string betOfferId;
        public string outcomeId;
    }
    public class OpenBet_Goldbet
    {
        public string market_mn;
        public string market_mi;
        public string market_mti;
        public string market_oc;
        public string market_sn;
        public string market_si;
        public string market_oi;

        public bool isLive;
    }
    //public class OpenBet_Goldbet
    //{
    //    public string market_mn = "1X2";
    //    public string market_mi = "1329626039";
    //    public string market_mti = "10";
    //    public string market_oc = "10340";
    //    public string market_sn = "1";
    //    public string market_si = "-692711197";
    //    public string market_oi = "29068473198";

    //    public bool isLive = false;
    //}
    public class OpenBet_Eurobet
    {
        public int marketId;
        public int betCode;
        public int resultCode;
        public int eventCode;
        public int programCode;
    }
    public class OpenBet_Paddypower
    {
        public string marketId;
        public string selectionId;
    }

    public class OpenBet_Unibet
    {
        public string marketId { get; set; }
        public string offerId { get; set; }
        public string eventId { get; set; }
        public string marketName { get; set; }
        public string odds { get; set; }
        public OpenBet_Unibet(string _market, string _offer, string _eventid, string _odd)
        {
            marketId = _market;
            offerId = _offer;
            eventId = _eventid;
            odds = _odd;
        }
        public OpenBet_Unibet() { }
    }

    public class OpenBet_Sisal
    {
        public string sublink { get; set; }

        public List<string> eventIds { get; set; }        
        public OpenBet_Sisal()
        {
            eventIds = new List<string>();
        }
    }
    public class OpenBet_Betfair
    {
        public string eventId { get; set; }
        public string bseId { get; set; }
        public string bsmId { get; set; }
        public string bssId { get; set; }
        public OpenBet_Betfair(string _bseId, string _bsmId, string _bssId, string _eventid)
        {
            eventId = _eventid;
            bseId = _bseId;
            bsmId = _bsmId;
            bssId = _bssId;
        }
        public OpenBet_Betfair() { }
    }
}
