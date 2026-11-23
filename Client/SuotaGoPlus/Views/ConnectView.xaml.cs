using Microsoft.Maui.Controls;
using Prism.Navigation;

namespace suota_pgp
{
    public partial class ConnectView : ContentPage
    {
        public ConnectView(ConnectViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as INavigationAware)?.OnNavigatedTo(null);
        }
    }
}
