﻿namespace PlannerBusConductoresN_MAUI;

public class SourceChangedEventArgs : EventArgs {
    public WebViewSource Source { get; private set; }

    public SourceChangedEventArgs (WebViewSource source) {
        Source = source;
    }
}

public class JavaScriptActionEventArgs : EventArgs {
    public string Payload { get; private set; }

    public JavaScriptActionEventArgs (string payload) {
        Payload = payload;
    }
}

public interface IHybridWebView : IView {
    event EventHandler<SourceChangedEventArgs> SourceChanged;
    event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;
    event EventHandler<EvaluateJavaScriptAsyncRequest> RequestEvaluateJavaScript;
    void Refresh ();
    Task EvaluateJavaScriptAsync (EvaluateJavaScriptAsyncRequest request);
    WebViewSource Source { get; set; }
    void Cleanup ();
    void InvokeAction (string data);
}

public class HybridWebView : WebView, IHybridWebView {
    public event EventHandler<SourceChangedEventArgs> SourceChanged;
    public event EventHandler<JavaScriptActionEventArgs> JavaScriptAction;
    public event EventHandler<EvaluateJavaScriptAsyncRequest> RequestEvaluateJavaScript;

    public async Task EvaluateJavaScriptAsync (EvaluateJavaScriptAsyncRequest request) {
        await Task.Run (() => { RequestEvaluateJavaScript?.Invoke (this, request); });
    }

    public void Refresh () {
        if (Source == null) return;
        WebViewSource wvsource = Source;
        Source = null;
        Source = wvsource;
    }

    public WebViewSource Source {
        get { return (WebViewSource) GetValue (SourceProperty); }
        set { SetValue (SourceProperty, value); }
    }

    public static readonly BindableProperty SourceProperty = BindableProperty.Create (propertyName: "Source", returnType: typeof (WebViewSource),
        declaringType: typeof (HybridWebView), defaultValue: new UrlWebViewSource () { Url = "about:blank" }, propertyChanged: OnSourceChanged);

    private static void OnSourceChanged (BindableObject bindable, object oldValue, object newValue) {
        HybridWebView? view = bindable as HybridWebView;
        bindable.Dispatcher.Dispatch (() => {
            view.SourceChanged?.Invoke (view, new SourceChangedEventArgs (newValue as WebViewSource));
        });
    }

    public void Cleanup () {
        JavaScriptAction = null;
    }

    public void InvokeAction (string data) {
        JavaScriptAction?.Invoke (this, new JavaScriptActionEventArgs (data));
    }
}