using GalerIA.App.Pages;

namespace GalerIA.App;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private async void OnNewImageClicked(object sender, EventArgs e) {
        await Navigation.PushAsync(new NewImagePage());
    }

}

