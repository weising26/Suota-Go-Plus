using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Prism.Navigation
{
    /// <summary>
    /// Lightweight navigation-parameter bag.
    /// </summary>
    public interface INavigationParameters
    {
        bool TryGetValue<T>(string key, out T value);
        T GetValue<T>(string key);
        object this[string key] { get; }
    }

    public class NavigationParameters : INavigationParameters
    {
        private readonly Dictionary<string, object> _items = new();

        public void Add(string key, object value) => _items[key] = value;

        public object this[string key] => _items.TryGetValue(key, out var v) ? v : null;

        public bool TryGetValue<T>(string key, out T value)
        {
            if (_items.TryGetValue(key, out var raw) && raw is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }

        public T GetValue<T>(string key) => TryGetValue<T>(key, out var v) ? v : default;
    }

    public interface INavigatedAware
    {
        void OnNavigatedFrom(INavigationParameters parameters);
        void OnNavigatedTo(INavigationParameters parameters);
    }

    public interface INavigationAware : INavigatedAware { }

    public interface INavigatingAware
    {
        void OnNavigatingTo(INavigationParameters parameters);
    }

    public interface INavigationService
    {
        Task NavigateAsync(string name);
        Task NavigateAsync(string name, INavigationParameters parameters);
        Task GoBackAsync();
        Task GoBackAsync(INavigationParameters parameters);
    }

    /// <summary>
    /// Maps page keys registered at startup to a factory, and pushes them onto
    /// the application's current navigation stack. Replaces Prism's URI
    /// navigation for the handful of transitions this app performs.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private static readonly Dictionary<string, Func<Page>> _routes = new();

        public static void Register(string name, Func<Page> factory) => _routes[name] = factory;

        public Task NavigateAsync(string name) => NavigateAsync(name, null);

        public async Task NavigateAsync(string name, INavigationParameters parameters)
        {
            if (!_routes.TryGetValue(name, out var factory))
                throw new InvalidOperationException($"No navigation route registered for '{name}'.");

            var page = factory();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var nav = Application.Current?.MainPage?.Navigation;
                if (nav == null)
                    return;

                await nav.PushAsync(page);

                if (page.BindingContext is INavigatedAware aware)
                    aware.OnNavigatedTo(parameters);
            });
        }

        public Task GoBackAsync() => GoBackAsync(null);

        public async Task GoBackAsync(INavigationParameters parameters)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var nav = Application.Current?.MainPage?.Navigation;
                if (nav != null && nav.NavigationStack.Count > 1)
                    await nav.PopAsync();
            });
        }
    }
}
