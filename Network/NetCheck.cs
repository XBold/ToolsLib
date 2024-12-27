namespace Tools.Network
{
    public static class NetCheck
    {
        public static bool PortInRange(int port)
        {
            if (port > 1020 && port <= 65535)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
