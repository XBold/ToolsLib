using System.Net;
using static Tools.Network.NetworkConstants;

namespace Tools.Network
{
    public static class NetCheck
    {
        /// <summary>
        /// Check that port is in the range of valid ports (from <see cref="MinPort">1024</see> to <see cref="MaxPort">49151</see>).
        /// </summary>
        /// <param name="port">Port number that need to be checked</param>
        /// <returns>True if the port is in the range, false otherwise</returns>
        public static bool PortInRange(int port)
        {
            if (port > MinPort && port <= MaxPort)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check that the string IP in input is a valid address
        /// </summary>
        /// <param name="input">IP in string format</param>
        /// <returns>True if the IP is valid, false otherwise</returns>
        public static bool IsStandardIPAddress(string input)
        {
            if (IPAddress.TryParse(input, out IPAddress? address))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string[] parts = input.Split('.');
                    return parts.Length == 4 && parts.All(part => byte.TryParse(part, out _));
                }

                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    return input.Contains(':');
                }
            }

            return false;
        }
    }
}