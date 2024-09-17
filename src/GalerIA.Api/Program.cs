using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure OpenAI client
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["AzureOpenAI:Endpoint"];
    var key = config["AzureOpenAI:ApiKey"];

    return string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key)
        ? throw new InvalidOperationException("Azure OpenAI configuration is missing or invalid.")
        : new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});

// Configure Azure Blob Storage client
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AzureBlobStorage:ConnectionString"];

    return string.IsNullOrEmpty(connectionString)
        ? throw new InvalidOperationException("Azure Blob Storage configuration is missing or invalid.")
        : new BlobServiceClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/images", async (CreateImageRequest request, OpenAIClient client, IConfiguration config) =>
{
    var prompt = $"capiara in a ${request.Prompt}, cartoon style";

    var deploymentName = config["AzureOpenAI:DeploymentName"];
    if (string.IsNullOrEmpty(deploymentName))
    {
        throw new InvalidOperationException("Azure OpenAI DeploymentName is missing or invalid.");
    }

    var imageGenerationOptions = new ImageGenerationOptions
    {
        Prompt = $"capiara in a ${prompt}, cartoon style",
        Size = ImageSize.Size1024x1024,
        DeploymentName = deploymentName
    };
    var imageResult = await client.GetImageGenerationsAsync(imageGenerationOptions);
    return Results.Created($"/images/${prompt}", new { imageResult.Value.Data[0].Url });
})
.WithName("GenerateImage")
.WithOpenApi();

app.MapPost("/gallery", async (AddToGalleryRequest request, BlobServiceClient blobServiceClient, IConfiguration config) =>
{
    // Download the image
    using var httpClient = new HttpClient();
    var imageBytes = await httpClient.GetByteArrayAsync(request.Url);

    // Upload to Azure Blob Storage
    var blobContainerClient = blobServiceClient.GetBlobContainerClient("gallery");
    await blobContainerClient.CreateIfNotExistsAsync();
    var blobName = $"{Guid.NewGuid()}.png";
    var blobClient = blobContainerClient.GetBlobClient(blobName);
    using var ms = new MemoryStream(imageBytes);
    await blobClient.UploadAsync(ms, overwrite: true);

    // Return the blob URL
    return Results.Created($"/gallery/${blobName}", new { blobClient.Uri });
})
.WithName("StoreImage")
.WithOpenApi();

app.MapGet("/gallery", async (BlobServiceClient blobServiceClient) =>
{
    var containerClient = blobServiceClient.GetBlobContainerClient("gallery");
    var images = new List<GalleryImage>();

    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    {
        var blobClient = containerClient.GetBlobClient(blobItem.Name);
        images.Add(new GalleryImage(blobItem.Name, blobClient.Uri.ToString()));
    }

    return Results.Ok(images);
})
.WithName("ListGalleryImages")
.WithOpenApi();

await app.RunAsync();

record CreateImageRequest (string Prompt);
record AddToGalleryRequest(string Url);
record GalleryImage(string Name, string Url);
