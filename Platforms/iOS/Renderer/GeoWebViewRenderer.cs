using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using PlannerBusConductoresN_MAUI;
using PlannerBusConductoresN_MAUI.Platforms.iOS;
using WebKit;

[assembly: ExportRenderer(typeof(GeoWebView), typeof(GeoWebViewRenderer))]
namespace PlannerBusConductoresN_MAUI.Platforms.iOS;


public class GeoWebViewRenderer : WkWebViewRenderer, IWKScriptMessageHandler {

    private const string InvokeDisplayJSText = nameof (InvokeDisplayJSText);
    private const string JAVASCRIPT_FUNCTION = "function InvokeDisplayJSText(link){window.webkit.messageHandlers.InvokeDisplayJSText.postMessage(link);} ";

    protected override void OnElementChanged (VisualElementChangedEventArgs e) {
        base.OnElementChanged (e);

        if (e.OldElement != null) {
            Configuration?.UserContentController?.RemoveAllUserScripts ();
            Configuration?.UserContentController?.RemoveScriptMessageHandler (InvokeDisplayJSText);
            (e.OldElement as GeoWebView)?.Cleanup ();
        }

        if (e.NewElement != null) {
            if (!(Element is GeoWebView))
                return;


            Configuration?.UserContentController?.AddScriptMessageHandler (this, InvokeDisplayJSText);
            System.Diagnostics.Debug.WriteLine ("CustomWebViewRenderer", "JavascriptInterface 'InvokeDisplayJSText' added.");
        }
    }

    public void DidReceiveScriptMessage (WKUserContentController userContentController, WKScriptMessage message) {
        if (!(Element is GeoWebView webView))
            return;

        switch (message.Name) {
            case InvokeDisplayJSText:
                webView.InvokeDisplayJSTextAction (message.Body.ToString ());
                System.Diagnostics.Debug.WriteLine ("CustomWebViewRenderer", "EvaluateJavascript result: " + message.Body.ToString ());
                break;
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