using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using PlannerBusConductoresN_MAUI;
using PlannerBusConductoresN_MAUI.Platforms.iOS;
using WebKit;

[assembly: ExportRenderer(typeof(GeoWebView), typeof(GeoWebViewRenderer))]
namespace PlannerBusConductoresN_MAUI.Platforms.iOS;

public class GeoWebViewRenderer : WkWebViewRenderer {

    WKUserContentController userController;

    public GeoWebViewRenderer() : this(new WKWebViewConfiguration()) {
    }

    public GeoWebViewRenderer(WKWebViewConfiguration config) : base(config) {
        userController = config.UserContentController;
    }

    protected override void OnElementChanged(VisualElementChangedEventArgs e) {
        base.OnElementChanged(e);
        if (e.OldElement != null) {
            GeoWebView hybridWebView = e.OldElement as GeoWebView;
        }
    }
}