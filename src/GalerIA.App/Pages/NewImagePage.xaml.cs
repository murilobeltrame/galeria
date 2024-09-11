namespace GalerIA.App.Pages;

public partial class NewImagePage : ContentPage
{
	public NewImagePage()
	{
		InitializeComponent();
	}

    private void OnPromptEntryTextChanged(object sender, TextChangedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(e.NewTextValue)) {
            GenerateImageButton.IsEnabled = true;
        }
    }

    private void OnGenerateImageClicked(object sender, EventArgs e) {
        ImagePreview.Source = "dotnet_bot.png";
        AddToGalleryButton.IsEnabled = true;
    }

    private async void OnAddToGalleryClicked(object sender, EventArgs e) {
        await Navigation.PopAsync();
    }
}