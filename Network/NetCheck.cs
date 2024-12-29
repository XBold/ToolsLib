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
    }
}
