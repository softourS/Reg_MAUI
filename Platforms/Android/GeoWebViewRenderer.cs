using Android.Content;
using Android.Util;
using Android.Webkit;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using PlannerBusConductoresN_MAUI;
using PlannerBusConductoresN_MAUI.Platforms.Android;
using WebView = Android.Webkit.WebView;

[assembly: ExportRenderer(typeof(GeoWebView), typeof(GeoWebViewRenderer))]
namespace PlannerBusConductoresN_MAUI.Platforms.Android;

public class GeoWebViewRenderer : WebViewRenderer {
    const string JAVASCRIPT_FUNCTION = @"function OpenLink(text) { Xamarin.InvokeDisplayJSText(text); }";

    public GeoWebViewRenderer(Context context) : base(context) {

    }

    protected override void OnElementChanged(ElementChangedEventArgs<Microsoft.Maui.Controls.WebView> e) {
        base.OnElementChanged(e);

        if (Control != null) {
            Control.ClearCache(true);
            Control.Settings.BuiltInZoomControls = true;
            Control.Settings.DisplayZoomControls = false;
            Control.Settings.LoadWithOverviewMode = true;
            Control.Settings.UseWideViewPort = true;
            Control.Settings.MediaPlaybackRequiresUserGesture = false;
            Control.Settings.CacheMode = CacheModes.NoCache;
            Control.Settings.SetAppCacheMaxSize(1);
            Control.Settings.JavaScriptCanOpenWindowsAutomatically = true;
            Control.Settings.DomStorageEnabled = true;
            Control.Settings.JavaScriptEnabled = true;
            Control.Settings.SetGeolocationEnabled(true);

            if (Element is GeoWebView webView) {
                if (e.OldElement != null) {
                    Control.RemoveJavascriptInterface("Xamarin");
                    webView.Cleanup();
                }
                if (e.NewElement != null) {
                    Control.AddJavascriptInterface(new JSBridge(this), "Xamarin");
                    Log.Debug("CustomWebViewRenderer", "JavascriptInterface 'Xamarin' added.");

                }
            }
            Control.SetWebChromeClient(new MyWebChromeClient());
        }
    }
}

public class MyWebChromeClient : WebChromeClient {
    public override void OnGeolocationPermissionsShowPrompt(string origin, GeolocationPermissions.ICallback callback) {
        callback.Invoke(origin, true, false);
    }

    public override void OnPermissionRequest(PermissionRequest request) {
        request.Grant(request.GetResources());
    }
}

public class JavascriptWebViewClient : FormsWebViewClient {
    private readonly string _javascript;

    public JavascriptWebViewClient(GeoWebViewRenderer renderer, string javascript) : base(renderer) {
        _javascript = javascript;
    }

    public override void OnPageFinished(WebView view, string url) {
        base.OnPageFinished(view, url);
        view.EvaluateJavascript("if (typeof Xamarin !== 'undefined') { console.log('Xamarin object is available.'); } else { console.log('Xamarin object is not available.'); }", null);
    }
}