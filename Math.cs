using System.Text.RegularExpressions;

namespace Tools
{
    public static partial class Math
    {
        [GeneratedRegex("[^0-9.]+")]
        private static partial Regex NumericAndDots();
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex Numeric();

        /// <summary>
        /// Limit the value between the MIN parameter and the MAX parameter
        /// </summary>
        public static T Limit<T>(T value, T min, T max) where T : IComparable<T>
        {
            return (value.CompareTo(min) < 0) ? min : (value.CompareTo(max) > 0) ? max : value;
        }

        /// <summary>
        /// Check that the input text contains only number or number with dots
        /// </summary>
        /// <param name="acceptDots">TRUE -> The input text can containt numbers AND dots</param>
        public static bool IsTextNumeric(string text, bool acceptDots = false)
        {
            if (acceptDots)
            {
                Regex regex = NumericAndDots();
                return !regex.IsMatch(text);
            }
            else
            {
                Regex regex = Numeric();
                return !regex.IsMatch(text);
            }
        }

    }
}
