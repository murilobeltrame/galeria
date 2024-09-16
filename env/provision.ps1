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

# Create Blob container
Write-Host "Creating Blob container: $containerName"
az storage container create --name $containerName --connection-string $storageConnectionString

# Output configuration values
Write-Host "`nConfiguration values for appsettings.json:"
Write-Host "AzureOpenAI:Endpoint: $openAiEndpoint"
Write-Host "AzureOpenAI:ApiKey: $openAiKey"
Write-Host "AzureOpenAI:DeploymentName: $deploymentName"
Write-Host "AzureBlobStorage:ConnectionString: $storageConnectionString"

Write-Host "`nProvisioning completed successfully."