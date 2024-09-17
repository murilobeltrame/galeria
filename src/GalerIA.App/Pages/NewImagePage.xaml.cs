using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GalerIA.App.Pages;

public partial class NewImagePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public NewImagePage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        
        // Set the API base URL based on the current device/platform
        _apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:7033" // Android emulator
            : "http://localhost:7033"; // Windows, macOS, or iOS simulator
        
        #if DEBUG
        if (DeviceInfo.DeviceType == DeviceType.Physical)
        {
            // For physical devices, use your computer's IP address
            _apiBaseUrl = "http://YOUR_COMPUTER_IP:7033";
            // TODO: Replace YOUR_COMPUTER_IP with your actual IP address when debugging on a physical device
        }
        #endif
    }

    private void OnPromptEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        GenerateImageButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
    }

    private async void OnGenerateImageClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Prompt.Text))
        {
            await DisplayAlert("Error", "Please enter a prompt", "OK");
            return;
        }

        GenerateImageButton.IsEnabled = false;
        string imageUrl = await GenerateImage(Prompt.Text);
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            ImagePreview.Source = imageUrl;
            AddToGalleryButton.IsEnabled = true;
        }
        else
        {
            await DisplayAlert("Error", "Failed to generate image", "OK");
        }
        
        GenerateImageButton.IsEnabled = true;
    }

    private async Task<string> GenerateImage(string prompt)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/images", new { prompt });
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(content);
            return responseObject.GetProperty("url").GetString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            return null;
        }
    }

    private async void OnAddToGalleryClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}