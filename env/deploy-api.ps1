param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$true)]
    [string]$WebAppName,

    [Parameter(Mandatory=$true)]
    [string]$ProjectPath
)

# Ensure the project path exists
if (-not (Test-Path $ProjectPath)) {
    Write-Error "The specified project path does not exist: $ProjectPath"
    exit 1
}

# Build the project
Write-Host "Building the API project..."
dotnet publish $ProjectPath -c Release -o ./publish /p:UseAppHost=false

# Create a deployment package (zip)
Write-Host "Creating deployment package..."
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# Deploy the package to the Azure Web App
Write-Host "Deploying to Azure Web App..."
az webapp deployment source config-zip --resource-group $ResourceGroupName --name $WebAppName --src ./deploy.zip

# Clean up
Remove-Item -Path ./publish -Recurse -Force
Remove-Item -Path ./deploy.zip -Force

Write-Host "Deployment completed successfully."
Write-Host "Your API is now available at: https://$WebAppName.azurewebsites.net"

# Restart the Web App to ensure all changes are applied
Write-Host "Restarting the Web App..."
az webapp restart --name $WebAppName --resource-group $ResourceGroupName

Write-Host "Web App restarted. Deployment process complete."