using BetburgerServer.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    [Serializable]
    public enum ODDSTYPES
    {
        threeway = 69,
        points = 192,
        totals = 47,
        ahc = 48,
        dnb = 112,
        ehc = 8,
        setHandicap = 71,
        setOverUnder = 100,
        gameHandicap = 39,
        doubble = 9,
        homeAway = 70,
        moneyline = 466,
        killsHandicap = 365,
        killsOverUnder = 328,
        roundsHandicap = 379,
        roundsOverUnder = 377
    };

    public class JsonTrade
    {
        public string bet365Link { get; set; }
        public string selectionId { get; set; }
        public string id { get; set; }
        public string homeTeam { get; set; }
        public string homeTeamId { get; set; }
        public string awayTeam { get; set; }
        public string awayTeamId { get; set; }
        public string countryCodeHome { get; set; }
        public string countryCodeAway { get; set; }
        public string bookmaker { get; set; }
        public string couponKey { get; set; }
        public string countryName { get; set; }
        public string eventId { get; set; }
        public int eventPartId { get; set; }
        public double edge { get; set; }
        public double kelly { get; set; }
        public string leagueName { get; set; }
        public double yardstick { get; set; }
        public double volume { get; set; }
        public int outcomeTypeId { get; set; }
        public int oddsType { get; set; }
        public double oddsTypeCondition { get; set; }
        public double baseline { get; set; }
        public double odds { get; set; }
        public string lastUpdated { get; set; }
        public string output { get; set; }
        public string sportId { get; set; }
        public string startTime { get; set; }
        public string templateId { get; set; }
        public int typeId { get; set; }
        public int venueId { get; set; }
        public string bettingOfferId { get; set; }
        public string lineId { get; set; }
        public string outcomeId { get; set; }
        public string market { get; set; }
        public string b3Market { get; set; }
        public string period { get; set; }
        public string bet365FI { get; set; }

        public string bookmakerName
        {
            get
            {
                return GameConstants.Tradematesports_getBookieTitle(bookmaker);
            }
        }

        public string sportName
        {
            get
            {
                return GameConstants.Tradematesports_getSportTitle(sportId);
            }
        }

        public string startIn
        {
            get
            {
                TimeSpan timediff = (Utils.UnixTimeStampToDateTime(double.Parse(startTime)) - DateTime.UtcNow);
                double hours = 0;
                if (timediff.Days > 0)
                    hours += timediff.Days * 24;

                hours += timediff.Hours;
                return string.Format("{0} h {1} m", hours, timediff.Minutes);
            }
        }

        public string participant
        {
            get
            {
                return "o1" == this.output ? this.homeTeamId : this.awayTeamId;
            }
        }


        public string outcomeText
        {
            get
            {
                string participant = "o1" == this.output ? this.homeTeamId : this.awayTeamId;
                switch ((ODDSTYPES)oddsType)
                {
                    case ODDSTYPES.threeway:
                        return participant == homeTeamId ? "1x2 (" + homeTeam + ")" : participant == awayTeamId ? "1x2 (" + awayTeam + ")" : "Draw";
                    case ODDSTYPES.roundsOverUnder:
                        return "o1" == output ? "Over " + oddsTypeCondition + " Rounds" : "o2" == output ? "Under " + oddsTypeCondition + " Rounds" : "N/A";
                    case ODDSTYPES.killsOverUnder:
                        return "o1" == output ? "Over " + oddsTypeCondition + " Kills" : "o2" == output ? "Under " + oddsTypeCondition + " Kills" : "N/A";
                    case ODDSTYPES.setOverUnder:
                        return "o1" == output ? "Over " + oddsTypeCondition + " Sets" : "o2" == output ? "Under " + oddsTypeCondition + " Sets" : "N/A";
                    case ODDSTYPES.totals:
                        return 13 == typeId ? "Over " + oddsTypeCondition : 14 == typeId ? "Under " + oddsTypeCondition : "N/A";
                    case ODDSTYPES.homeAway:
                        return participant == homeTeamId ? homeTeam + " to win" : awayTeam + " to win";
                    case ODDSTYPES.moneyline:
                        return participant == homeTeamId ? homeTeam + " to win" : awayTeam + " to win";
                    case ODDSTYPES.dnb:
                        return participant == awayTeamId ? "Draw no bet (" + awayTeam + ")" : "Draw no bet (" + homeTeam + ")";
                    case ODDSTYPES.ahc:
                        return participant == homeTeamId ? "Asian hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + homeTeam + ")") : participant == awayTeamId ? "Asian hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + awayTeam + ")") : "N/A";
                    case ODDSTYPES.points:
                        return "o1" == output ? oddsTypeCondition < 0 ? "Handicap " + oddsTypeCondition.ToString("N2") + (" (" + homeTeam + ")") : "Handicap +" + oddsTypeCondition.ToString("N2") + (" (" + homeTeam + ")") : oddsTypeCondition < 0 ? "Handicap +" + (-1 * oddsTypeCondition).ToString("N2") + (" (" + awayTeam + ")") : "Handicap " + (-1 * oddsTypeCondition).ToString("N2") + (" (" + awayTeam + ")");
                    case ODDSTYPES.ehc:
                        string u = "";
                        u = oddsTypeCondition >= 0 ? "(" + oddsTypeCondition.ToString() + "-0)" : "(0" + oddsTypeCondition.ToString() + ")";
                        return "o1" == output ? "Euro hcp " + u + (" (" + homeTeam + ")") : "Euro hcp " + u + (" (" + awayTeam + ")");
                    case ODDSTYPES.setHandicap:
                    case ODDSTYPES.gameHandicap:
                        string c = (ODDSTYPES)oddsType == ODDSTYPES.setHandicap ? "Set" : "Game";
                        return participant == homeTeamId ? c + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + homeTeam + ")") : participant == awayTeamId ? c + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + awayTeam + ")") : "N/A";
                    case ODDSTYPES.roundsHandicap:
                        string cc = "Rounds";
                        return participant == homeTeamId ? cc + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + homeTeam + ")") : participant == awayTeamId ? cc + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + awayTeam + ")") : "N/A";
                    case ODDSTYPES.killsHandicap:
                        string ccc = "Kills";
                        return participant == homeTeamId ? ccc + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + homeTeam + ")") : participant == awayTeamId ? ccc + " hcp " + (oddsTypeCondition >= 0 ? "+" : "") + oddsTypeCondition + (" (" + awayTeam + ")") : "N/A";
                    default:
                        return "N/A";
                }
                return string.Empty;
            }
        }

        public string marketText
        {
            get
            {
                string participant = "o1" == this.output ? this.homeTeamId : this.awayTeamId;
                switch ((ODDSTYPES)oddsType)
                {
                    case ODDSTYPES.threeway:
                        return "1X2";
                    case ODDSTYPES.roundsOverUnder:
                        return "Total Rounds";
                    case ODDSTYPES.killsOverUnder:
                        return "Total Kills";
                    case ODDSTYPES.setOverUnder:
                        return "Total Sets";
                    case ODDSTYPES.totals:
                        return "Total";
                    case ODDSTYPES.homeAway:
                        return "Winner";
                    case ODDSTYPES.moneyline:
                        return "Money Line";
                    case ODDSTYPES.dnb:
                        return "Draw no bet";
                    case ODDSTYPES.ahc:
                        return "Asian handicap";
                    case ODDSTYPES.points:
                        return "Handicap";
                    case ODDSTYPES.ehc:
                        return "3-Way Handicap";
                    case ODDSTYPES.setHandicap:
                        return "Set Handicap";
                    case ODDSTYPES.gameHandicap:
                        return "Game Handicap";
                    case ODDSTYPES.roundsHandicap:
                        return "Rounds Handicap";
                    case ODDSTYPES.killsHandicap:
                        return "Kills Handicap";
                    default:
                        return "N/A";
                }
                return string.Empty;
            }
        }

        public string runnerTextAlt
        {
            get
            {
                string participant = "o1" == this.output ? this.homeTeamId : this.awayTeamId;
                switch ((ODDSTYPES)oddsType)
                {
                    case ODDSTYPES.threeway:
                        {
                            participant = "o1" == this.output ? this.homeTeamId : this.output == "o3" ? this.awayTeamId : "";
                            return participant == homeTeamId ? "Home" : participant == awayTeamId ? "Away" : "Draw";
                        }
                    case ODDSTYPES.roundsOverUnder:
                        return "o1" == output ? $"Over {oddsTypeCondition}" : "o2" == output ? $"Under {oddsTypeCondition}" : "N/A";
                    case ODDSTYPES.killsOverUnder:
                        return "o1" == output ? $"Over {oddsTypeCondition}" : "o2" == output ? $"Under {oddsTypeCondition}" : "N/A";
                    case ODDSTYPES.setOverUnder:
                        return "o1" == output ? $"Over {oddsTypeCondition}" : "o2" == output ? $"Under {oddsTypeCondition}" : "N/A";
                    case ODDSTYPES.totals:
                        return "o1" == output ? $"Over {oddsTypeCondition}" : "o2" == output ? $"Under {oddsTypeCondition}" : "N/A";
                    case ODDSTYPES.homeAway:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.moneyline:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.dnb:
                        return participant == awayTeamId ? "Away" : "Home";
                    case ODDSTYPES.ahc:
                        return participant == homeTeamId ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    case ODDSTYPES.points:
                        return "o1" == output ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    case ODDSTYPES.ehc:
                        return "o1" == output ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    case ODDSTYPES.setHandicap:
                    case ODDSTYPES.gameHandicap:
                        return "o1" == output ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    case ODDSTYPES.roundsHandicap:
                        return participant == homeTeamId ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    case ODDSTYPES.killsHandicap:
                        return participant == homeTeamId ? $"Home {oddsTypeCondition}" : $"Away {oddsTypeCondition}";
                    default:
                        return "N/A";
                }
                return string.Empty;
            }

            set { }
        }

        public string runnerText
        {
            get
            {
                string participant = "o1" == this.output ? this.homeTeamId : this.awayTeamId;
                switch ((ODDSTYPES)oddsType)
                {
                    case ODDSTYPES.threeway:
                        {
                            participant = "o1" == this.output ? this.homeTeamId : this.output == "o3" ? this.awayTeamId : "";
                            return participant == homeTeamId ? "Home" : participant == awayTeamId ? "Away" : "Draw";
                        }
                    case ODDSTYPES.roundsOverUnder:
                        return "o1" == output ? "Over" : "o2" == output ? "Under" : "N/A";
                    case ODDSTYPES.killsOverUnder:
                        return "o1" == output ? "Over" : "o2" == output ? "Under" : "N/A";
                    case ODDSTYPES.setOverUnder:
                        return "o1" == output ? "Over" : "o2" == output ? "Under" : "N/A";
                    case ODDSTYPES.totals:
                        return "o1" == output ? "Over" : "o2" == output ? "Under" : "N/A";
                    case ODDSTYPES.homeAway:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.moneyline:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.dnb:
                        return participant == awayTeamId ? "Away" : "Home";
                    case ODDSTYPES.ahc:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.points:
                        return "o1" == output ? "Home" : "Away";
                    case ODDSTYPES.ehc:
                        return "o1" == output ? "Home" : "Away";
                    case ODDSTYPES.setHandicap:
                    case ODDSTYPES.gameHandicap:
                        return "o1" == output ? "Home" : "Away";
                    case ODDSTYPES.roundsHandicap:
                        return participant == homeTeamId ? "Home" : "Away";
                    case ODDSTYPES.killsHandicap:
                        return participant == homeTeamId ? "Home" : "Away";
                    default:
                        return "N/A";
                }
                return string.Empty;
            }

            set { }
        }


        public JsonTrade()
        {

        }
    }
}
