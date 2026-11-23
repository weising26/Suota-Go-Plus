using Android.App;
using Android.Widget;
using Microsoft.Maui.ApplicationModel;

namespace suota_pgp.Droid.Services
{
    internal class NotifyManager : INotifyManager
    {
        public NotifyManager() { }

        private static Activity CurrentActivity => Platform.CurrentActivity;

        public void ShowLongToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = CurrentActivity;
                if (activity != null)
                    Toast.MakeText(activity, message, ToastLength.Long).Show();
            });
        }

        public void ShowShortToast(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = CurrentActivity;
                if (activity != null)
                    Toast.MakeText(activity, message, ToastLength.Short).Show();
            });
        }

        public void ShowDialogInfoBox(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = CurrentActivity;
                if (activity == null)
                    return;

                new AlertDialog.Builder(activity)
                    .SetMessage(message)
                    .SetPositiveButton("OK", (sender, e) => { })
                    .Show();
            });
        }

        public void ShowDialogErrorBox(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = CurrentActivity;
                if (activity == null)
                    return;

                new AlertDialog.Builder(activity)
                    .SetMessage(message)
                    .SetTitle("Error")
                    .SetPositiveButton("OK", (sender, e) => { })
                    .Show();
            });
        }
    }
}
