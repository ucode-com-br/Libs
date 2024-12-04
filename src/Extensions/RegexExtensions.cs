using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for working with regular expressions.
    /// </summary>
    /// <remarks>
    /// This class contains methods that enhance the functionality of the regular expression 
    /// processing in .NET by providing additional utilities and convenience methods.
    /// </remarks>
    public static class RegexExtensions
    {


        /// <summary>
        /// Filters the given input string using the specified regex pattern.
        /// This method will return a concatenated string of all captures found in the input
        /// that match the provided regex pattern.
        /// </summary>
        /// <param name="pattern">
        /// The regex pattern to use for filtering the input string.
        /// </param>
        /// <param name="input">
        /// The input string from which to extract values based on the regex pattern.
        /// </param>
        /// <returns>
        /// A string containing all the captured values that match the regex pattern,
        /// concatenated together. If no matches are found, an empty string is returned.
        /// </returns>
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

        /// <summary>
        /// Matches named captures in a regular expression pattern against a specified input string.
        /// </summary>
        /// <param name="pattern">A string representing the regular expression pattern that contains named captures.</param>
        /// <param name="input">The input string to be matched against the regular expression pattern.</param>
        /// <returns>
        /// A dictionary containing the named captures as key-value pairs, 
        /// where the key is the name of the capture group and the value is the corresponding substring from the input.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when either the <paramref name="pattern"/> or <paramref name="input"/> is null.
        /// </exception>
        /// <example>
        /// <code>
        /// var pattern = @"(?<name>\w+)";
        // string input = "Hello, my name is John.";
        // var captures = input.MatchNamedCaptures(pattern);
        /// </code>
        /// The <c>captures</c> dictionary will contain "name" as key and "John" as its value.
        /// </example>
        public static Dictionary<string, string> MatchNamedCaptures(this string pattern, string input) => MatchNamedCaptures(new System.Text.RegularExpressions.Regex(pattern), input);

        /// <summary>
        /// Extension method that matches a regular expression against a given input string
        /// and retrieves named captures as a dictionary.
        /// </summary>
        /// <param name="regex">
        /// The regular expression to match against the input string.
        /// </param>
        /// <param name="input">
        /// The input string to match the regular expression against.
        /// </param>
        /// <returns>
        /// A dictionary containing named capture groups and their corresponding values
        /// from the regex match. The keys are the names of the capture groups, and the values
        /// are the matched strings.
        /// </returns>
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
