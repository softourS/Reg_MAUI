using Android.Util;
using Android.Webkit;
using Java.Interop;

namespace PlannerBusConductoresN_MAUI.Platforms.Android;

public class JavascriptBridge : Java.Lang.Object {
    private readonly WeakReference<GeoWebViewRenderer> _hybridWebViewRenderer;

    public JavascriptBridge(GeoWebViewRenderer hybridRenderer) {
        _hybridWebViewRenderer = new WeakReference<GeoWebViewRenderer>(hybridRenderer);
    }

    [Export("InvokeDisplayJSText")]
    [JavascriptInterface]
    public void InvokeDisplayJSText(string link) {
        if (_hybridWebViewRenderer != null && _hybridWebViewRenderer.TryGetTarget(out GeoWebViewRenderer hybridRenderer)) {
            ((GeoWebView)hybridRenderer.Element).InvokeDisplayJSTextAction(link);
        }
    }

    public class ValueCallback : Java.Lang.Object, IValueCallback {
        public void OnReceiveValue(Java.Lang.Object value) {
            Log.Debug("CustomWebViewRenderer", "EvaluateJavascript result: " + value.ToString());
        }
    }
}