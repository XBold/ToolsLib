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
        /// Simply check that min is &lt; than max, otherwise will swap the 2 values
        /// </summary>
        public static (T minOk, T maxOk) SortMinMax<T>(T min, T max) where T : IComparable<T>
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
        /// <param name="acceptDots">TRUE -&gt; The input text can containt numbers AND dots</param>
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

        //IMPORTED MANUALLY FROM SYSTEM.MATH
        public static double Abs(double value) => System.Math.Abs(value);
        public static float Abs(float value) => System.Math.Abs(value);
        public static int Abs(int value) => System.Math.Abs(value);
        public static double Acos(double d) => System.Math.Acos(d);
        public static double Asin(double d) => System.Math.Asin(d);
        public static double Atan(double d) => System.Math.Atan(d);
        public static double Atan2(double y, double x) => System.Math.Atan2(y, x);
        public static double Ceiling(double a) => System.Math.Ceiling(a);
        public static double Cos(double d) => System.Math.Cos(d);
        public static double Cosh(double value) => System.Math.Cosh(value);
        public static double Exp(double d) => System.Math.Exp(d);
        public static double Floor(double d) => System.Math.Floor(d);
        public static double Log(double d) => System.Math.Log(d);
        public static double Log10(double d) => System.Math.Log10(d);
        public static double Max(double val1, double val2) => System.Math.Max(val1, val2);
        public static int Max(int val1, int val2) => System.Math.Max(val1, val2);
        public static double Min(double val1, double val2) => System.Math.Min(val1, val2);
        public static int Min(int val1, int val2) => System.Math.Min(val1, val2);
        public static double Pow(double x, double y) => System.Math.Pow(x, y);
        public static double Round(double a) => System.Math.Round(a);
        public static double Round(double value, int digits) => System.Math.Round(value, digits);
        public static int Sign(double value) => System.Math.Sign(value);
        public static double Sin(double a) => System.Math.Sin(a);
        public static double Sqrt(double d) => System.Math.Sqrt(d);
        public static double Tan(double a) => System.Math.Tan(a);
        public static double Truncate(double d) => System.Math.Truncate(d);
    }
}
