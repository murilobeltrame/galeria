# Run this script as an administrator

# Function to check if a command exists
function Test-CommandExists {
    param ($command)
    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = 'stop'
    try { if (Get-Command $command) { return $true } }
    catch { return $false }
    finally { $ErrorActionPreference = $oldPreference }
}

# Install Chocolatey if not already installed
if (!(Test-CommandExists choco)) {
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
}

# Install Visual Studio 2022 (Community Edition) if not already installed
if (!(Test-Path "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe")) {
    choco install visualstudio2022community -y
}

# Install Visual Studio workloads
choco install visualstudio2022-workload-netcrossplat -y
choco install visualstudio2022-workload-manageddesktop -y
choco install visualstudio2022-workload-universal -y
choco install visualstudio2022-workload-aspnet -y  # ASP.NET Core workload

# Install .NET 8 SDK if not already installed
if (!(Test-CommandExists dotnet) -or !(dotnet --list-sdks | Select-String -Pattern "8.0" -Quiet)) {
    choco install dotnet-8.0-sdk -y
}

# Install Android SDK if not already installed
if (!(Test-Path "$env:LOCALAPPDATA\Android\Sdk")) {
    choco install android-sdk -y
}

# Install Java Development Kit (JDK) if not already installed
if (!(Test-Path "C:\Program Files\Java\jdk*")) {
    choco install jdk8 -y
}

# Install Git if not already installed
if (!(Test-CommandExists git)) {
    choco install git -y
}

# Install Visual Studio Code if not already installed
if (!(Test-CommandExists code)) {
    choco install vscode -y
}

# Install Azure CLI if not already installed
if (!(Test-CommandExists az)) {
    choco install azure-cli -y
}

# Refresh environment variables
refreshenv

# Install .NET MAUI workload
dotnet workload install maui

# Install VSCode extensions
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension ms-azuretools.vscode-azureappservice
code --install-extension ms-azuretools.vscode-azurefunctions
code --install-extension ms-azuretools.vscode-azurestorage
code --install-extension ms-azuretools.vscode-cosmosdb
code --install-extension ms-azuretools.vscode-azureresourcegroups
code --install-extension ms-dotnettools.dotnet-maui

Write-Host "Installation complete. Please restart your computer to ensure all changes take effect."
