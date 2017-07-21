using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WindowsFormsApp1.Tools.Extensions;

namespace WindowsFormsApp1.Tools {
    internal static class HtmlUtilities 
	{
		public static void SetHTMLAttribute(HtmlDocument document, string attribName, string fieldName, string set) 
		{
			HtmlElementCollection elements = document.GetElementsByTagName("input");
			foreach (HtmlElement inputElement in elements) 
			{
				String nameStr = inputElement.GetAttribute(attribName);
				if (nameStr == fieldName) 
				{
					document.GetElementById(nameStr)?.SetAttribute("value", set);
				}
			}
		}

		public static void ClickHTMLAttribute(HtmlDocument document, string attribName, string fieldName)
		{
			var submitElement = GetElement(document, "input" ,attribName, fieldName);
			submitElement.InvokeMember("click");
		}

		public static string GetHTMLAttributeValue(HtmlDocument document, string tag, string attribName, string attribValue) 
		{
			var value = GetElement(document, tag, attribName, attribValue);
			return value.InnerText;
		}

		private static HtmlElement GetElement(HtmlDocument document,string tag, string attribName, string fieldName) 
		{
			HtmlElementCollection elements = document.GetElementsByTagName(tag);
			foreach (HtmlElement inputElement in elements) {
				String nameStr = inputElement.GetAttribute(attribName);
				if (nameStr == fieldName) {
					return inputElement;
				}
			 }
			return null;
		}

		public static string[] GetAccountInfoFromHtmlString(string html)
		{
		    string result = string.Empty;
            MatchCollection mc = Regex.Matches(html, @"User\.Parameters = {.*};", RegexOptions.None);
            //ID LVL HONOR EXP
		    MatchCollection mc2 = Regex.Matches(html, @"<span>.*</span>");
            string credits = null, uridium = null, Level = null, Exp = null, Honor = null, Id = null;
		    int o = 0;
		    foreach (Match match in mc2)
		    {
		        string res = match.Value.Replace(@"<span>", "");
                res = res.Replace(@"</span>", "");
                res = res.Replace(@".", "");
		        result += $"{res}|";
		        o++;
		    }

            foreach (Match match in mc) 
			{
			    var strings = match.Value.Split(new[] { ":", "}", ",", "{"}, StringSplitOptions.None);
			    int counter = 0;
			    foreach (string s in strings)
			    {
                    if(s.Contains("uridium") || s.Contains("credits"))
                        result += $"{strings[counter + 1]}|";
			        counter++;
			    }
			}
		    var tempRes = result.Split('|');

            for (int i = 0; i < tempRes.Length; i++)
            {
                tempRes[i] = tempRes[i].Replace("|", "");
                tempRes[i] = tempRes[i].Trim();
            }
		    return tempRes;
		}

		public static string[] GetGatesInfoFromHtmlString(string html)
		{
			string result = string.Empty;
			MatchCollection currentPartsMatches = Regex.Matches(html, " current=\".*\"");
		    bool isKronos = false;
			foreach (Match match in currentPartsMatches)
			{
			    if (isKronos) {
			        isKronos = false;
                    continue;
			    }
                //If is Kronos just ignore it
			    if (match.Value.Contains(" id=\"12\"")) {
			        isKronos = true;
			        continue;
			    }

			    string tempRes = match.Value.Substring(" current=\"", "\"");
			    result += $"{tempRes}|";
			}

		    return result.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
		}


        public static string[] GetSpinRewards(string html)
        {
            string result = string.Empty;
			Console.WriteLine(html);
            MatchCollection rewardsCollection = Regex.Matches(html, "<item type=.*/>");
			Console.WriteLine(rewardsCollection.Count);
            foreach (Match match in rewardsCollection)
            {
                //<item type="ore" item_id="4" spins="1" amount="10" date="1499712033" />
                string type = match.Value.Substring("<item type=\"", "\" ");
                string ammount = match.Value.Substring(" amount=\"", "\" ");
                string line = $"[{type.ToUpper()}] Ammount: {ammount}";
                result += $"{line}|";
            }
	        Console.WriteLine(result);
            return result.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
        }

		/*ALPHA   <gate total = "34"  current="0"  id="1"   prepared="0" totalWave="40" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      BETA    <gate total = "48"  current="3"  id="2"   prepared="0" totalWave="40" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      GAMMA   <gate total = "82"  current="0"  id="3"   prepared="0" totalWave="40" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      DELTA   <gate total = "128" current="1"  id="4"   prepared="0" totalWave="29" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      EPSILON <gate total = "99"  current="1"  id="5"   prepared="0" totalWave="30" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      ZETA    <gate total = "111" current="2"  id="6"   prepared="0" totalWave="46" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      KAPPA   <gate total = "120" current="67" id="7"   prepared="0" totalWave="29" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      LAMBDA  <gate total = "45"  current="4"  id="8"   prepared="0" totalWave="21" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      
	      KRONOS  <gate total = "21"  current="14" id="12" prepared="0" totalWave="50" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" >
	      <gatebuilders name = "Alpha" current="4" total="4" />
	      <gatebuilders name = "Beta" current="2" total="3" />
	      <gatebuilders name = "Gamma" current="1" total="1" />
	      <gatebuilders name = "Delta" current="0" total="1" />
	      <gatebuilders name = "Epsilon" current="1" total="4" />
	      <gatebuilders name = "Zeta" current="1" total="1" />
	      <gatebuilders name = "Kappa" current="2" total="2" />
	      <gatebuilders name = "Lambda" current="3" total="5" />
	      </gate>
	      
	      HADES  <gate total = "45"  current="0" id="13"  prepared="0" totalWave="12" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
	      KUIPER <gate total = "100" current="0" id="19"  prepared="0" totalWave="54" currentWave="0" state="in_progress" livesLeft="-1" lifePrice="-1" />
		*/
	}
}
