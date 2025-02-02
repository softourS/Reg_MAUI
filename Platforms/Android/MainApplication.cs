﻿using Android.App;
using Android.Runtime;
[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessBackgroundLocation)]
[assembly: UsesFeature("android.hardware.location", Required = false)]
[assembly: UsesFeature("android.hardware.location.gps", Required = false)]
[assembly: UsesFeature("android.hardware.location.network", Required = false)]

namespace PlannerBusConductoresN_MAUI;

[Application(Label = "PlannerBus Conductores", Theme = "@style/Maui.SplashTheme")]
public class MainApplication : MauiApplication {

    public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) {

    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}