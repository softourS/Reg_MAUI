namespace PlannerBusConductoresN_MAUI.Views;

public partial class GeolocationPage : ContentPage {
	public GeolocationPage(GeolocationViewModel vm) {
        InitializeComponent();
        BindingContext = vm;
    }
}