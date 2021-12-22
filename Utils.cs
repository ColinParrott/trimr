using System;
using System.Globalization;

namespace trimr
{
    class Utils
    {
        private static readonly CultureInfo cultureInfo = new("en-GB");

        public static string MsToTimeString(double ms)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(ms);
            return ts.ToString(@"mm\:ss\:fff", cultureInfo);
        }
    }
}
