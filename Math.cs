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
        /// Limit the value between MIN and MAX parameters
        /// </summary>
        public static T Limit<T>(T min, T value, T max) where T : IComparable<T>
        {
            var (minOk, maxOk) = SortMinMax(min, max);
            return (value.CompareTo(minOk) < 0) ? minOk : (value.CompareTo(maxOk) > 0) ? maxOk : value;
        }

        /// <summary>
        /// TRUE in case the value is between MIN and MAX parameters
        /// </summary>
        public static bool IsInrange<T>(T value, T min, T max) where T : IComparable<T>
        {
            var (minOk, maxOk) = SortMinMax(min, max);
            return value.CompareTo(minOk) >= 0 && value.CompareTo(maxOk) <= 0;
        }

        /// <summary>
        /// Simply check that min is < than max, otherwise will swap the 2 values
        /// </summary>
        private static (T minOk, T maxOk) SortMinMax<T>(T min, T max) where T : IComparable<T>
        {
            if (min.CompareTo(max) > 0)
            {
                T temp = min;
                min = max;
                max = temp;
            }

            return (min, max);
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
