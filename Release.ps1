[CmdletBinding()]
Param(
    [string]$VMUser = $env:VMUser,
    [string]$VMUserPassword = $env:VMUserPassword,
    [string]$VMName = $env:VMName,
    [string]$ManagedImageResourceGroupName = $env:ManagedImageResourceGroupName,
    [string]$ManagedImageName = $env:ManagedImageName,
    [string]$AgentPoolResourceGroup = $env:AgentPoolResourceGroup,
    [string]$Location = $env:Location,
    [string]$VSTSToken = $env:VSTSToken,
    [string]$VSTSUrl = $env:VSTSUrl
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Get-AzureRmResourceGroup -Name $AgentPoolResourceGroup -ev notPresent -ea 0

if (-Not $notPresent) {
    "Removing $AgentPoolResourceGroup"
    Remove-AzureRmResourceGroup -Name $AgentPoolResourceGroup -Force 
}

"Creating new resource group $AgentPoolResourceGroup"
New-AzureRmResourceGroup -Name $AgentPoolResourceGroup -Location $Location

"Create a virtual network subnet"
$subnet = New-AzureRmVirtualNetworkSubnetConfig `
    -Name "Subnet" `
    -AddressPrefix 10.0.0.0/24

"Create a virtual network"
$vnet = New-AzureRmVirtualNetwork `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name "Vnet" `
    -Location $Location `
    -AddressPrefix 10.0.0.0/16 `
    -Subnet $subnet `
    -Force

"Create a public IP address"
$publicIP = New-AzureRmPublicIpAddress `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Location $Location `
    -AllocationMethod Static `
    -Name "PublicIP" `
    -Force

"Create a frontend and backend IP pool"
$frontendIP = New-AzureRmLoadBalancerFrontendIpConfig `
    -Name "FrontEndPool" `
    -PublicIpAddress $publicIP
$backendPool = New-AzureRmLoadBalancerBackendAddressPoolConfig `
    -Name "BackEndPool"

"Create a Network Address Translation (NAT) pool"
$inboundNATPool = New-AzureRmLoadBalancerInboundNatPoolConfig `
    -Name "RDPRule" `
    -FrontendIpConfigurationId $frontendIP.Id `
    -Protocol TCP `
    -FrontendPortRangeStart 50001 `
    -FrontendPortRangeEnd 59999 `
    -BackendPort 3389

"Create the load balancer"
$lb = New-AzureRmLoadBalancer `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name "LoadBalancer" `
    -Location $Location `
    -FrontendIpConfiguration $frontendIP `
    -BackendAddressPool $backendPool `
    -InboundNatPool $inboundNATPool `
    -Force

"Create a load balancer health probe on port 80"
Add-AzureRmLoadBalancerProbeConfig -Name "HealthProbe" `
    -LoadBalancer $lb `
    -Protocol TCP `
    -Port 80 `
    -IntervalInSeconds 15 `
    -ProbeCount 2

"Create a load balancer rule to distribute traffic on port 80"
Add-AzureRmLoadBalancerRuleConfig `
    -Name "LoadBalancerRule" `
    -LoadBalancer $lb `
    -FrontendIpConfiguration $lb.FrontendIpConfigurations[0] `
    -BackendAddressPool $lb.BackendAddressPools[0] `
    -Protocol TCP `
    -FrontendPort 80 `
    -BackendPort 80

"Update the load balancer configuration"
Set-AzureRmLoadBalancer -LoadBalancer $lb

"Create IP address configurations"
$ipConfig = New-AzureRmVmssIpConfig `
    -Name "IPConfig" `
    -LoadBalancerBackendAddressPoolsId $lb.BackendAddressPools[0].Id `
    -LoadBalancerInboundNatPoolsId $inboundNATPool.Id `
    -SubnetId $vnet.Subnets[0].Id

"Create a config object"
$vmssConfig = New-AzureRmVmssConfig `
    -Location $Location `
    -SkuCapacity 1 `
    -SkuName "Standard_DS4_v3" `
    -UpgradePolicyMode Automatic

"Set the image created by Packer"
$image = Get-AzureRMImage -ImageName $ManagedImageName -ResourceGroupName $ManagedImageResourceGroupName
Set-AzureRmVmssStorageProfile $vmssConfig `
    -OsDiskCreateOption FromImage `
    -ManagedDisk StandardLRS `
    -OsDiskCaching "None" `
    -OsDiskOsType Windows `
    -ImageReferenceId $image.id

"Set up information for authenticating with the virtual machine"
Set-AzureRmVmssOsProfile $vmssConfig `
    -AdminUsername $VMUser `
    -AdminPassword $VMUserPassword `
    -ComputerNamePrefix $VMName

"Attach the virtual network to the config object"
Add-AzureRmVmssNetworkInterfaceConfiguration `
    -VirtualMachineScaleSet $vmssConfig `
    -Name "network-config" `
    -Primary $true `
    -IPConfiguration $ipConfig

"Create the scale set with the config object (this step might take a few minutes)"
New-AzureRmVmss `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name "ScaleSet" `
    -VirtualMachineScaleSet $vmssConfig

"Deploying Agent script to VM"

$StorageAccountName = "scriptstorage"
$ContainerName = "scripts"
    
$StorageAccountAvailability = Get-AzureRmStorageAccountNameAvailability -Name $StorageAccountName
    
if ($StorageAccountAvailability.NameAvailable) {
    "Creating storage account $StorageAccountName in $AgentPoolResourceGroup"
    New-AzureRmStorageAccount -ResourceGroupName $AgentPoolResourceGroup -AccountName $StorageAccountName -Location $Location -SkuName "Standard_LRS"
}
else {
    "Storage account $StorageAccountName in $AgentPoolResourceGroup already exists"
}
    
$StorageAccountKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $AgentPoolResourceGroup -Name $StorageAccountName).Value[0]
$StorageContext = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey
    
$container = Get-AzureStorageContainer -Context $StorageContext |  where-object {$_.Name -eq "scripts"}
if ( -Not $container) {
    "Creating container $ContainerName in $StorageAccountName"
    New-AzureStorageContainer -Name $ContainerName -Context $StorageContext -Permission blob
}
else {
    "Container $ContainerName in $StorageAccountName already exists"
}
    
$FileName = "AddAgentToVM.ps1";
$basePath = $PWD;
if ($env:SYSTEM_DEFAULTWORKINGDIRECTORY) {
    $basePath = "$env:SYSTEM_DEFAULTWORKINGDIRECTORY/VSTSHostedAgentPool"
}
$LocalFile = "$basePath/scripts\$FileName"
    
"Uploading file $LocalFile to $StorageAccountName"
Set-AzureStorageBlobContent `
    -Container $ContainerName `
    -Context $StorageContext `
    -File $Localfile `
    -Blob $Filename `
    -ErrorAction Stop -Force | Out-Null
    
$publicSettings = @{
    "fileUris"         = @("https://$StorageAccountName.blob.core.windows.net/$ContainerName/$FileName");
    "commandToExecute" = "PowerShell -ExecutionPolicy Unrestricted .\$FileName -VSTSToken $VSTSToken -VSTSUrl $VSTSUrl -windowsLogonAccount $VMUser -windowsLogonPassword $VMUserPassword";
};

"Get information about the scale set"
$vmss = Get-AzureRmVmss `
    -ResourceGroupName $AgentPoolResourceGroup `
    -VMScaleSetName "ScaleSet"

"Use Custom Script Extension to install VSTS Agent"
Add-AzureRmVmssExtension -VirtualMachineScaleSet $vmss `
    -Name "VSTS_Agent_Install" `
    -Publisher "Microsoft.Compute" `
    -Type "CustomScriptExtension" `
    -TypeHandlerVersion 1.8 `
    -ErrorAction Stop `
    -Setting $publicSettings

"Update the scale set and apply the Custom Script Extension to the VM instances"
Update-AzureRmVmss `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name "ScaleSet" `
    -VirtualMachineScaleSet $vmss
    
"Finished creating VM Scale Set and installing Agent"
