namespace Tools.Network
{
    public class IP
    {
        public byte[] Octets { get; private set; }
        private bool error;

        /// <summary>
        /// Convert a string to an IP address
        /// </summary>
        /// <param name="ipAddress">IP addres in string format</param>
        public IP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                Logger.Log("IP address is null or string is empty", Logger.Severity.WARNING);
                error = true;
            }

            if (error)
            {
                Octets = [];
            }
            else
            {
                Octets = ParseIPAddress(ipAddress);
            }
        }

        private byte[] ParseIPAddress(string ipAddress)
        {
            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
            {
                Logger.Log("Not possible to find the octet, there are more thant 4 parts divided by '.'", Logger.Severity.WARNING);
                error = true;
                return null;
            }

            return parts.Select(part =>
            {
                if (byte.TryParse(part, out var octet))
                {
                    return octet;
                }
                else
                {
                    Logger.Log("Not possible to parse the found part in the octet", Logger.Severity.CRITICAL);
                    error = true;
                    return (byte)0;
                }
            }).ToArray();
        }

        /// <summary>
        /// Convert the "IP" to string
        /// </summary>
        public override string ToString()
        {
            if (error)
            {
                Octets = [];
                return string.Empty;
            }
            return string.Join(".", Octets);
        }

        /// <summary>
        /// Check if the ip is in the subnet. Return false if the IP is not in the subnet or if the subnet and subnet mask are not valid
        /// </summary>
        public bool IsInSubnet(string subnet, string subnetMask)
        {
            var subnetBytes = ParseIPAddress(subnet);
            var maskBytes = ParseIPAddress(subnetMask);

            if (subnetBytes != null && maskBytes != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((Octets[i] & maskBytes[i]) != (subnetBytes[i] & maskBytes[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetBroadcastAddress(string subnetMask)
        {
            var maskBytes = ParseIPAddress(subnetMask);
            var broadcastBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                broadcastBytes[i] = (byte)(Octets[i] | ~maskBytes[i]);
            }

            return string.Join(".", broadcastBytes);
        }

        public string GetNetworkAddress(string subnetMask)
        {
            var maskBytes = ParseIPAddress(subnetMask);
            var networkBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(Octets[i] & maskBytes[i]);
            }

            return string.Join(".", networkBytes);
        }

        /// <summary>
        /// Add the requested value to the last octet of the IP, in case the value will be too big it will not increment the octet before
        /// </summary>
        public void Increment(uint value)
        {
            if (error)
            {
                return;
            }
            if (Octets[3] + value > 255)
            {
                Logger.Log("Not possible to increment the IP, final value is bigger than 255", Logger.Severity.WARNING);
                error = true;
                return;
            }
            else
            {
                Octets[3] = (byte)(Octets[3] + value);
            }
        }
    }
}
