using Microsoft.Maui.Controls.Compatibility;
using Firebase.Analytics;
#if ANDROID
using Android.Content;
using Android.OS;
#elif IOS
using Foundation;
#endif

namespace PlannerBusConductoresN_MAUI;

public interface IFirebaseAnalyticsService {
    void Log(string value, string eventName = "screen_view", string paramName = "screen_name");
    void Log(string eventName, IDictionary<string, string> parameters);
}

public class FirebaseAnalyticsService : IFirebaseAnalyticsService {
    public void Log(string value, string eventName = "screen_view", string paramName = "screen_name") {
        Log(eventName, new Dictionary<string, string> {
            { paramName, value.Replace("ViewModel", "", StringComparison.InvariantCulture) }
        });
    }
    
    public void Log(string eventName, IDictionary<string, string> parameters) {
        #if ANDROID
        FirebaseAnalytics firebaseAnalytics = FirebaseAnalytics.GetInstance(Platform.CurrentActivity);
        if (parameters == null) {
            firebaseAnalytics.LogEvent(eventName, null);
            return;
        }
        Bundle bundle = new Bundle();
        foreach (KeyValuePair<string, string> param in parameters) {
            bundle.PutString(param.Key, param.Value);
        }
        firebaseAnalytics.LogEvent(eventName, bundle);
        #elif IOS
        if (parameters == null) {
            Analytics.LogEvent(eventName, (Dictionary<object, object>)null);
            return;
        }
        List<NSString> keys = new List<NSString>();
        List<NSString> values = new List<NSString>();
        foreach (KeyValuePair<string, string> item in parameters) {
            keys.Add(new NSString(item.Key));
            values.Add(new NSString(item.Value));
        }
        NSDictionary<NSString, NSObject> parametersDictionary = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values.ToArray(), keys.ToArray(), keys.Count);
        Analytics.LogEvent(eventName, parametersDictionary);
        #endif
    }
}