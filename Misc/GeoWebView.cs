namespace PlannerBusConductoresN_MAUI;

public class GeoWebView : WebView {
    private Action<string>? _displayJSTextAction;
    public void Cleanup() {
        _displayJSTextAction = null;
    }

    public void RegisterDisplayJSTextAction(Action<string> displayJSTextAction) {
        _displayJSTextAction = displayJSTextAction;
    }

    public void InvokeDisplayJSTextAction(string displayJSTextAction) {
        _displayJSTextAction?.Invoke(displayJSTextAction);
    }
}