using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class RNG
    {
        private static Random rng = new Random();

        public static int Next()
        {
            return rng.Next();
        }

        public static int Next(int max)
        {
            return rng.Next(max);
        }

        public static int Next(int min, int max)
        {
            return rng.Next(min,max);
        }

        public static double NextDouble()
        {
            return rng.NextDouble();
        }

        public static string PsudoGUID()
        {
            return rng.Next().ToString() + NextString(rng.Next(5,20)) + "." + NextString(rng.Next(9, 18)) + rng.Next().ToString();
        }

        private static string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string NextString (int size)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
                sb.Append(letters[rng.Next(letters.Length)]);

            return sb.ToString();
        }
    }
}
