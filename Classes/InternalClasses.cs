using Microsoft.VisualBasic;

namespace Tools.Classes
{
    internal class InternalClasses
    {
        internal static string CreateLog(string message, byte severity, byte numberOfMilliseconds = 2)
        {
            string format = "dd - MM - yyyy HH: mm: ss." + new string('f', Tools.Limit(numberOfMilliseconds, 1, 7));
            string sev = severity == 0 ? "[INFO]" : severity == 1 ? "[WARNING]" : severity == 2 ? "[CRITICAL]" : "[FATAL ERROR]";
            return sev + (numberOfMilliseconds > 0 ? DateAndTime.Now.ToString(format) : "") + message;
        }
    }
}
