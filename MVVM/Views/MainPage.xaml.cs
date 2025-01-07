using System.Diagnostics;
using Plugin.Firebase.CloudMessaging;
using System.Text;
using Newtonsoft.Json;
using PlannerBusConductoresN_MAUI.Models.Permissions;
using GeolocatorPlugin.Abstractions;
using GeolocatorPlugin;
#if ANDROID
using Application = Android.App.Application;
using LocationManager = Android.Locations.LocationManager;
using Android.Content;
#elif IOS
using CoreLocation;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace PlannerBusConductoresN_MAUI.Views;

public partial class MainPage : ContentPage {
    private static HybridWebView browser = new();
    private readonly MainPageViewModel _vm;
    private bool first = true;
    Position? previous_pos;
    Location? previous_locat;
    TimeSpan? previous_time;
    private string url = "";

#if ANDROID
    IServiceTest Services;

    public MainPage(MainPageViewModel vm, IServiceTest Services_) {
        InitializeComponent();
        Services = Services_;
        BindingContext = vm;
        _vm = vm;
        FireBaseMessSettings ();
    }
#elif IOS
    public MainPage (MainPageViewModel vm) {
        InitializeComponent ();
        BindingContext = vm;
        _vm = vm;
        FireBaseMessSettings();
        On<iOS> ().SetUseSafeArea (true);
    }
#endif

    void FireBaseMessSettings () {
        CrossFirebaseCloudMessaging.Current.NotificationReceived += (sender, e) => {
            Debug.WriteLine ("Received", "Firebase [MAIN ONR]");
            try {
                string data64Code = Convert.ToBase64String (Encoding.UTF8.GetBytes (JsonConvert.SerializeObject (e.Notification.Data)));
                MainThread.BeginInvokeOnMainThread (async () => {
                    await RunJSWebViewNotif (data64Code);
                });
            } catch (Exception ex) {
                Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN ONR]");
            }
        };
        CrossFirebaseCloudMessaging.Current.TokenChanged += async (source, e) => {
            try {
                if (string.IsNullOrEmpty (e.Token)) {
                    string token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync ();
                    if (!string.IsNullOrEmpty (token)) {
                        ResetToken (token, "IF");
                    }
                } else {
                    ResetToken (e.Token, "ELSE");
                }
            } catch (Exception ex) {
                Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN OTR]");
            }
        };
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        try {
#if ANDROID
            Services.Stop();
#endif
        } catch (Exception ex) {
            Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN ONDISAPP]");
        }
    }

    private void ResetToken(string token, string txt) {
        if (!token.Equals(Constantes.tokepush)) {
            Debug.WriteLine($"TOKEN changed: {token}", $"Firebase [MAIN OTR {txt}]");
            Constantes.tokepush = token;
            MainThread.BeginInvokeOnMainThread(async () => {
                await RunJSWebViewIDDeviceNToken();
            });
        }
    }
    
    private async Task CallPermissions() {
        try {
            bool p1 = CheckGrantedPermissions(await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>());
            bool p2 = CheckGrantedPermissions(await Permissions.CheckStatusAsync<Permissions.LocationAlways>());
            bool p3 = CrossFirebaseCloudMessaging.IsSupported && CheckGrantedPermissions(await Permissions.CheckStatusAsync<NotificationPermission>());
            bool p4 = CheckGrantedPermissions(await Permissions.CheckStatusAsync<Permissions.Camera>());
            if (!p1 || !p2 || !p3 || !p4) {
                string body = "Se le va a solicitar acceso a la ubicación del dispositivo y la cámara del mismo," +
                    " esta funcionalidad es necesaria para el correcto funcionamiento de la aplicación," +
                    " en ningun momento se almacenarán estos datos ni se vincularán con su persona." +
                    " Recuerde que su ubicación se utilizará cuando la aplicación esté en uso y/o en segundo plano.";
                string differ = DeviceInfo.Platform.Equals(DevicePlatform.Android)
                    ? $"Por favor, marca 'PRECISA', y 'MIENTRAS SE USA LA APLICACIÓN'"
                    : $"Por favor, marca 'PERMITIR AL USARSE LA APP' y 'SIEMPRE' en el menu de permisos";
                await DisplayAlert("Política de privacidad", $"{body}{Environment.NewLine}{differ}", "OK");
                PermissionStatus status = PermissionStatus.Granted;
#if ANDROID
                if ((int)Android.OS.Build.VERSION.SdkInt >= 26 && (int)Android.OS.Build.VERSION.SdkInt <= 28) {
                    status = p1 ? PermissionStatus.Granted : await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                if ((int)Android.OS.Build.VERSION.SdkInt > 28) {
                    status = p2 ? PermissionStatus.Granted : await Permissions.RequestAsync<Permissions.LocationAlways>();
                }
#elif IOS
                status = p2 ? PermissionStatus.Granted : await Permissions.RequestAsync<Permissions.LocationAlways>();
                status = p1 ? PermissionStatus.Granted : await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
#endif
                status = p4 ? PermissionStatus.Granted : await Permissions.RequestAsync<Permissions.Camera>();
                if (CrossFirebaseCloudMessaging.IsSupported) {
                    status = p3 ? PermissionStatus.Granted : await Permissions.RequestAsync<NotificationPermission>();
                } else {
                    await DisplayAlert("Alerta", "Este dispositivo no soporta mensajería Firebase", "OK");
                    return;
                }
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN ONNAV]");
        }
    }

    private static bool CheckGrantedPermissions(PermissionStatus permission) => permission == PermissionStatus.Granted;

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);
        try {
            DeviceDisplay.KeepScreenOn = true;
            if (Connectivity.NetworkAccess != NetworkAccess.Internet) {
                _ = DisplayAlert("Error", "El dispositivo no tiene conexión, la aplicación no puede ejecutarse", "Ok");
            }
            await _vm.LoadAsync();
            string cachePath = Path.GetTempPath();
            if (Directory.Exists(cachePath)) {
                Directory.Delete(cachePath, true);
            }
            if (!Directory.Exists(cachePath)) {
                Directory.CreateDirectory(cachePath);
            }
            string utoken = Constantes.Base64Encode($"Device={Constantes.id_device}&TokenFCM={Constantes.tokepush}");
            url = "https://atmv-dev.softoursistemas.net/";
            //url = $"https://pruebas-appconductores.softoursistemas.com/#/login?t={utoken}";
            //url = $"https://appconductores.softoursistemas.com/#/login?t={utoken}";
            Debug.WriteLine(url, "Url [MAIN ONNAV]");
            browser = new HybridWebView { Source = url };
            browser.Navigated += async (sender, e) => {
                switch (e.Result) {
                    case WebNavigationResult.Success: {
                            if (first) {
                                first = false;
                                await CallPermissions();
                                try {
#if ANDROID
                                    Services.Start();
#endif
                                    await GetCurrentLocation();
                                } catch (Exception ex) {
                                    Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN ONAPP]");
                                }
                            }
                            await RunJSWebViewIDDeviceNToken();
                        }
                        break;
                    case WebNavigationResult.Failure: break;
                    case WebNavigationResult.Timeout: break;
                    case WebNavigationResult.Cancel: break;
                    default: break;
                }
            };
            Content = browser;
        } catch (Exception ex) {
            Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN ONNAV]");
        }
    }

    public async Task GetCurrentLocation() {
        try {
            const string separator = "\n-----------------------------------------\n";
            if (IsLocationServiceEnabled()) {
                if (await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 1, true, new ListenerSettings {
                    ActivityType = ActivityType.AutomotiveNavigation, AllowBackgroundUpdates = true,
                    PauseLocationUpdatesAutomatically = false, DeferralDistanceMeters = 0
                })) {
                    CrossGeolocator.Current.PositionChanged += (sender, e) => {
                        Position position = e.Position;
                        TimeSpan actualtime = DateTime.Now.TimeOfDay;

                        Constantes.latitude = position.Latitude;
                        Constantes.longitude = position.Longitude;
                        Constantes.timestamp = actualtime;
                        Constantes.degrees = previous_pos != null
                            ? Misc_Objects.DegreeBearing(previous_pos.Latitude, previous_pos.Longitude, position.Latitude, position.Longitude)
                            : 0.0;
                        Constantes.speed = (previous_pos != null
                            ? Misc_Objects.CalcSpeed(previous_pos.Latitude, previous_pos.Longitude, previous_time ?? DateTime.Now.TimeOfDay, position.Latitude, position.Longitude, actualtime)
                            : 0.0) * 3.6;
                        Constantes.metres = previous_pos != null
                            ? Misc_Objects.ArcInMeters(previous_pos.Latitude, previous_pos.Longitude, position.Latitude, position.Longitude)
                            : 0.0;
                        if (previous_time != null && Constantes.timestamp.Seconds == ((TimeSpan)previous_time).Seconds) {
                        } else {
                            Debug.WriteLine($"{separator}{Misc_Objects.AllSortedOut()}{separator}", "Location [MAIN GETLOCAT]");
                            Debug.WriteLine($"Location Updated: {DateTime.Now}", "Location [MAIN GETLOCAT]");
                            MainThread.BeginInvokeOnMainThread(async () => {
                                await RunJSWebViewLocation($"{ChangeData(Constantes.latitude)},{ChangeData(Constantes.longitude)},{ChangeData(Constantes.degrees)}|" +
                                    $"{ChangeData(Constantes.speed)}|{ChangeData(Constantes.metres)}|{Constantes.timestamp.ToString("c").Remove(8)}");
                            });
                        }

                        previous_pos = position;
                        previous_time = actualtime;
                    };
                    CrossGeolocator.Current.PositionError += (sender, e) => {
                        Debug.WriteLine(e.Error, "Error [MAIN GETLOCAT]");
                    };
                }
            } else {
                throw new Exception("Activa el GPS.");
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to query location: {ex.Message}", "Error [MAIN GETLOCAT]");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    public async Task GetCurrentLocationM() {
        try {
            const string separator = "\n-----------------------------------------\n";
            if (IsLocationServiceEnabled()) {
                GeolocationListeningRequest request = new(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
                if (await Geolocation.StartListeningForegroundAsync(request)) {
                    Geolocation.LocationChanged += (sender, e) => {
                        Location location = e.Location;
                        TimeSpan actualtime = DateTime.Now.TimeOfDay;
                        Constantes.latitude = location.Latitude;
                        Constantes.longitude = location.Longitude;
                        Constantes.timestamp = actualtime;
                        Constantes.degrees = previous_locat != null
                            ? Misc_Objects.DegreeBearing(previous_locat.Latitude, previous_locat.Longitude, location.Latitude, location.Longitude)
                            : 0.0;
                        Constantes.speed = (previous_locat != null
                            ? Misc_Objects.CalcSpeed(previous_locat.Latitude, previous_locat.Longitude, previous_time ?? DateTime.Now.TimeOfDay, location.Latitude, location.Longitude, actualtime)
                            : 0.0) * 3.6;
                        Constantes.metres = previous_locat != null
                            ? Misc_Objects.ArcInMeters(previous_locat.Latitude, previous_locat.Longitude, location.Latitude, location.Longitude)
                            : 0.0;
                        if (previous_time != null && Constantes.timestamp.Seconds == ((TimeSpan)previous_time).Seconds) {
                        } else {
                            Debug.WriteLine($"{separator}{Misc_Objects.AllSortedOut()}{separator}", "Location [MAIN GETLOCAT]");
                            Debug.WriteLine($"Location Updated: {DateTime.Now}", "Location [MAIN GETLOCAT]");
                            MainThread.BeginInvokeOnMainThread(async () => {
                                await RunJSWebViewLocation($"{ChangeData(Constantes.latitude)},{ChangeData(Constantes.longitude)},{ChangeData(Constantes.degrees)}|" +
                                    $"{ChangeData(Constantes.speed)}|{ChangeData(Constantes.metres)}|{Constantes.timestamp.ToString("c").Remove(8)}");
                            });
                        }
                        previous_locat = location;
                        previous_time = actualtime;
                    };
                } else {
                    await GetCurrentLocationM();
                    return;
                }
            } else {
                throw new Exception("Activa el GPS.");
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to query location: {ex.Message}", "Error [MAIN_VM GETLOCAT]");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    private static bool IsLocationServiceEnabled() {
#if ANDROID
        LocationManager locationManager = (LocationManager)Application.Context.GetSystemService(Context.LocationService);
        return locationManager is not null && locationManager.IsProviderEnabled(LocationManager.GpsProvider);
#elif IOS
        return CLLocationManager.Status == CLAuthorizationStatus.Authorized;
#endif
    }

    public async Task RunJSWebViewIDDeviceNToken() {
        try {
            if (string.IsNullOrEmpty(Constantes.tokepush) || string.IsNullOrEmpty(Constantes.id_device)) {
                await _vm.LoadAsync();
                Debug.WriteLine($"TOKEN: {Constantes.tokepush}", "Firebase [MAIN JSDEVTOKEN IF]");
                Debug.WriteLine($"DeviceID: {Constantes.id_device}", "ID [MAIN JSDEVTOKEN IF]");
            } else {
                Debug.WriteLine($"TOKEN: {Constantes.tokepush}", "Firebase [MAIN JSDEVTOKEN ELSE]");
                Debug.WriteLine($"DeviceID: {Constantes.id_device}", "ID [MAIN JSDEVTOKEN ELSE]");
            }
            await browser.EvaluateJavaScriptAsync(new EvaluateJavaScriptAsyncRequest ($"XamarinFunctionTkenFCM('{Constantes.tokepush}')"));
            await browser.EvaluateJavaScriptAsync(new EvaluateJavaScriptAsyncRequest ($"XamarinFunctionDeviceId('{Constantes.id_device}')"));
        } catch (Exception ex) {
            Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN JSDEVTOKEN]");
            await RunJSWebViewIDDeviceNToken ();
        }
    }

    public static async Task RunJSWebViewLocation(string latlon) {
        try {
            await browser.EvaluateJavaScriptAsync (new EvaluateJavaScriptAsyncRequest ($"XamarinFunctionLocation('{latlon}')"));
        } catch (Exception ex) {
            Debug.WriteLine($"Error: {ex.Message}", "Error [MAIN JSLOC]");
        }
    }

    public static async Task RunJSWebViewNotif (string data64Code) {
        try {
            await browser.EvaluateJavaScriptAsync (new EvaluateJavaScriptAsyncRequest ($"XamarinFunctionNotificationReceived(\"{data64Code}\")"));
        } catch (Exception ex) {
            Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN JSNOTIF]");
        }
    }

    /*public async Task RunJSWebViewIDDeviceNToken () {
        try {
            if (string.IsNullOrEmpty (Constantes.tokepush) || string.IsNullOrEmpty (Constantes.id_device)) {
                await _vm.LoadAsync ();
                Debug.WriteLine ($"TOKEN: {Constantes.tokepush}", "Firebase [MAIN JSDEVTOKEN IF]");
                Debug.WriteLine ($"DeviceID: {Constantes.id_device}", "ID [MAIN JSDEVTOKEN IF]");
            } else {
                Debug.WriteLine ($"TOKEN: {Constantes.tokepush}", "Firebase [MAIN JSDEVTOKEN ELSE]");
                Debug.WriteLine ($"DeviceID: {Constantes.id_device}", "ID [MAIN JSDEVTOKEN ELSE]");
            }
            string result = await browser.EvaluateJavaScriptAsync ($"XamarinFunctionTkenFCM('{Constantes.tokepush}')");
            result = await browser.EvaluateJavaScriptAsync ($"XamarinFunctionDeviceId('{Constantes.id_device}')");
            if (!Constantes.id_device.Equals (result)) {
                await RunJSWebViewIDDeviceNToken ();
                return;
            }
        } catch (Exception ex) {
            Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN JSDEVTOKEN]");
        }
    }

    public static async Task RunJSWebViewLocation (string latlon) {
        try {
            string result = await browser.EvaluateJavaScriptAsync ($"XamarinFunctionLocation('{latlon}')");
        } catch (Exception ex) {
            Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN JSLOC]");
        }
    }

    public static async Task RunJSWebViewNotif (string data64Code) {
        try {
            string result = await browser.EvaluateJavaScriptAsync ($"XamarinFunctionNotificationReceived(\"{data64Code}\")");
        } catch (Exception ex) {
            Debug.WriteLine ($"Error: {ex.Message}", "Error [MAIN JSNOTIF]");
        }
    }*/

    private static string ChangeData(double? value) {
        return value is null ? "0.0" : ((double)value).ToString().Contains(',') ? ((double)value).ToString().Replace(',', '.') : ((double)value).ToString();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
        await Clipboard.Default.SetTextAsync(!string.IsNullOrEmpty(((Editor)sender).Text) ? ((Editor)sender).Text : null);
        await DisplayAlert("Aviso", !string.IsNullOrEmpty(((Editor)sender).Text) ? $"Copiado al Portapapeles:\n\n{((Editor)sender).Text}" : "Portapapeles vacio", "OK");
    }
}