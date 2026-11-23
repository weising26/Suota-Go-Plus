using Microsoft.Maui.Controls;

namespace suota_pgp
{
    public class MainPage : TabbedPage
    {
        public MainPage(ConnectView connectView, DeviceInfoView deviceInfoView)
        {
            Title = "Pokemon GO Plus Key Extractor";
            Children.Add(connectView);
            Children.Add(deviceInfoView);
        }
    }
}
