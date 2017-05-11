using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class TextTools
    {
        public static char ASCIIToHex(char c)
        {
            if (c >= '0' && c <= '9')
                return (char)((int)c - (int)'0');
            if (c >= 'A' && c <= 'F')
                return (char)((int)c - (int)'A' + 10);
            if (c >= 'a' && c <= 'f')
                return (char)((int)c - (int)'a' + 10);

            return char.MinValue;
        }
    }
}
