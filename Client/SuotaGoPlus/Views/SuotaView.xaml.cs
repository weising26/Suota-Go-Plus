using Microsoft.Maui.Controls;

namespace suota_pgp
{
    public partial class SuotaView : ContentPage
    {
        public SuotaView(SuotaViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
