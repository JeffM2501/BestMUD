using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Telnet
{
    public static class TelnetColors
    {
        private static readonly string codePrefix = "\x1b";

        public static readonly string reset = codePrefix + "[0m";
        public static readonly string bold = codePrefix + "[1m";
        public static readonly string dim = codePrefix + "[2m";
        public static readonly string under = codePrefix + "[4m";
        public static readonly string reverse = codePrefix + "[7m";
        public static readonly string hide = codePrefix + "[8m";

        public static readonly string clearscreen = codePrefix + "[2J";
        public static readonly string clearline = codePrefix + "[2K";

        public static readonly string black = codePrefix + "[30m";
        public static readonly string red = codePrefix + "[31m";
        public static readonly string green = codePrefix + "[32m";
        public static readonly string yellow = codePrefix + "[33m";
        public static readonly string blue = codePrefix + "[34m";
        public static readonly string magenta = codePrefix + "[35m";
        public static readonly string cyan = codePrefix + "[36m";
        public static readonly string white = codePrefix + "[37m";

        public static readonly string bblack = codePrefix + "[40m";
        public static readonly string bred = codePrefix + "[41m";
        public static readonly string bgreen = codePrefix + "[42m";
        public static readonly string byellow = codePrefix + "[43m";
        public static readonly string bblue = codePrefix + "[44m";
        public static readonly string bmagenta = codePrefix + "[45m";
        public static readonly string bcyan = codePrefix + "[46m";
        public static readonly string bwhite = codePrefix + "[47m";

        public static readonly string newline = "\r\n" + codePrefix + "[0m";


        private static string[,,] ColorCache = new string[3,3,3];

        private static Dictionary<string, string> NamedColorCache = new Dictionary<string, string>();

        static TelnetColors()
        {
            // Due to the lack of depth of telnet colors (only 15), not all colors
            // can be represented. Colors that are not exact have a * after them,
            // and colors that are very different have ** after them.
            // For example, [2][1][0] is really supposed to be orange, but there is
            // no way to represent orange in telnet, so I used what I thought to be
            // the closest equivalent, dim yellow. Another example is [0][1][2],
            // which is a really cool seagreen-blue color, but the closest there is
            // in telnet would be bright blue.
            ColorCache[0,0,0] = black + dim;
            ColorCache[0,0,1] = blue + dim;
            ColorCache[0,0,2] = blue + bold;
            ColorCache[0,1,0] = green + dim;
            ColorCache[0,1,1] = cyan + dim;
            ColorCache[0,1,2] = blue + bold;              // *
            ColorCache[0,2,0] = green + bold;
            ColorCache[0,2,1] = green + bold;             // *
            ColorCache[0,2,2] = cyan + bold;

            ColorCache[1,0,0] = red + dim;
            ColorCache[1,0,1] = magenta + dim;
            ColorCache[1,0,2] = magenta + bold;           // *
            ColorCache[1,1,0] = yellow + dim;
            ColorCache[1,1,1] = white + dim;
            ColorCache[1,1,2] = blue + bold;              // *
            ColorCache[1,2,0] = green + bold;             // *
            ColorCache[1,2,1] = green + bold;             // *
            ColorCache[1,2,2] = cyan + bold;              // *

            ColorCache[2,0,0] = red + bold;
            ColorCache[2,0,1] = red + bold;               // *
            ColorCache[2,0,2] = magenta + bold;
            ColorCache[2,1,0] = yellow + dim;             // **
            ColorCache[2,1,1] = red + bold;               // **
            ColorCache[2,1,2] = magenta + bold;           // *
            ColorCache[2,2,0] = yellow + bold;
            ColorCache[2,2,1] = yellow + bold;            // *
            ColorCache[2,2,2] = white + bold;

            // set the named color codes
            NamedColorCache.Add("$black", black);
            NamedColorCache.Add("$red", red);
            NamedColorCache.Add("$yellow", yellow);
            NamedColorCache.Add("$green", green);
            NamedColorCache.Add("$blue", blue);
            NamedColorCache.Add("$magenta", magenta);
            NamedColorCache.Add("$cyan", cyan);
            NamedColorCache.Add("$white", white);
            NamedColorCache.Add("$bold", bold);
            NamedColorCache.Add("$dim", dim);
            NamedColorCache.Add("$reset", reset);
        }

        public static string TranslateBMCode(string input)
        {
            if (input[0] != '<' || input[input.Length - 1] != '>' || input.Length <= 2)
                return string.Empty;

            string nubs = input.Substring(1, input.Length - 2).ToLowerInvariant();
            if (NamedColorCache.ContainsKey(nubs))
                return NamedColorCache[nubs];
            else
                return TranslateNumberColor(nubs);
        }

    
        public static string TranslateNumberColor(string tag)
        {
            string input = tag.Substring(1);

            if (input.Length < 6)
                return string.Empty;

            int r = TextTools.ASCIIToHex(input[0]) * 16 + TextTools.ASCIIToHex(input[1]);
            int g = TextTools.ASCIIToHex(input[2]) * 16 + TextTools.ASCIIToHex(input[3]);
            int b = TextTools.ASCIIToHex(input[4]) * 16 + TextTools.ASCIIToHex(input[5]);

            // convert the numbers to the 0-2 range
            // ie:  0 -  85 = 0
            //     86 - 171 = 1
            //    172 - 255 = 2
            // This gives a good approximation of the true color by assigning equal
            // ranges to each value.
            r = r / 86;
            g = g / 86;
            b = b / 86;

            if (r > 2 || g > 2 || b > 2)
                return string.Empty;

            return ColorCache[r,g,b];
        }
    }
}
