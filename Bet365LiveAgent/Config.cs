using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bet365LiveAgent
{
    
    public enum CONDITION_CHECKER
    {     
        //[Description("Match Time")]
        [Description("比赛时间")]
        MATCH_TIME,

        //[Description("Exact Score")]
        [Description("比分")]
        SCORE_DETAIL,
        //[Description("Score Sum")]
        [Description("比分和")]
        SCORE_SUM,

        //[Description("Home Team Goals")]
        [Description("主队进球")]
        HOME_GOALS,
        //[Description("Home Team OnTarget")]
        [Description("主队射正")]
        HOME_ONTARGET,
        //[Description("Home Team Shots")]
        [Description("主队射偏")]
        HOME_SHOTS,
        //[Description("Home Team Shots+OnTarget")]
        [Description("主队射门")]
        HOME_SHOTS_PLUS_ONTARGET,
        //[Description("Home Team Dangerous Attacks")]
        [Description("主队危险进攻")]
        HOME_DANGEROUSATTACKS,
        //[Description("Home Team Attacks")]
        [Description("主队进攻")]
        HOME_ATTACKS,

        //[Description("Home Team Goals - Ratio")]
        [Description("主队进球比")]
        HOME_GOALS_RATIO,
        //[Description("Home Team OnTarget - Ratio")]
        [Description("主队射正比")]
        HOME_ONTARGET_RATIO,
        //[Description("Home Team Shots - Ratio")]
        [Description("主队射偏比")]
        HOME_SHOTS_RATIO,
        //[Description("Home Team Shots+OnTarget - Ratio")]
        [Description("主队射门比")]
        HOME_SHOTS_PLUS_ONTARGET_RATIO,
        //[Description("Home Team Dangerous Attacks - Ratio")]
        [Description("主队危险进攻比")]
        HOME_DANGEROUSATTACKS_RATIO,
        //[Description("Home Team Attacks - Ratio")]
        [Description("主队进攻比")]
        HOME_ATTACKS_RATIO,

        //[Description("Away Team Goals")]
        [Description("客队进球")]
        AWAY_GOALS,
        //[Description("Away Team OnTarget")]
        [Description("客队射正")]
        AWAY_ONTARGET,
        //[Description("Away Team Shots")] 
        [Description("客队射偏")]
        AWAY_SHOTS,
        //[Description("Away Team Shots+OnTarget")] 
        [Description("客队射门")]
        AWAY_SHOTS_PLUS_ONTARGET,
        //[Description("Away Team Dangerous Attacks")] 
        [Description("客队危险进攻")]
        AWAY_DANGEROUSATTACKS,
        //[Description("Away Team Attacks")] 
        [Description("客队进攻")]
        AWAY_ATTACKS,

        //[Description("Away Team Goals - Ratio")]
        [Description("客队进球比")]
        AWAY_GOALS_RATIO,
        //[Description("Away Team OnTarget - Ratio")]
        [Description("客队射正比")]
        AWAY_ONTARGET_RATIO,
        //[Description("Away Team Shots - Ratio")]
        [Description("客队射偏比")]
        AWAY_SHOTS_RATIO,
        //[Description("Away Team Shots+OnTarget - Ratio")]
        [Description("客队射门比")]
        AWAY_SHOTS_PLUS_ONTARGET_RATIO,
        //[Description("Away Team Dangerous Attacks - Ratio")]
        [Description("客队危险进攻比")]
        AWAY_DANGEROUSATTACKS_RATIO,
        //[Description("Away Team Attacks - Ratio")]
        [Description("客队进攻比")]
        AWAY_ATTACKS_RATIO,

        [Description("比赛最近时间")]
        MATCH_RECENT_TIME,
    }

    public enum COMPARISON
    {
        [Description(">")]
        GREATER,
        [Description(">=")]
        GREATER_EQUAL,
        [Description("<")]
        LESS,
        [Description("<=")]
        LESS_EQUAL,
        [Description("=")]
        EQUAL,        
    }

    public enum BET_PLUSMINUS
    {        
        [Description("+")]
        PLUS,        
        [Description("-")]
        MINUS
    }

    public enum BET_MARKET
    {
        //[Description("Asian Handicap")]
        [Description("亚洲让分盘")]
        ASIAN_HANDICAP,
        //[Description("Goal Line")]
        [Description("大小盘")]
        GOAL_LINE
    }

    public enum BET_TEAM
    {
        //[Description("Home Team")]
        [Description("主队")]
        HOME_TEAM,
        //[Description("Away Team")]
        [Description("客队")]
        AWAY_TEAM
    }

    public enum BET_OVERUNDER
    {
        //[Description("Over")]
        [Description("高于")]
        OVER,
        //[Description("Under")]
        [Description("低于")]
        UNDER
    }
    [Serializable]
    public class CONDITION
    {
        public CONDITION_CHECKER Checker;
        public COMPARISON Comparer;
        public string Value;
        public string Param1;
        public string Param2;

        public CONDITION()
        {
            Checker = Utils.Revert(Checker);
        }

        public CONDITION(CONDITION condition)
        {
            Checker = condition.Checker;
            Comparer = condition.Comparer;
            Value = condition.Value;
            Param1 = condition.Param1;
            Param2 = condition.Param2;
        }

        public void Revert()
        {
            Checker = Utils.Revert(Checker);
        }
        public override string ToString()
        {
            return $"{Checker.GetDescription()} {Comparer.GetDescription()} {Value}"; 
        }

        public static bool operator ==(CONDITION obj1, CONDITION obj2)
        {
            if (obj1 is null && obj2 is null)
                return true;

            if (obj1 is null || obj2 is null)
                return false;
            

            if (obj1.Checker == obj2.Checker &&
                obj1.Comparer == obj2.Comparer &&
                obj1.Value == obj2.Value &&
                obj1.Param1 == obj2.Param1 &&
                obj1.Param2 == obj2.Param2)
                return true;
            return false;
        }
        public static bool operator !=(CONDITION obj1, CONDITION obj2) => !(obj1 == obj2);
    }

    [Serializable]
    public class COMMAND
    {
        //public List<List<CONDITION>> Conditions = new List<List<CONDITION>>;
        public List<CONDITION> Conditions = new List<CONDITION>();

        public BET_MARKET BetMarket;

        public BET_TEAM BetTeam;                        //Asian Handicap
        public BET_PLUSMINUS BetPlusMinus;              //Asian Handicap
        public BET_OVERUNDER BetOverUnder;              //Goal Line
        public string Handicap;

        public COMPARISON OddComparer;
        public string OddValue;

        public COMMAND()
        {

        }
        public COMMAND(COMMAND command)
        {
            foreach (CONDITION condition in command.Conditions)
                Conditions.Add(new CONDITION(condition));

            BetTeam = command.BetTeam;
            BetMarket = command.BetMarket;
            BetPlusMinus = command.BetPlusMinus;
            BetOverUnder = command.BetOverUnder;
            Handicap = command.Handicap;
            OddComparer = command.OddComparer;
            OddValue = command.OddValue;
        }

        public void Revert()
        {
            for (int i = 0; i < Conditions.Count; i++)                
                Conditions[i].Revert();
                        
            BetOverUnder = Utils.Revert(BetOverUnder);
            BetTeam = Utils.Revert(BetTeam);
        }
        public override string ToString()
        {
            string cond = "";
            foreach (var condition in Conditions)
                cond += $"{condition} | ";
            if (cond.Length > 3)
                cond = cond.Substring(0, cond.Length - 3);
                        
            string result = "";
            if (BetMarket == BET_MARKET.ASIAN_HANDICAP)
                result = $"{cond} ==> {BetMarket.GetDescription()} {BetTeam.GetDescription()}({BetPlusMinus.GetDescription()}{Handicap})  Odd {OddComparer.GetDescription()} {OddValue}";
            else if (BetMarket == BET_MARKET.GOAL_LINE)
                result = $"{cond} ==> {BetMarket.GetDescription()} {BetOverUnder.GetDescription()}({Handicap})  Odd {OddComparer.GetDescription()} {OddValue}";
            return result;
        }
               
        public static bool operator ==(COMMAND obj1, COMMAND obj2)
        {
            if (obj1 is null && obj2 is null)
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            if (obj1.Conditions.Count != obj2.Conditions.Count)
                return false;   

            bool bIsEqual = true;
            for (int i = 0; i < obj1.Conditions.Count; i++)
                if (obj1.Conditions[i] != obj2.Conditions[i])
                {
                    bIsEqual = false;
                    break;
                }
            
            if (bIsEqual &&
                obj1.BetTeam == obj2.BetTeam &&
                obj1.BetMarket == obj2.BetMarket &&
                obj1.BetPlusMinus == obj2.BetPlusMinus &&
                obj1.BetOverUnder == obj2.BetOverUnder &&
                obj1.Handicap == obj2.Handicap &&
                obj1.OddComparer == obj2.OddComparer &&
                obj1.OddValue == obj2.OddValue)
                return true;
            return false;
        }
        public static bool operator !=(COMMAND obj1, COMMAND obj2) => !(obj1 == obj2);
    }
    class Config
    {      

        string confName = "setting.conf";

        public string Bet365Domain { get; set; }

        public List<COMMAND> Commands = null;
       
        private static Config _instance = null;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Config();
                return _instance;
            }            
        }

        public Config()
        {
            
        }

        public void CommandsSort()
        {
            for (int i = 0; i < Config.Instance.Commands.Count - 1; i++)
            {
                for (int j = i + 1; j < Config.Instance.Commands.Count; j++)
                {
                    if (Config.Instance.Commands[i].BetMarket == BET_MARKET.ASIAN_HANDICAP && Config.Instance.Commands[j].BetMarket == BET_MARKET.ASIAN_HANDICAP)
                    {
                        double iHandicap = Convert.ToDouble(Config.Instance.Commands[i].Handicap);
                        if (Config.Instance.Commands[i].BetPlusMinus == BET_PLUSMINUS.PLUS)
                            iHandicap = Math.Abs(iHandicap);
                        else
                            iHandicap = 0 - Math.Abs(iHandicap);

                        double jHandicap = Convert.ToDouble(Config.Instance.Commands[j].Handicap);
                        if (Config.Instance.Commands[j].BetPlusMinus == BET_PLUSMINUS.PLUS)
                            jHandicap = Math.Abs(jHandicap);
                        else
                            jHandicap = 0 - Math.Abs(jHandicap);

                        if (iHandicap < jHandicap)
                        {
                            COMMAND temp = Config.Instance.Commands[j];
                            Config.Instance.Commands[j] = Config.Instance.Commands[i];
                            Config.Instance.Commands[i] = temp;
                        }
                    }
                }
            }

        }
        public void LoadConfig()
        {
            if (!File.Exists(confName))
                return;

            string jsonSetting = File.ReadAllText(confName);
            _instance = JsonConvert.DeserializeObject<Config>(jsonSetting);



            if (_instance.Commands == null)
                _instance.Commands = new List<COMMAND>();

            CommandsSort();
        }

        public void SaveConfig()
        {
            string jsonSetting = JsonConvert.SerializeObject(_instance);
            File.WriteAllText(confName, jsonSetting);
        }
    }
}
