param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$true)]
    [string]$Location
)

# Set variables
$openAiName = "$ResourceGroupName-openai"
$storageAccountName = "$($ResourceGroupName -replace '-', '')storage"
$containerName = "gallery"
$deploymentName = "dall-e-3"
$appServicePlanName = "$ResourceGroupName-plan"
$webAppName = "$ResourceGroupName-api"

# Login to Azure (uncomment if not already logged in)
# az login

# Create resource group
Write-Host "Creating resource group: $ResourceGroupName in $Location"
az group create --name $ResourceGroupName --location $Location

# Create Azure OpenAI service
Write-Host "Creating Azure OpenAI service: $openAiName"
az cognitiveservices account create --name $openAiName --resource-group $ResourceGroupName --kind OpenAI --sku S0 --location $Location

# Deploy DALL-E 3 model to the OpenAI service
Write-Host "Deploying DALL-E 3 model to OpenAI service"
az cognitiveservices account deployment create --name $openAiName --resource-group $ResourceGroupName --deployment-name $deploymentName --model-name "dall-e-3" --model-version "latest" --model-format OpenAI

# Get OpenAI service details
$openAiKey = $(az cognitiveservices account keys list --name $openAiName --resource-group $ResourceGroupName --query key1 -o tsv)
$openAiEndpoint = $(az cognitiveservices account show --name $openAiName --resource-group $ResourceGroupName --query properties.endpoint -o tsv)

# Create Azure Storage account
Write-Host "Creating Azure Storage account: $storageAccountName"
az storage account create --name $storageAccountName --resource-group $ResourceGroupName --location $Location --sku Standard_LRS --kind StorageV2

# Get storage account connection string
$storageConnectionString = $(az storage account show-connection-string --name $storageAccountName --resource-group $ResourceGroupName --query connectionString -o tsv)

# Create Blob container with public access level set to "blob"
Write-Host "Creating Blob container: $containerName with public read access for blobs"
az storage container create --name $containerName --connection-string $storageConnectionString --public-access blob

# Get the storage account's blob service URL
$blobServiceUrl = "https://$storageAccountName.blob.core.windows.net"

# Create App Service Plan (Linux)
Write-Host "Creating App Service Plan: $appServicePlanName"
az appservice plan create --name $appServicePlanName --resource-group $ResourceGroupName --location $Location --sku B1 --is-linux

# Create Web App (.NET 8 on Linux)
Write-Host "Creating Web App: $webAppName"
az webapp create --name $webAppName --resource-group $ResourceGroupName --plan $appServicePlanName --runtime "DOTNETCORE:8.0"

# Configure Web App settings
Write-Host "Configuring Web App settings"
az webapp config appsettings set --name $webAppName --resource-group $ResourceGroupName --settings "AzureOpenAI:Endpoint=$openAiEndpoint" "AzureOpenAI:ApiKey=$openAiKey" "AzureOpenAI:DeploymentName=$deploymentName" "AzureBlobStorage:ConnectionString=$storageConnectionString" "AzureBlobStorage:BlobServiceUrl=$blobServiceUrl"

# Output configuration values
Write-Host "`nConfiguration values:"
Write-Host "AzureOpenAI:Endpoint: $openAiEndpoint"
Write-Host "AzureOpenAI:ApiKey: $openAiKey"
Write-Host "AzureOpenAI:DeploymentName: $deploymentName"
Write-Host "AzureBlobStorage:ConnectionString: $storageConnectionString"
Write-Host "AzureBlobStorage:BlobServiceUrl: $blobServiceUrl"
Write-Host "Web App Name: $webAppName"
Write-Host "Web App URL: https://$webAppName.azurewebsites.net"

Write-Host "`nProvisioning completed successfully."
Write-Host "Note: The 'gallery' container is now publicly readable. Blobs can be accessed directly from the internet using the URL format:"
Write-Host "$blobServiceUrl/$containerName/[BlobName]"
Write-Host "Ensure that you don't store sensitive information in this container."

Write-Host "`nTo deploy your API to the Web App, use the deploy-api.ps1 script:"
Write-Host ".\deploy-api.ps1 -ResourceGroupName $ResourceGroupName -WebAppName $webAppName -ProjectPath path/to/your/api/project"
Write-Host "Make sure to replace 'path/to/your/api/project' with the actual path to your API project."