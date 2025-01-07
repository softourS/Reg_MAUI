using Android.Graphics;
using Android.Util;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Handlers;
using WebView = Android.Webkit.WebView;

namespace PlannerBusConductoresN_MAUI.Platforms.Android;

public class HybridWebViewHandler : ViewHandler<IHybridWebView, WebView> {
    public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler> (ViewHandler.ViewMapper);
    const string JavascriptFunction = @"function OpenLink(text) { Xamarin.InvokeDisplayJSText(text); }";
    private JSBridge jsBridgeHandler;
    static SynchronizationContext sync;

    public HybridWebViewHandler () : base (HybridWebViewMapper) {
        sync = SynchronizationContext.Current;
    }

    private void VirtualView_SourceChanged (object sender, SourceChangedEventArgs e) {
        LoadSource (e.Source, PlatformView);
    }

    protected override WebView CreatePlatformView () {
        sync = sync ?? SynchronizationContext.Current;
        WebView webView = new WebView (Context);
        jsBridgeHandler = new JSBridge (this);
        webView.Settings.JavaScriptEnabled = true;
        webView.SetWebViewClient (new JSWebViewClient ($"javascript: {JavascriptFunction}"));
        webView.AddJavascriptInterface (jsBridgeHandler, "Xamarin");
        return webView;
    }

    protected override void ConnectHandler (WebView platformView) {
        base.ConnectHandler (platformView);
        if (VirtualView.Source != null) {
            LoadSource (VirtualView.Source, PlatformView);
        }
        VirtualView.SourceChanged += VirtualView_SourceChanged;
        VirtualView.RequestEvaluateJavaScript += VirtualView_RequestEvaluateJavaScript;
    }

    private void VirtualView_RequestEvaluateJavaScript (object sender, EvaluateJavaScriptAsyncRequest e) {
        sync.Post ((o) => PlatformView.EvaluateJavascript (e.Script, null), null);
    }

    protected override void DisconnectHandler (WebView platformView) {
        base.DisconnectHandler (platformView);
        VirtualView.SourceChanged -= VirtualView_SourceChanged;
        VirtualView.Cleanup ();
        jsBridgeHandler?.Dispose ();
        jsBridgeHandler = null;
    }

    private static void LoadSource (WebViewSource source, WebView control) {
        try {
            if (source is HtmlWebViewSource html) {
                control.LoadDataWithBaseURL (html.BaseUrl, html.Html, null, "charset=UTF-8", null);
            } else if (source is UrlWebViewSource url) {
                control.LoadUrl (url.Url);
            }
        } catch { }
    }
}

public class JSWebViewClient : WebViewClient {
    string _javascript;
    public JSWebViewClient (string javascript) {
        _javascript = javascript;
    }

    public override void OnPageStarted (WebView view, string url, Bitmap favicon) {
        base.OnPageStarted (view, url, favicon);
        view.EvaluateJavascript (_javascript, null);
    }

    public override void OnPageFinished (WebView view, string url) {
        base.OnPageFinished (view, url);
        view.EvaluateJavascript ("if (typeof Xamarin !== 'undefined') { console.log('Xamarin object is available.'); } else { console.log('Xamarin object is not available.'); }", null);
    }
}

public class JSBridge : Java.Lang.Object {
    readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;
    internal JSBridge (HybridWebViewHandler hybridRenderer) {
        hybridWebViewRenderer = new WeakReference<HybridWebViewHandler> (hybridRenderer);
    }

    [JavascriptInterface]
    [Export ("invokeAction")]
    public void InvokeAction (string data) {
        HybridWebViewHandler hybridRenderer;
        if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget (out hybridRenderer)) {
            hybridRenderer.VirtualView.InvokeAction (data);
        }
    }

    public class ValueCallback : Java.Lang.Object, IValueCallback {
        public void OnReceiveValue (Java.Lang.Object value) {
            Log.Debug ("CustomWebViewRenderer", "EvaluateJavascript result: " + value.ToString ());
        }
    }
}