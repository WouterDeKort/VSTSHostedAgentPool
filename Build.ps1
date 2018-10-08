[CmdletBinding()]
Param(
    $Location = $env:Location,
    $PackerFile = $env:Packerfile,
    $ClientId = $env:ClientId,
    $ClientSecret = $env:ClientSecret,
    $TenantId = $env:TenantId,
    $SubscriptionId = $env:SubscriptionId,
    $ObjectId = $env:ObjectId,
    $ManagedImageResourceGroupName = $env:ManagedImageResourceGroupName,
    $ManagedImageName = $env:ManagedImageName,
    [switch]$InstallPrerequisites
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($InstallPrerequisites) {
    "Installing prerequisites"
    Set-ExecutionPolicy Bypass -Scope Process -Force
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

    "Install Packer"
    choco install packer --version 1.2.5 -y
    "Install Git"
    choco install git -y

    "Install AzureRM PowerShell commands"
    Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
    Install-Module AzureRM -AllowClobber -Force
    Import-Module AzureRM
}

Get-AzureRmResourceGroup -Name $ManagedImageResourceGroupName -ErrorVariable notPresent -ErrorAction SilentlyContinue
if ( -Not $notPresent) {
    "Cleaning up previous image versions"
    Remove-AzureRmImage -ResourceGroupName $ManagedImageResourceGroupName -ImageName $ManagedImageName -Force
}

"Build Image"
if ($env:BUILD_REPOSITORY_LOCALPATH) {
    Set-Location $env:BUILD_REPOSITORY_LOCALPATH
}

$commitId = $(git log --pretty=format:'%H' -n 1)
"CommitId: $commitId"
  
packer build `
    -var "commit_id=$commitId" `
    -var "client_id=$ClientId" `
    -var "client_secret=$ClientSecret" `
    -var "tenant_id=$TenantId" `
    -var "subscription_id=$SubscriptionId" `
    -var "object_id=$ObjectId" `
    -var "location=$Location" `
    -var "managed_image_resource_group_name=$ManagedImageResourceGroupName" `
    -var "managed_image_name=$ManagedImageName" `
    -on-error=abort `
    $PackerFile

if ($LASTEXITCODE -eq 1){
    Write-Error "Packer build faild"
    exit 1
}