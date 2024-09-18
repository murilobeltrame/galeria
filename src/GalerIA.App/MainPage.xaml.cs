using System.Collections.ObjectModel;
using System.Net.Http.Json;
using GalerIA.App.Pages;

namespace GalerIA.App;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public ObservableCollection<GalleryImage> Images { get; set; }

    public MainPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        Images = new ObservableCollection<GalleryImage>();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FetchImagesAsync();
    }

    private async Task FetchImagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://mbc-galeria-2-api.azurewebsites.net/gallery");
            if (response.IsSuccessStatusCode)
            {
                var images = await response.Content.ReadFromJsonAsync<List<GalleryImage>>();
                if (images != null && images.Any())
                {
                    Images.Clear();
                    foreach (var image in images)
                    {
                        Images.Add(image);
                    }
                    ImagesCollection.IsVisible = true;
                    NoContentLabel.IsVisible = false;
                }
                else
                {
                    ShowNoContentMessage();
                }
            }
            else
            {
                ShowNoContentMessage();
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            ShowNoContentMessage();
        }
    }

    private void ShowNoContentMessage()
    {
        Images.Clear();
        ImagesCollection.IsVisible = false;
        NoContentLabel.IsVisible = true;
    }

    private async void OnNewImageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewImagePage());
    }
}

public class GalleryImage
{
    public string Name { get; set; }
    public string Url { get; set; }
}
