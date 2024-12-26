using Tools.Classes;

namespace Tools
{
    public static class Tools
    {
        /// <summary>
        /// Add a line of log with the specified message
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity to use 0 = [INFO], 1 = [WARNING], 2 = [CRITICAL], 3..255 = [FATAL ERROR]</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME. Parameter not specified = 2 millseconds unit</param>
        public static void Log(string message, byte severity, byte numberOfMilliseconds = 2)
        {
            System.Diagnostics.Debug.WriteLine(InternalClasses.CreateLog(message, severity, numberOfMilliseconds));
        }

        /// <summary>
        /// Add a line of log with the specified message to the specified file
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity to use 0 = [INFO], 1 = [WARNING], 2 = [CRITICAL], 3..255 = [FATAL ERROR]</param>
        /// <param name="filePathAndName">Path and file name where to save log file. Parameters not specified = log.txt, same folder where program it's executed</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME. Parameter not specified = 2 millseconds unit</param>
        public static void Log(string message, byte severity, string filePathAndName, byte numberOfMilliseconds = 2)
        {
            File.AppendAllText(filePathAndName, InternalClasses.CreateLog(message, severity, numberOfMilliseconds));
        }

        /// <summary>
        /// Limit the value between the MIN parameter and the MAX parameter
        /// </summary>
        public static T Limit<T>(T value, T min, T max) where T : IComparable<T>
        {
            return (value.CompareTo(min) < 0) ? min : (value.CompareTo(max) > 0) ? max : value;
        }
    }
}
