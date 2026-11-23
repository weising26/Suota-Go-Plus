using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace suota_pgp.Droid
{
    /// <summary>
    /// Centralises the Android runtime-permission logic for BLE (Android 12+
    /// split permissions) and "All files access" storage (Android 11+).
    /// </summary>
    internal static class AndroidPermissions
    {
        internal const int RequestCode = 9001;

        private static Context Context => Android.App.Application.Context;

        private static bool IsGranted(string permission) =>
            ContextCompat.CheckSelfPermission(Context, permission) == Permission.Granted;

        /// <summary>
        /// True when the app can scan/connect over BLE given the OS version.
        /// </summary>
        public static bool HasBluetooth()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+
            {
                return IsGranted(Android.Manifest.Permission.BluetoothScan) &&
                       IsGranted(Android.Manifest.Permission.BluetoothConnect);
            }

            // Pre-12: scanning requires fine location.
            return IsGranted(Android.Manifest.Permission.AccessFineLocation);
        }

        public static Task<bool> HasBluetoothAsync() => Task.FromResult(HasBluetooth());

        /// <summary>
        /// True when the app can read/write the shared SuotaPgp folder.
        /// </summary>
        public static bool HasStorage()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // Android 11+
            {
                return Android.OS.Environment.IsExternalStorageManager;
            }

            return IsGranted(Android.Manifest.Permission.WriteExternalStorage);
        }

        /// <summary>
        /// Requests the runtime permissions appropriate for the OS version.
        /// Storage ("All files access") must be granted in Settings, so this
        /// opens that screen when needed.
        /// </summary>
        public static void RequestAll(Activity activity)
        {
            var toRequest = new System.Collections.Generic.List<string>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                if (!IsGranted(Android.Manifest.Permission.BluetoothScan))
                    toRequest.Add(Android.Manifest.Permission.BluetoothScan);
                if (!IsGranted(Android.Manifest.Permission.BluetoothConnect))
                    toRequest.Add(Android.Manifest.Permission.BluetoothConnect);
            }

            // Fine location still helps discovery on every version and is
            // required for scanning below Android 12.
            if (!IsGranted(Android.Manifest.Permission.AccessFineLocation))
                toRequest.Add(Android.Manifest.Permission.AccessFineLocation);

            if (Build.VERSION.SdkInt < BuildVersionCodes.R &&
                !IsGranted(Android.Manifest.Permission.WriteExternalStorage))
            {
                toRequest.Add(Android.Manifest.Permission.WriteExternalStorage);
            }

            if (toRequest.Count > 0)
            {
                ActivityCompat.RequestPermissions(activity, toRequest.ToArray(), RequestCode);
            }

            // Android 11+ needs the user to grant "All files access" in Settings.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R && !Android.OS.Environment.IsExternalStorageManager)
            {
                try
                {
                    var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission,
                                            Android.Net.Uri.Parse("package:" + Context.PackageName));
                    intent.AddFlags(ActivityFlags.NewTask);
                    Context.StartActivity(intent);
                }
                catch
                {
                    try
                    {
                        var intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
                        intent.AddFlags(ActivityFlags.NewTask);
                        Context.StartActivity(intent);
                    }
                    catch { }
                }
            }
        }

        public static suota_pgp.Model.PermissionState CurrentState()
        {
            return new suota_pgp.Model.PermissionState
            {
                LocationAuthorized = HasBluetooth(),
                StorageAuthorized = HasStorage()
            };
        }
    }
}
