# Adicionando comportamento ao aplicativo.

- [Navagando para a próxima página.](#navegando-para-a-próxima-página)
- [Habilitando botão gerar conforme digitação do usuário](#habilitando-botão-gerar-conforme-digitação-do-usuario)
- [Gerando a imagem](#gerando-a-imagem)
- [Adicionando a imagem na galeria](#adicionando-a-imagem-na-galeria)
- [Carregando as imagens](#carregando-as-imagens)

## Navegando para a próxima página.

No arquivo [MainPage.xaml.cs](./MainPage.xaml.cs), adicione o seguinte código:

```csharp
public partial class MainPage : ContentPage
{
    // omitido para brevidade
    private async void OnNewImageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewImagePage());
    }
    // omitido para brevidade
}
```

No arquivo [MainPage.xaml](./MainPage.xaml), adicione o seguinte código:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage ...>
    <Grid ...>
        <!-- Omitido para brevidade -->
        <Button Grid.Row="1" Text="Nova image" Clicked="OnNewImageClicked" />
        <!-- Omitido para brevidade -->
    </Grid>
</ContentPage>
```

## Habilitando botão gerar conforme digitação do usuário

No arquivo [NewImagePage.xaml.cs](./Pages/NewImagePage.xaml.cs), adicione o seguinte código:

```csharp
public partial class NewImagePage : ContentPage
{
    // omitido para brevidade
    private void OnPromptEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        GenerateImageButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
    }
    // omitido para brevidade
}
```

No arquivo [NewImagePage.xaml](./Pages/NewImagePage.xaml), adicione o seguinte código:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage ...>
    <VerticalStackLayout ...>
        <Entry x:Name="Prompt" 
            Placeholder="Descreva um cenário aqui ..." 
            TextChanged="OnPromptEntryTextChanged" />
        <!-- Omitido para brevidade -->
    </VerticalStackLayout>
</ContentPage>
```

## Gerando a imagem

No arquivo [NewImagePage.xaml.cs](./Pages/NewImagePage.xaml.cs), adicione o seguinte código:

```csharp
public partial class NewImagePage : ContentPage
{
    private readonly HttpClient _httpClient;

    public NewImagePage()
    {
        // omitido para brevidade
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://mbc-galeria-2-api.azurewebsites.net/")
        };
    }

    // omitido para brevidade
}
```
...configurando o cliente HTTP com a URL do serviço.

```csharp
public partial class NewImagePage : ContentPage
{
    // omitido para brevidade
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
    // omitido para brevidade
}
```
... criando o método de acesso à API que gera a imagem.
```csharp
public partial class NewImagePage : ContentPage
{
    private string _generatedImageUrl;

    // omitido para brevidade
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
    // omitido para brevidade
}
```
... criando o manipulador de evento. <br />
No arquivo [NewImagePage.xaml](./Pages/NewImagePage.xaml), adicione o seguinte código:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage ...>
    <VerticalStackLayout ...>
        <!-- Omitido para brevidade -->
        <Button x:Name="GenerateImageButton" 
            Text="Gerar" 
            IsEnabled="False"
            Clicked="OnGenerateImageClicked" />
        <!-- Omitido para brevidade -->
    </VerticalStackLayout>
</ContentPage>
```

## Adicionando a imagem na galeria

No arquivo [NewImagePage.xaml.cs](./Pages/NewImagePage.xaml.cs), adicione o seguinte código:

```csharp
public partial class NewImagePage : ContentPage
{
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
```
```csharp
public partial class NewImagePage : ContentPage
{
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
}
```

No arquivo [NewImagePage.xaml](./Pages/NewImagePage.xaml), adicione o seguinte código:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage ...>
    <VerticalStackLayout ...>
        <!-- Omitido para brevidade -->
        <Button x:Name="AddToGalleryButton" 
            Text="Adicionar à galeria" 
            IsEnabled="False"
            Clicked="OnAddToGalleryClicked" />
    </VerticalStackLayout>
</ContentPage>
```

## Carregando as imagens

No arquivo [MainPage.xaml.cs](./MainPage.xaml.cs), adicione o seguinte código:

```csharp
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
```

No arquivo [MainPage.xaml](./MainPage.xaml), adicione o seguinte código:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage ...>
    <Grid ...>
        <!-- Omitido para brevidade -->
        <CollectionView x:Name="ImagesCollection" Grid.Row="0"
                        ItemsSource="{Binding Images}"
                        EmptyView="No images available.">
            <CollectionView.ItemsLayout>
                <GridItemsLayout Orientation="Vertical"
                                 Span="3"
                                 VerticalItemSpacing="10"
                                 HorizontalItemSpacing="10" />
            </CollectionView.ItemsLayout>
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Image Source="{Binding Url}" 
                           Aspect="AspectFill"
                           HeightRequest="150"
                           WidthRequest="150" />
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        <!-- Omitido para brevidade -->
    </Grid>
</ContentPage>
```