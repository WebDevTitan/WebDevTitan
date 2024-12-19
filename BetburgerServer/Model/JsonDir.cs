using BetburgerServer.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetburgerServer.Model
{
    public class JsonDir
    {
        public List<JsonDirFormula> arb_formulas { get; set; }
        public List<JsonDirType> arb_types { get; set; }
        public List<JsonDirBetCombination> bet_combinations { get; set; }
        public List<JsonDirBetValue> bet_values { get; set; }
        public List<JsonDirBetVariation> bet_variations { get; set; }
        public List<JsonDirBookmakerClone> bookmaker_clones { get; set; }
        public JsonDirTotalBookmaker bookmakers { get; set; }
        public List<JsonDirCurrency> currencies { get; set; }
        public List<JsonDirMarketVariation> market_variations { get; set; }
        public List<JsonDirMarket> markets { get; set; }
        public List<JsonDirPeriod> periods { get; set; }
        public List<object> sport_periods { get; set; }
        public List<JsonDirSport> sports { get; set; }
        public string getSportById(string id)
        {
            foreach (JsonDirSport sport in sports)
            {
                if (sport.id == id)
                    return sport.name;
            }

            return string.Empty;
        }

        public string getBookmakerById(string id)
        {
            foreach (JsonDirBookmaker bookmaker in bookmakers.arbs)
            {
                if (bookmaker.id == id)
                    return bookmaker.name;
            }

            return string.Empty;
        }

        public void getOutcomeById(string id, ref string outcome, ref string variation, ref string value, ref string market_id , List<JsonDirBetCombination> combines)
        {
            foreach (JsonDirBetCombination bc in combines)
            {
                if (id == bc.id)
                {
                    string mv_id = bc.mv_id;
                    string val_id = bc.value_id;
                    var title = string.Empty;

                    var n = getMarketVariationById(mv_id, ref market_id, ref title);
                    var i = "";
                    if (val_id != null)
                        i = getValueById(val_id);
                    else
                        i = "";

                    double dI = 0;
                    if (!double.TryParse(i, out dI) || dI == 0)
                    {
                        variation = n;
                        value = null;
                        outcome = string.Format(Constants.getMarketVariaton(title), 0);
                        return;
                    }

                    string u = ((dI > 0 && n.IndexOf("F") > -1) ? "+" : "") + Math.Round(dI, 1).ToString();

                    if (n == "CS" || n == "CS_N" || n == "SET_CS" || n == "SET_CS_N")
                    {
                        var t = "";
                        if (!string.IsNullOrEmpty(i))
                            t = Math.Round(double.Parse(i), 1).ToString();

                        string[] ts = t.Split(new char[] { '.' }, StringSplitOptions.None);
                        if (ts == null || ts.Length < 2)
                        {
                            variation = n;
                            value = u;
                            outcome = string.Format(Constants.getMarketVariaton(title), u);
                            return;
                        }

                        outcome = ts[0] + ":" + ts[1];
                        return;
                    }
                    else
                    {
                        variation = n;
                        value = u;
                        outcome = string.Format(Constants.getMarketVariaton(title), u);
                    }
                }
            }
        }

        private string getMarketVariationById(string id, ref string market_id, ref string title)
        {
            foreach (JsonDirMarketVariation mv in market_variations)
            {
                if (id == mv.id)
                {
                    string bv_id = mv.bet_variation_id;
                    market_id = mv.market_id;
                    title = mv.title;
                    return getVariationById(bv_id);
                }
            }

            return string.Empty;
        }

        private string getMarketVariationById(string id)
        {
            foreach (JsonDirMarketVariation mv in market_variations)
            {
                if (id == mv.id)
                {
                    string bv_id = mv.bet_variation_id;
                    return getVariationById(bv_id);
                }
            }

            return string.Empty;
        }

        private string getVariationById(string id)
        {
            foreach (JsonDirBetVariation bv in bet_variations)
            {
                if (id == bv.id)
                    return bv.name;
            }

            return string.Empty;
        }

        private string getValueById(string id)
        {
            foreach (JsonDirBetValue bv in bet_values)
            {
                if (id == bv.id)
                    return bv.value;
            }

            return string.Empty;
        }

        public string getCalcFormula(string id)
        {
            foreach (JsonDirFormula cf in arb_formulas)
            {
                if (cf.id == id)
                    return cf.calc_formula;
            }

            return string.Empty;
        }

        private string getPeriodIdentifier(string id)
        {
            foreach (JsonDirPeriod p in periods)
            {
                if (p.id == id)
                    return p.identifier;
            }

            return string.Empty;
        }

        public string getPeriodTitle(string periodId, string sportId)
        {
            string title = string.Empty;
            string periodIdentifier = getPeriodIdentifier(periodId);

            try
            {
                string n = string.Empty, r = string.Empty;
                int i = int.Parse(periodIdentifier), s = int.Parse(sportId);

                if (i == -2)
                    r = s == 6 ? "with overtime and shootouts" : "match";
                else if (i == -3)
                    r = "match (doubles)";
                else if (i == -1)
                {
                    if (s == 8 || s == 13)
                    { n = "match"; r = ""; }
                    else
                        r = "with overtime";
                }
                else if (i == 0)
                {
                    if (s == 1 || s == 7 || s == 8 || s == 9 || s == 11 || s == 12 || s == 13 || s == 14 || s == 21 || s == 29)
                    {
                        n = "match"; r = "";
                    }
                    else
                        r = "regular time";
                }
                else if (i == -19 || i == -16 || i == -13 || i == -10 || i == -7)
                {
                    n = "match";
                    r = "";
                }
                else if (i == -18 || i == -15 || i == -12 || i == -9 || i == -6)
                {
                    r = "1 time";
                }
                else if (i == -17 || i == -14 || i == -11 || i == -8 || i == -5)
                    r = "2 time";
                else if (i == -100)
                    r = "next round";
                else if (s == 11)
                    r = i.ToString() + " frame";
                else if (s == 8 || s == 9 || s == 13 || s == 14)
                {
                    if (i > 100)
                    {
                        var o = i / 100;
                        var u = i - o * 100;
                        r = o.ToString() + " set, " + u + " game";
                    }
                    else
                        r = i.ToString() + " set";
                }
                else
                {
                    if (s == 6)
                        r = i.ToString() + " period";
                    else
                    {
                        if (s == 7)
                            r = i.ToString() + " time";
                        else
                        {
                            if (s == 1 || s == 2 || s == 10 || s == 16 || s == 20)
                            {
                                if (i == 10 || i == 20)
                                    r = (i / 10).ToString() + " half";
                                else
                                {
                                    r = s == 1 ? i.ToString() + " inning" : i.ToString() + " quarter";
                                }
                            }
                            else
                            {
                                r = i.ToString() + " half";
                            }
                        }
                    }
                }

                return !string.IsNullOrEmpty(n) ? n : r;
            }
            catch (Exception e)
            {
                return title;
            }
        }
    }
}
