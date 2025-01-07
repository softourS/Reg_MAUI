using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using WebKit;

namespace PlannerBusConductoresN_MAUI.Platforms.iOS;

public class HybridWebViewHandler : ViewHandler<IHybridWebView, WKWebView> {
    public static PropertyMapper<IHybridWebView, HybridWebViewHandler> HybridWebViewMapper = new PropertyMapper<IHybridWebView, HybridWebViewHandler> (ViewMapper);
    const string JavaScriptFunction = "function InvokeDisplayJSText(link){window.webkit.messageHandlers.InvokeDisplayJSText.postMessage(link);} ";
    private WKUserContentController userController;
    private JSBridge jsBridgeHandler;
    static SynchronizationContext sync;

    public HybridWebViewHandler () : base (HybridWebViewMapper) {
        sync = SynchronizationContext.Current;
    }

    private void VirtualView_SourceChanged (object sender, SourceChangedEventArgs e) {
        LoadSource (e.Source, PlatformView);
    }

    protected override WKWebView CreatePlatformView () {
        sync = sync ?? SynchronizationContext.Current;
        jsBridgeHandler = new JSBridge (this);
        userController = new WKUserContentController ();
        WKUserScript script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
        userController.AddUserScript (script);
        userController.AddScriptMessageHandler (jsBridgeHandler, "CustomWebViewRenderer");
        WKWebViewConfiguration config = new WKWebViewConfiguration { UserContentController = userController };
        WKWebView webView = new WKWebView(CGRect.Empty, config);
        return webView;
    }

    protected override void ConnectHandler (WKWebView platformView) {
        base.ConnectHandler (platformView);
        if (VirtualView.Source != null) {
            LoadSource (VirtualView.Source, PlatformView);
        }
        VirtualView.SourceChanged += VirtualView_SourceChanged;
        VirtualView.RequestEvaluateJavaScript += VirtualView_RequestEvaluateJavaScript;
    }

    private void VirtualView_RequestEvaluateJavaScript (object sender, EvaluateJavaScriptAsyncRequest e) {
        sync.Post ((o) => { PlatformView.EvaluateJavaScript (e); }, null);
    }

    protected override void DisconnectHandler (WKWebView platformView) {
        base.DisconnectHandler (platformView);
        VirtualView.SourceChanged -= VirtualView_SourceChanged;
        userController.RemoveAllUserScripts ();
        userController.RemoveScriptMessageHandler ("CustomWebViewRenderer");
        jsBridgeHandler?.Dispose ();
        jsBridgeHandler = null;
    }


    private static void LoadSource (WebViewSource source, WKWebView control) {
        if (source is HtmlWebViewSource html) {
            control.LoadHtmlString (html.Html, new NSUrl (html.BaseUrl ?? "http://localhost", true));
        } else if (source is UrlWebViewSource url) {
            control.LoadRequest (new NSUrlRequest (new NSUrl (url.Url)));
        }
    }
}

public class JSBridge : NSObject, IWKScriptMessageHandler {
    readonly WeakReference<HybridWebViewHandler> hybridWebViewRenderer;

    internal JSBridge (HybridWebViewHandler hybridRenderer) {
        hybridWebViewRenderer = new WeakReference<HybridWebViewHandler> (hybridRenderer);
    }

    public void DidReceiveScriptMessage (WKUserContentController userContentController, WKScriptMessage message) {
        HybridWebViewHandler hybridRenderer;
        if (hybridWebViewRenderer.TryGetTarget (out hybridRenderer)) {
            hybridRenderer.VirtualView?.InvokeAction (message.Body.ToString ());
            System.Diagnostics.Debug.WriteLine ("CustomWebViewRenderer", "EvaluateJavascript result: " + message.Body.ToString ());
        }
    }

    bool _isDisposed;
    protected override void Dispose (bool disposing) {
        if (_isDisposed)
            return;

        _isDisposed = true;

        base.Dispose (disposing);
        GC.SuppressFinalize (this);
    }
}