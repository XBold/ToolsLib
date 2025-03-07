﻿using System.IO;
namespace Tools.Logger
{
    public class Logger
    {
        public const byte defaultMiliseconds = 3;

        /// <summary>
        /// Add a line of log with the specified message
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity of the log, use <see cref="Severity"/> to specify the severity</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME. Parameter not specified = 3 millseconds unit</param>
        public static void LogConsole(string message, Severity severity, byte numberOfMilliseconds = defaultMiliseconds)
        {
            Console.WriteLine(CreateLog(message, severity, numberOfMilliseconds));
        }

        /// <summary>
        /// Add a line of log with the specified message to the specified file
        /// </summary>
        /// <param name="message">Message to insert inside log</param>
        /// <param name="severity">Severity of the log, use <see cref="Severity"/> to specify the severity</param>
        /// <param name="filePathAndName">Path and file name where to save log file. Parameters not specified = log.txt, same folder where program it's executed</param>
        /// <param name="numberOfMilliseconds">Number of milliseconds to use in the date and time. 0 = NO DATE AND TIME</param>
        /// <param name="logOnConsole">If true, log will be printed on console too</param>
        public static void Log(string message, Severity severity, string filePathAndName = "log.txt", byte numberOfMilliseconds = defaultMiliseconds, bool logOnConsole = true)
        {
            string log = CreateLog(message, severity, numberOfMilliseconds);
            if (logOnConsole)
            {
                Console.WriteLine(log);
            }
#if WPF
            try
            {
                File.AppendAllText(filePathAndName, log + Environment.NewLine);
            }
            catch (Exception ex)
            {
                LogConsole("EXCEPTION SAVING FILE LOG: " + ex.Message, Severity.FATAL_ERROR);
            }
#else
            LogConsole("FILE LOGGING NOT SUPPORTED IN THIS PLATFORM", Severity.FATAL_ERROR);
            throw new PlatformNotSupportedException("FILE LOGGING NOT SUPPORTED IN THIS PLATFORM");
#endif
        }

        protected static string CreateLog(string message, Severity severity, byte numberOfMilliseconds = defaultMiliseconds)
        {
            int digits = Math.Limit(numberOfMilliseconds, 1, 7);
            string format = "dd-MM-yyyy HH:mm:ss:" + new string('f', digits) + " ";

            string sev = severity == Severity.INFO ? "[INFO]" :
                         severity == Severity.WARNING ? "[WARNING]" :
                         severity == Severity.CRITICAL ? "[CRITICAL]" :
                         severity == Severity.FATAL_ERROR ? "[FATAL ERROR]" :
                         "[NO SEVERITY SPECIFIED!]";
            sev += " ";

            return sev + (numberOfMilliseconds > 0 ? DateTime.Now.ToString(format) : "") + message;
        }

    }
}
