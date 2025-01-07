using Microsoft.Maui.LifecycleEvents;
using System.Reflection;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
using PlannerBusConductoresN_MAUI.Platforms.Android;
#elif IOS
using Plugin.Firebase.Core.Platforms.iOS;
using Plugin.Firebase.CloudMessaging;
using UIKit;
#endif

namespace PlannerBusConductoresN_MAUI;

public static class MauiProgram {
    static IServiceProvider? _serviceProvider;

    public static TService GetService<TService>() where TService : notnull => _serviceProvider is not null ? (_serviceProvider.GetRequiredService<TService>()) : throw new Exception("Service provider not registered yet");

    public static MauiApp CreateMauiApp() {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlannerBusConductoresN_MAUI.appsettings.json") ?? throw new Exception("appsettings.json not found");
        IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonStream(stream);
#if ANDROID
        using Stream androidStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlannerBusConductoresN_MAUI.appsettings.Android.json") ?? throw new Exception("appsettings.Android.json not found");
        configBuilder = configBuilder.AddJsonStream(androidStream);
#elif IOS
        using Stream iosStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlannerBusConductoresN_MAUI.appsettings.Ios.json") ?? throw new Exception("appsettings.Ios.json not found");
        configBuilder = configBuilder.AddJsonStream(iosStream);
#endif
#if DEBUG
        using Stream debugStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlannerBusConductoresN_MAUI.appsettings.Debug.json") ?? throw new Exception("appsettings.Debug.json not found");
        configBuilder = configBuilder.AddJsonStream(debugStream);
#elif !DEBUG
        using Stream releaseStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlannerBusConductoresN_MAUI.appsettings.Release.json") ?? throw new Exception("appsettings.Release.json not found");
        configBuilder = configBuilder.AddJsonStream(releaseStream);
#endif
        IConfigurationRoot config = configBuilder.Build();
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder.Configuration.AddConfiguration(config);
        builder.UseMauiApp<App>().RegisterFirebaseServices().ConfigureFonts(fonts => {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddTransient<MainPage>();

#if ANDROID
        builder.Services.AddTransient<IServiceTest, DemoServices>();
#endif

#if ANDROID
        builder.ConfigureMauiHandlers(handlers => {
            handlers.AddHandler(typeof(HybridWebView), typeof(HybridWebViewHandler));
        });
#elif IOS
        builder.ConfigureMauiHandlers(handlers => {
            handlers.AddHandler(typeof(HybridWebView), typeof(Platforms.iOS.HybridWebViewHandler));
        });
#endif

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddSingleton<AppShell>().AddViews().AddViewModels().AddCustomServices(config);
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        MauiApp app = builder.Build();
        _serviceProvider = app.Services;
        return app;
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder) {
        builder.ConfigureLifecycleEvents(events => {
#if ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                CrossFirebase.Initialize(activity);
            }));
#elif IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((app, launchOptions) => {
                CrossFirebase.Initialize();
                FirebaseCloudMessagingImplementation.Initialize();
                return true;
            }));
#endif
        });
        return builder;
    }
}