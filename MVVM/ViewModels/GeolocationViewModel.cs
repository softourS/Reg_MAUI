using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace PlannerBusConductoresN_MAUI.ViewModels;

public class GeolocationViewModel : ObservableObject  {
    const string notAvailable = "not available";
    string lastLocation;
    string currentLocation;
    int accuracy = (int)GeolocationAccuracy.High;
    string listeningLocation;
    string listeningLocationStatus;

    public GeolocationViewModel() {
        GetLastLocationCommand = new Command(OnGetLastLocation);
        GetCurrentLocationCommand = new Command(OnGetCurrentLocation);
        StartListeningCommand = new Command(OnStartListening);
        StopListeningCommand = new Command(OnStopListening);
    }

    public ICommand GetLastLocationCommand { get; }

    public ICommand GetCurrentLocationCommand { get; }

    public ICommand StartListeningCommand { get; }

    public ICommand StopListeningCommand { get; }

    public string LastLocation {
        get => lastLocation;
        set => SetProperty(ref lastLocation, value);
    }

    public string CurrentLocation {
        get => currentLocation;
        set => SetProperty(ref currentLocation, value);
    }

    public int Accuracy {
        get => accuracy;
        set => SetProperty(ref accuracy, value);
    }

    public bool IsListening => Geolocation.IsListeningForeground;

    public bool IsNotListening => !IsListening;

    public string ListeningLocation {
        get => listeningLocation;
        set => SetProperty(ref listeningLocation, value);
    }

    public string ListeningLocationStatus {
        get => listeningLocationStatus;
        set => SetProperty(ref listeningLocationStatus, value);
    }

    async void OnGetLastLocation() {
        try {
            LastLocation = FormatLocation(await Geolocation.GetLastKnownLocationAsync());
        } catch (Exception ex) {
            LastLocation = FormatLocation(null, ex);
        }
    }

    async void OnGetCurrentLocation() {
        try {
            GeolocationRequest request = new((GeolocationAccuracy)Accuracy, TimeSpan.FromSeconds(5));
            #if IOS
            request.RequestFullAccuracy = true;
            #endif
            CurrentLocation = FormatLocation(await Geolocation.GetLocationAsync(request));
        } catch (Exception ex) {
            CurrentLocation = FormatLocation(null, ex);
        }
    }

    async void OnStartListening() {
        try {
            GeolocationListeningRequest request = new((GeolocationAccuracy)Accuracy, TimeSpan.FromSeconds(5));
            ListeningLocationStatus = await Geolocation.StartListeningForegroundAsync(request) ?
                "Started listening for foreground location updates" : "Couldn't start listening";
            Geolocation.LocationChanged += (sender, e) => {
                ListeningLocation = FormatLocation(e.Location);
            };
        } catch (Exception ex) {
            ListeningLocationStatus = FormatLocation(null, ex);
        }
        OnPropertyChanged(nameof(IsListening));
        OnPropertyChanged(nameof(IsNotListening));
    }

    void OnStopListening() {
        try {
            Geolocation.LocationChanged -= (sender, e) => {
                ListeningLocation = FormatLocation(e.Location);
            };
            Geolocation.StopListeningForeground();
            ListeningLocationStatus = "Stopped listening for foreground location updates";
        } catch (Exception ex) {
            ListeningLocationStatus = FormatLocation(null, ex);
        }
        OnPropertyChanged(nameof(IsListening));
        OnPropertyChanged(nameof(IsNotListening));
    }

    static string FormatLocation(Location? location, Exception? ex = null) {
        if (location == null) {
            return $"Unable to detect location. Exception: {ex?.Message ?? string.Empty}";
        }
        return
            $"Latitude: {location.Latitude}\n" +
            $"Longitude: {location.Longitude}\n" +
            $"HorizontalAccuracy: {location.Accuracy}\n" +
            $"Altitude: {(location.Altitude.HasValue ? location.Altitude.Value.ToString() : notAvailable)}\n" +
            $"AltitudeRefSys: {location.AltitudeReferenceSystem}\n" +
            $"VerticalAccuracy: {(location.VerticalAccuracy.HasValue ? location.VerticalAccuracy.Value.ToString() : notAvailable)}\n" +
            $"Heading: {(location.Course.HasValue ? location.Course.Value.ToString() : notAvailable)}\n" +
            $"Speed: {(location.Speed.HasValue ? location.Speed.Value.ToString() : notAvailable)}\n" +
            $"Date (UTC): {location.Timestamp:d}\n" +
            $"Time (UTC): {location.Timestamp:T}\n" +
            $"Mocking Provider: {location.IsFromMockProvider}";
    }
}