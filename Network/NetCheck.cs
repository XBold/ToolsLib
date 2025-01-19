using System.Net;

namespace Tools.Network
{
    public static class NetCheck
    {
        public static bool PortInRange(int port)
        {
            if (port > NetworkConstants.MinPort && port <= NetworkConstants.MaxPort)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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
                    return input.Contains(":");
                }
            }

            return false;
        }
    }
}
