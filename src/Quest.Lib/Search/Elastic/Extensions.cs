namespace Quest.Lib.Search.Elastic
{
    public static class Extensions
    {
        /// <summary>
        /// Remove text between two delimiter
        /// </summary>
        /// <param name="text">the string to modify</param>
        /// <param name="left">left marker</param>
        /// <param name="right">right merker</param>
        /// <returns></returns>
        public static string RemoveBetween(this string text, char left, char right)
        {
            var p1 = text.IndexOf(left);
            if (p1 >= 0)
            {
                var p2 = text.IndexOf(right);
                if (p2 > p1)
                {
                    var s = text.Remove(p1, p2 - p1 + 1);
                    return s;
                }
            }
            return text;
        }


    }
}
