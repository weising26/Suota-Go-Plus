using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Prism.Events;
using suota_pgp.Droid;

namespace suota_pgp
{
    [Activity(Label = "Suota Go+",
              Icon = "@mipmap/appicon",
              Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                                     ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                                     ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            RequestAppPermissions();
            PublishPermissionState();
        }

        private void RequestAppPermissions()
        {
            if (!AndroidPermissions.HasBluetooth() || !AndroidPermissions.HasStorage())
            {
                AndroidPermissions.RequestAll(this);
            }
        }

        private static void PublishPermissionState()
        {
            var aggregator = App.Services?.GetService<IEventAggregator>();
            if (aggregator == null)
                return;

            var state = AndroidPermissions.CurrentState();
            aggregator.GetEvent<PrismEvents.PermissionStateChangedEvent>().Publish(state);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PublishPermissionState();
        }
    }
}
