using Microsoft.Maui.Controls;

namespace suota_pgp
{
    public partial class DeviceInfoView : ContentPage
    {
        public DeviceInfoView(DeviceInfoViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
