using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UCode.Extensions
{
    /// <summary>
    /// Regex Exttension
    /// </summary>
    public static class RegexExtensions
    {
        //public static Dictionary<string, string> RegexMatchGroups(this string pattern, string source)
        //{
        //    var result = new Dictionary<string, string>();

        //    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

        //    System.Text.RegularExpressions.Match match = regex.Match(source);

        //    if (match.Success)
        //    {
        //        string[] groupNames = regex.GetGroupNames();
        //        foreach (var name in groupNames)
        //        {
        //            Group grp = match.Groups[name];
        //            result.Add(name, grp.Value);
        //        }
        //    }

        //    return result;
        //}

        public static string RegexFilter(this string pattern, string input)
        {
            var result = "";

            var regex = new System.Text.RegularExpressions.Regex(pattern);

            foreach (var match in regex.Matches(input).Cast<Match>())
            {
                foreach (var capture in match.Captures.Cast<Capture>())
                {
                    result += capture.Value;
                }
            }

            return result;
        }

        public static Dictionary<string, string> MatchNamedCaptures(this string pattern, string input) => MatchNamedCaptures(new System.Text.RegularExpressions.Regex(pattern), input);

        public static Dictionary<string, string> MatchNamedCaptures(this System.Text.RegularExpressions.Regex regex,
            string input)
        {
            var namedCaptureDictionary = new Dictionary<string, string>();
            var groups = regex.Match(input).Groups;
            var groupNames = regex.GetGroupNames();
            foreach (var groupName in groupNames)
            {
                if (groups[groupName].Captures.Count > 0)
                {
                    namedCaptureDictionary.Add(groupName, groups[groupName].Value);
                }
            }

            return namedCaptureDictionary;
        }
    }
}
