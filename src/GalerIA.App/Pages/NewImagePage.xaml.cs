using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GalerIA.App.Pages;

public partial class NewImagePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private string _generatedImageUrl;

    public NewImagePage()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://mbc-galeria-2-api.azurewebsites.net/")
        };
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
        AddToGalleryButton.IsEnabled = false;
        _generatedImageUrl = await GenerateImage(Prompt.Text);
        
        if (!string.IsNullOrEmpty(_generatedImageUrl))
        {
            ImagePreview.Source = _generatedImageUrl;
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
            var response = await _httpClient.PostAsJsonAsync("images", new { prompt });
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
        if (!string.IsNullOrEmpty(_generatedImageUrl))
        {
            await AddToGallery(_generatedImageUrl);
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to add image to gallery", "OK");
        }
    }

    private async Task AddToGallery(string url)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("gallery", new { url });
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }
}