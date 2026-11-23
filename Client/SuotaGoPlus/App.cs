using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Prism.Navigation;
using suota_pgp.Services;

namespace suota_pgp
{
    public class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider services)
        {
            Services = services;

            // Resolve long-lived singletons in dependency order so their
            // event-aggregator subscriptions are wired before the BLE layer
            // publishes its initial adapter state.
            services.GetRequiredService<IStateManager>();
            services.GetRequiredService<IBleManager>();
            services.GetRequiredService<IExtractorManager>();
            services.GetRequiredService<IFileManager>();
            services.GetRequiredService<ISuotaManager>();

            // Register navigation routes used by the view models.
            NavigationService.Register("SuotaView", () => services.GetRequiredService<SuotaView>());

            var mainPage = services.GetRequiredService<MainPage>();
            MainPage = new NavigationPage(mainPage);
        }
    }
}
