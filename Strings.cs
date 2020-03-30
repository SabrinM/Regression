using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SM
{
    public static class Strings
    {
        public static string ReplaceOptional(string str, bool keep)
        {
            return new Regex("<([^>]*)>").Replace(str, keep ? "$1" : "");
        }

        public static string ReplaceOr(string str, bool first, string splitChar = "/")
        {
            return new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>").Replace(str, first ? "$1" : "$2");
        }

        public static string ReplaceAndOr(string str, bool first, bool second, string splitChar = "&")
        {
            Regex regex = new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>");
            if (first && !second)
                return regex.Replace(str, "$1");
            if (!first & second)
                return regex.Replace(str, "$2");
            if (first & second)
                return regex.Replace(str, "$1 and $2");
            return regex.Replace(str, "");
        }

        public static string InsertVariables(string msg, Body body, Underwear underwear = null)
        {
            string str = msg;
            if (body != null)
                underwear = body.underwear;
            //RegressionMod.monitor.Log(underwear.name, StardewModdingAPI.LogLevel.Debug);
            if (underwear != null)//TODO: fix plural check
                str = Strings.ReplaceOr(str.Replace("$UNDERWEAR_NAME$", underwear.name.ToLower()).Replace("$UNDERWEAR_PREFIX$", underwear.prefix.ToLower()).Replace("$UNDERWEAR_DESC$", underwear.description).Replace("$INSPECT_UNDERWEAR_NAME$", Strings.DescribeUnderwear(underwear, underwear.name.ToLower())).Replace("$INSPECT_UNDERWEAR_DESC$", Strings.DescribeUnderwear(underwear, underwear.description)), false/*!underwear.plural,*/, "#");
            if (body != null)
                str = str.Replace("$PANTS_NAME$", body.bottoms.name.ToLower()).Replace("$PANTS_PREFIX$", body.bottoms.prefix.ToLower()).Replace("$PANTS_DESC$", body.bottoms.description).Replace("$BEDDING_DRYTIME$", Game1.getTimeOfDayString(body.beddingDryTime));
            return Strings.ReplaceOr(str, (bool)Game1.player.isMale, "/").Replace("$FARMERNAME$", (string)Game1.player.name);
        }

        public static string RandString(string[] msgs = null)
        {
            return msgs[RegressionMod.rnd.Next(msgs.Length)];
        }

        public static List<int> ValidUnderwearTypes()
        {
            List<int> list = RegressionMod.data.underwearInformation.Keys.ToList();
            list.Remove((int)UnderwearType.Pants);
            list.Remove((int)UnderwearType.Bed);
            return list;
        }

        public static string DescribeUnderwear(Underwear underwear, string baseDescription = null)
        {
            string newDescription = baseDescription ?? underwear.description; //use u.description if baseDescription is null
            float wetPercent = underwear.Wetness / underwear.absorbency;
            float messyPercent = underwear.Messiness / underwear.containment;
            if ((double)wetPercent == 0.0 && (double)messyPercent == 0.0)
            {
                newDescription = underwear.CleanStatus == 0 ? RegressionMod.data.Underwear_Clean.Replace("$UNDERWEAR_DESC$", newDescription) : RegressionMod.data.Underwear_Drying.Replace("$UNDERWEAR_DESC$", newDescription);
            }
            else
            {
                if ((double)messyPercent > 0.0)
                {
                    for (int index = 0; index < RegressionMod.data.Underwear_Messy.Length; ++index)
                    {
                        float num3 = (float)(((double)index + 1.0) / ((double)RegressionMod.data.Underwear_Messy.Length - 1.0));
                        if (index == RegressionMod.data.Underwear_Messy.Length - 1 || (double)messyPercent <= (double)num3)
                        {
                            newDescription = Strings.ReplaceOptional(RegressionMod.data.Underwear_Messy[index].Replace("$UNDERWEAR_DESC$", newDescription), (double)wetPercent > 0.0);
                            break;
                        }
                    }
                }
                if ((double)wetPercent > 0.0)
                {
                    for (int index = 0; index < RegressionMod.data.Underwear_Wet.Length; ++index)
                    {
                        float num3 = (float)(((double)index + 1.0) / ((double)RegressionMod.data.Underwear_Wet.Length - 1.0));
                        if (index == RegressionMod.data.Underwear_Wet.Length - 1 || (double)wetPercent <= (double)num3)
                        {
                            string input = RegressionMod.data.Underwear_Wet[index].Replace("$UNDERWEAR_DESC$", newDescription);
                            Regex regex = new Regex("<([^>]*)>");
                            newDescription = (double)messyPercent != 0.0 ? regex.Replace(input, "$1") : regex.Replace(input, "");
                            break;
                        }
                    }
                }
            }
            return underwear.prefix.ToLower() + " " + newDescription;
        }
    }
}