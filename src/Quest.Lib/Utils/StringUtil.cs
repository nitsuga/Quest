using System;
using System.Linq;
using System.Text;

namespace Quest.Lib.Utils
{
    public static class StringUtil
    {
        /// <summary>
        /// Decompound a list of words
        /// </summary>
        /// <param name="textLine"></param>
        /// <param name="wordlist">list of word endings, ordered by longest first</param>
        /// <returns></returns>
        public static string Decompound(this string textLine, string[] wordlist)
        {
            var result = new StringBuilder();
            var parts = textLine.Split(new char[] { ',', ' ', '-' }).ToList();
            foreach (var text in parts)
            {
                result.Append(" " + text);
                foreach (var word in wordlist)
                {
                    if (text.EndsWith(word, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // split the word into two
                        var leftPart = text.Substring(0, text.Length - word.Length);
                        result.Append(" " + leftPart);
                        result.Append(" " + word);
                        break;
                    }

                }
            }
            return result.ToString();
        }
    }
}
