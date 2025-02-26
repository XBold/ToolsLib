using System.Net;
#if ANDROID  
using Android.Net.Wifi;
using Android.Content;
using Android.App;
#endif

namespace Tools.Network
{
    class Net
    {
        private (int signal, string ipAddress) WiFiData()
        {
#if ANDROID
            try
            {
                var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                var info = wifiManager.ConnectionInfo;
                int ipAddressRaw = info.IpAddress;
                string ip = "";
                try
                {
                    ip = string.Format(
                        "{0}.{1}.{2}.{3}",
                        (ipAddressRaw & 0xff),
                        (ipAddressRaw >> 8 & 0xff),
                        (ipAddressRaw >> 16 & 0xff),
                        (ipAddressRaw >> 24 & 0xff));
                }
                catch (Exception ex)
                {
                    Logger.Log("Error while formatting the IP address", 2);
                    ip = string.Empty;
                }

                return (WifiManager.CalculateSignalLevel(info.Rssi, 101), ip);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while getting signal strength. Error: {ex.Message}", 2);
                return (0, "");
            }
#else
                return (0, "");
#endif
        }
    }
}
