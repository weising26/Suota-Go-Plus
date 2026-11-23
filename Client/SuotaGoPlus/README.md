# Suota Go+ (.NET 10 MAUI client)

A .NET 10 MAUI port of the original Xamarin.Forms client, so it builds with
current tooling and installs on Android 12+ devices (the original APK fails to
install there because of its old target SDK and signature scheme).

## Download a prebuilt APK

Every push to the `net8-maui-port` branch builds a **signed APK** via GitHub
Actions. To get it:

1. Open the repo's **Actions** tab → **Build Android APK** → latest run.
2. Download the **`suota-go-plus-apk`** artifact (a zip containing the APK).
3. Unzip and sideload the `*-Signed.apk` onto your phone.

You can also trigger a build manually from the Actions tab (**Run workflow**).

## Build locally

Requirements: .NET 10 SDK, the MAUI Android workload, JDK 17, Android SDK
(platform 35 + build-tools 35).

```bash
dotnet workload install maui-android
dotnet publish Client/SuotaGoPlus/SuotaGoPlus.csproj -c Release -f net10.0-android \
  -p:AndroidPackageFormat=apk
```

The APK is written under `Client/SuotaGoPlus/bin/Release/net10.0-android/`.
For an installable build, sign it with your own keystore by also passing
`-p:AndroidKeyStore=true -p:AndroidSigningKeyStore=... -p:AndroidSigningKeyAlias=...`
`-p:AndroidSigningStorePass=... -p:AndroidSigningKeyPass=...` (see the CI
workflow for an example).

## Permissions on first launch

- **Bluetooth** (scan + connect): granted via the normal runtime prompt on
  Android 12+.
- **All files access**: the app opens the system "All files access" screen so
  it can read/write the shared `SuotaPgp` folder. Enable it for Suota Go+.

Put `patch.img` in `/sdcard/SuotaPgp/` (internal storage → `SuotaPgp`). Saved
key `.json` files are written to the same folder.

## What changed from the original

- Single-project .NET 10 MAUI app (`net10.0-android`, targetSdk 35) replacing
  the Xamarin.Forms shared + Android head projects.
- A small Prism-compatibility shim under `Compat/` (BindableBase,
  DelegateCommand, EventAggregator, navigation, logging) so the BLE/SUOTA logic
  carried over almost unchanged.
- `MvvmCross.Plugin.BLE` 1.3 → `Plugin.BLE` 3.x; `Plugin.Permissions` and
  `Plugin.CurrentActivity` removed in favour of MAUI + AndroidX APIs.
- Android 12+ `BLUETOOTH_SCAN` / `BLUETOOTH_CONNECT` and `MANAGE_EXTERNAL_STORAGE`.

The original Xamarin project is left intact under `Client/suota_pgp/` for
reference.
