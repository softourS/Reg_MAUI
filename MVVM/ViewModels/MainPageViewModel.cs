using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AppCenter.Crashes;
using PlannerBusConductoresN_MAUI.Models.Permissions;
using Plugin.Firebase.CloudMessaging;
#if ANDROID
using Android.Provider;
#elif IOS
using UIKit;
#endif

namespace PlannerBusConductoresN_MAUI.ViewModels;

public partial class MainPageViewModel(ILogger<MainPageViewModel> logger, IConnectivity connectivity) : ObservableObject {
    private readonly ILogger<MainPageViewModel> logger = logger;
    private readonly IConnectivity connectivity = connectivity;

    [ObservableProperty]
    private string _token = string.Empty;
    [ObservableProperty]
    private string _iddevice = string.Empty;

    public async Task LoadAsync() {
        try {
            if (connectivity.NetworkAccess != NetworkAccess.Internet) {
                await Shell.Current.DisplayAlert("Error de conexión", $"Compruebe su conexión e inténtelo de nuevo.", "OK");
                return;
            }
            if (!CrossFirebaseCloudMessaging.IsSupported) {
                logger.LogWarning("Firebase no soportado en este dispositivo");
                return;
            }
            PermissionStatus status = await Permissions.CheckStatusAsync<NotificationPermission>();
            if (status != PermissionStatus.Granted) {
                status = await Permissions.RequestAsync<NotificationPermission>();
            }
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            string firebaseToken = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (string.IsNullOrEmpty(firebaseToken)) {
                throw new InvalidOperationException("Firebase token could not be retrieved");
            } else {
                logger.LogInformation("Firebase token {token}", firebaseToken);

                Token = firebaseToken;
                Constantes.tokepush = Token;

                Iddevice = GetID();
                Constantes.id_device = Iddevice;
            }
        } catch (Exception ex) {
            Crashes.TrackError(ex);
            logger.LogError(ex, "Error getting firebase token: {message}", ex.Message);
        }
    }

    private static string GetID() {
        string id = "";
#if ANDROID
        id = Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Settings.Secure.AndroidId) ?? "";
#elif IOS
        id = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
#endif
        return id;
    }
}