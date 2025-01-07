using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace PlannerBusConductoresN_MAUI.Platforms.Android;

[Service]
public class DemoServices : Service, IServiceTest {
    public override IBinder OnBind(Intent intent) {
        throw new NotImplementedException();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId) {
        if (intent.Action == "START_SERVICE") {
            RegisterNotification();//Proceed to notify
        } else if (intent.Action == "STOP_SERVICE") {
            StopForeground(true);//Stop the service
            StopSelfResult(startId);
        }
        return StartCommandResult.NotSticky;
    }

    public void Start() {
        Intent startService = new(MainActivity.ActivityCurrent, typeof(DemoServices));
        startService.SetAction("START_SERVICE");
        MainActivity.ActivityCurrent.StartService(startService);
    }

    public void Stop() {
        Intent stopIntent = new(MainActivity.ActivityCurrent, Class);
        stopIntent.SetAction("STOP_SERVICE");
        MainActivity.ActivityCurrent.StartService(stopIntent);
    }

    private void RegisterNotification() {
        NotificationChannel channel = new("ServiceChannel", "ServiceDemo", NotificationImportance.Max);
        NotificationManager manager = (NotificationManager)MainActivity.ActivityCurrent.GetSystemService(NotificationService);
        manager.CreateNotificationChannel(channel);
        Notification notification = new Notification.Builder(this, "ServiceChannel").SetContentTitle("Service Working").SetOngoing(true).Build();
        StartForeground(100, notification);
    }
}