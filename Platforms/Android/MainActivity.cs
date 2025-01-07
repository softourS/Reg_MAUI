using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.Firebase.CloudMessaging;

namespace PlannerBusConductoresN_MAUI;

[Activity(Label = "PlannerBus Conductores", Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize |
    ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {

    public static bool AppCenterConfigured { get; set; }
   public static MainActivity ActivityCurrent { get; set; }

    public MainActivity() {
        ActivityCurrent = this;
    }

    protected override void OnCreate(Bundle? savedInstanceState) {
        Intent? intent = this.Intent;
        IConfiguration config = MauiProgram.GetService<IConfiguration>();
        string? secret = config.GetSection("AppCenter:Secret")?.Value;
        if (!string.IsNullOrEmpty(secret)) {
            if (!AppCenterConfigured) {
                AppCenter.Configure(secret);
                AppCenter.Start(typeof(Crashes), typeof(Analytics));
            }
        }
        HandleIntent(intent);
        CreateNotificationChannelIfNeeded();
        base.OnCreate(savedInstanceState);
    }

    protected override void OnNewIntent(Intent? intent) {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    private static void HandleIntent(Intent? intent) {
        if (intent is not null) {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#if ANDROID23_0_OR_GREATER
#pragma warning disable CA1416 // Validate platform compatibility
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CA1416 // Validate platform compatibility
#endif
    }

    private void CreateNotificationChannelIfNeeded() {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
            CreateNotificationChannel();
        }
    }

    private void CreateNotificationChannel() {
        string channelId = $"{PackageName}.general";
#if ANDROID26_0_OR_GREATER
        NotificationManager? notificationManager = GetSystemService(NotificationService) as NotificationManager;
        if (notificationManager is not null) {
            if ((int)Build.VERSION.SdkInt > 26) {
                NotificationChannel channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
        FirebaseCloudMessagingImplementation.ChannelId = channelId;
#endif
    }
}