using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bet365LiveAgent
{    
    static class Utils
    {
        public static bool bRun = false;

        private static NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;
        private static CultureInfo culture = CultureInfo.CreateSpecificCulture("es");

        public static T CreateDeepCopy<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }
        public static string TrimStart(this string sourceString, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
                return sourceString;

            string result = sourceString;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(this string sourceString, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
                return sourceString;

            string result = sourceString;
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        public static string Trim(this string sourceString, string trimString)
        {
            if (string.IsNullOrEmpty(trimString))
                return sourceString;

            string result = sourceString;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }
            while (result.EndsWith(trimString))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        public static string GetDescription(this Enum enumValue)
        {
            return ((DescriptionAttribute)Attribute.GetCustomAttribute((enumValue.GetType().GetField(enumValue.ToString())), typeof(DescriptionAttribute))).Description;
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary == null)            
                throw new ArgumentNullException(nameof(dictionary)); // using C# 6            
            if (key == null)
                throw new ArgumentNullException(nameof(key)); //  using C# 6

            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static string GenerateRandomNumberString(int length)
        {            
            StringBuilder strBuilder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {                
                strBuilder.Append(random.Next(0, 10));
            }

            return strBuilder.ToString();
        }

        public static BET_PLUSMINUS Revert(BET_PLUSMINUS param)
        {
            switch (param)
            {
                case BET_PLUSMINUS.PLUS:
                    return BET_PLUSMINUS.MINUS;
                case BET_PLUSMINUS.MINUS:
                    return BET_PLUSMINUS.PLUS;
            }
            return BET_PLUSMINUS.PLUS;
        }

        public static BET_OVERUNDER Revert(BET_OVERUNDER param)
        {
            switch (param)
            {
                case BET_OVERUNDER.OVER:
                    return BET_OVERUNDER.UNDER;
                case BET_OVERUNDER.UNDER:
                    return BET_OVERUNDER.OVER;
            }
            return BET_OVERUNDER.UNDER;
        }

        public static BET_TEAM Revert(BET_TEAM param)
        {
            switch (param)
            {
                case BET_TEAM.HOME_TEAM:
                    return BET_TEAM.AWAY_TEAM;
                case BET_TEAM.AWAY_TEAM:
                    return BET_TEAM.HOME_TEAM;
            }
            return BET_TEAM.HOME_TEAM;
        }

        public static bool CheckCondition(double checker, COMPARISON comparer, double value)
        {
            bool bResult = false;
            switch (comparer)
            {
                case COMPARISON.GREATER:
                    if (checker > value)
                        bResult = true;
                    break;
                case COMPARISON.GREATER_EQUAL:
                    if (checker >= value)
                        bResult = true;
                    break;
                case COMPARISON.LESS:
                    if (checker < value)
                        bResult = true;
                    break;
                case COMPARISON.LESS_EQUAL:
                    if (checker <= value)
                        bResult = true;
                    break;
                case COMPARISON.EQUAL:
                    if (checker == value)
                        bResult = true;
                    break;
            }
            return bResult;
        }

        public static double ParseToDouble(string str)
        {
            str = str.Replace("\"", "").Replace(",", ".").Replace(" ", "");
            double value = 0;
            double.TryParse(str, style, CultureInfo.InvariantCulture, out value);
            return value;
        }
        public static CONDITION_CHECKER Revert(CONDITION_CHECKER param)
        {
            switch (param)
            {
                case CONDITION_CHECKER.HOME_GOALS:
                    return CONDITION_CHECKER.AWAY_GOALS;
                case CONDITION_CHECKER.HOME_ONTARGET:
                    return CONDITION_CHECKER.AWAY_ONTARGET;
                case CONDITION_CHECKER.HOME_SHOTS:
                    return CONDITION_CHECKER.AWAY_SHOTS;
                case CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET:
                    return CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET;
                case CONDITION_CHECKER.HOME_DANGEROUSATTACKS:
                    return CONDITION_CHECKER.AWAY_DANGEROUSATTACKS;
                case CONDITION_CHECKER.HOME_ATTACKS:
                    return CONDITION_CHECKER.AWAY_ATTACKS;
                case CONDITION_CHECKER.HOME_GOALS_RATIO:
                    return CONDITION_CHECKER.AWAY_GOALS_RATIO;
                case CONDITION_CHECKER.HOME_ONTARGET_RATIO:
                    return CONDITION_CHECKER.AWAY_ONTARGET_RATIO;
                case CONDITION_CHECKER.HOME_SHOTS_RATIO:
                    return CONDITION_CHECKER.AWAY_SHOTS_RATIO;
                case CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET_RATIO:
                    return CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET_RATIO;
                case CONDITION_CHECKER.HOME_DANGEROUSATTACKS_RATIO:
                    return CONDITION_CHECKER.AWAY_DANGEROUSATTACKS_RATIO;
                case CONDITION_CHECKER.HOME_ATTACKS_RATIO:
                    return CONDITION_CHECKER.AWAY_ATTACKS_RATIO;

                case CONDITION_CHECKER.AWAY_GOALS:
                    return CONDITION_CHECKER.HOME_GOALS;
                case CONDITION_CHECKER.AWAY_ONTARGET:
                    return CONDITION_CHECKER.HOME_ONTARGET;
                case CONDITION_CHECKER.AWAY_SHOTS:
                    return CONDITION_CHECKER.HOME_SHOTS;
                case CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET:
                    return CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET;
                case CONDITION_CHECKER.AWAY_DANGEROUSATTACKS:
                    return CONDITION_CHECKER.HOME_DANGEROUSATTACKS;
                case CONDITION_CHECKER.AWAY_ATTACKS:
                    return CONDITION_CHECKER.HOME_ATTACKS;
                case CONDITION_CHECKER.AWAY_GOALS_RATIO:
                    return CONDITION_CHECKER.HOME_GOALS_RATIO;
                case CONDITION_CHECKER.AWAY_ONTARGET_RATIO:
                    return CONDITION_CHECKER.HOME_ONTARGET_RATIO;
                case CONDITION_CHECKER.AWAY_SHOTS_RATIO:
                    return CONDITION_CHECKER.HOME_SHOTS_RATIO;
                case CONDITION_CHECKER.AWAY_SHOTS_PLUS_ONTARGET_RATIO:
                    return CONDITION_CHECKER.HOME_SHOTS_PLUS_ONTARGET_RATIO;
                case CONDITION_CHECKER.AWAY_DANGEROUSATTACKS_RATIO:
                    return CONDITION_CHECKER.HOME_DANGEROUSATTACKS_RATIO;
                case CONDITION_CHECKER.AWAY_ATTACKS_RATIO:
                    return CONDITION_CHECKER.HOME_ATTACKS_RATIO;
                default:
                    return param;
            }
        }
    }
}
