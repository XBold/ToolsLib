using System.IO;
namespace Tools.Logger
{
    public static class Logger
    {
        /// <summary>
        /// Add a line of log with the specified message
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity of the log, use <see cref="Severity"/> to specify the severity</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME. Parameter not specified = 3 millseconds unit</param>
        public static void LogConsole(string message, Severity severity, byte numberOfMilliseconds = 3)
        {
            Console.WriteLine(CreateLog(message, severity, numberOfMilliseconds));
        }

        /// <summary>
        /// Add a line of log with the specified message to the specified file
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity of the log, use <see cref="Severity"/> to specify the severity</param>
        /// <param name="filePathAndName">Path and file name where to save log file. Parameters not specified = log.txt, same folder where program it's executed</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME. Parameter not specified = 3 millseconds unit</param>
        public static void Log(string message, Severity severity, string filePathAndName = "log.txt", byte numberOfMilliseconds = 3)
        {
            string log = CreateLog(message, severity, numberOfMilliseconds);
            Console.WriteLine(log);
            try
            {
                File.AppendAllText(filePathAndName, log);
            }
            catch (Exception ex)
            {
                LogConsole("EXCEPTION SAVING FILE LOG: " + ex.Message, Severity.FATAL_ERROR);
            }
        }

        private static string CreateLog(string message, Severity severity, byte numberOfMilliseconds = 3)
        {
            string format = "dd-MM-yyyy HH:mm:ss:" + new string('f', Math.Limit(numberOfMilliseconds, 1, 7));
            format += " ";
            string sev = severity == Severity.INFO ? "[INFO]" : severity == Severity.WARNING ? "[WARNING]" : severity == Severity.CRITICAL ? "[CRITICAL]" : severity == Severity.FATAL_ERROR ? "[FATAL ERROR]" : "[NO SEVERITY SPECIFIED!]";
            sev += " ";
            return sev + (numberOfMilliseconds > 0 ? DateTime.Now.ToString(format) : "") + message;
        }
    }
}
