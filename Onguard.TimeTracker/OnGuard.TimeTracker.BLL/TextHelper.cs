using System.Text.RegularExpressions;

namespace Onguard.TimeTracker.BLL
{
    public static class TextHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string RemoveTextBetweenBrackets(string s, char begin, char end)
        {
            var regex = new Regex($"\\{begin}.*?\\{end}");
            return regex.Replace(s, string.Empty);
        }
    }
}