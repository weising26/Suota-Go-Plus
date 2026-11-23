using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
using Prism.Logging;
using Prism.Navigation;
using suota_pgp.Droid.Services;
using suota_pgp.Services;

namespace suota_pgp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            var s = builder.Services;

            // Infrastructure / Prism-compat singletons.
            s.AddSingleton<IEventAggregator, EventAggregator>();
            s.AddSingleton<ILoggerFacade, DebugLogger>();
            s.AddSingleton<INavigationService, NavigationService>();

            // Plugin.BLE.
            s.AddSingleton<IBluetoothLE>(_ => CrossBluetoothLE.Current);

            // App services (Android implementations).
            s.AddSingleton<IStateManager, StateManager>();
            s.AddSingleton<INotifyManager, NotifyManager>();
            s.AddSingleton<IBleManager, BleManager>();
            s.AddSingleton<IFileManager, FileManager>();
            s.AddSingleton<IExtractorManager, ExtractorManager>();
            s.AddSingleton<ISuotaManager, SuotaManager>();

            // Pages + view models.
            s.AddTransient<ConnectViewModel>();
            s.AddTransient<DeviceInfoViewModel>();
            s.AddTransient<SuotaViewModel>();
            s.AddTransient<ConnectView>();
            s.AddTransient<DeviceInfoView>();
            s.AddTransient<SuotaView>();
            s.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
