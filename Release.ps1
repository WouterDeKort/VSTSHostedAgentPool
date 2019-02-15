[CmdletBinding()]
Param(
    [string]$VMUser = $env:VMUser,
    [string]$VMUserPassword = $env:VMUserPassword,
    #could not be longer than 9 symbols
    [string]$VMName = $env:VMName,
    [string]$ManagedImageResourceGroupName = $env:ManagedImageResourceGroupName,
    [string]$ManagedImageName = $env:ManagedImageName,
    [string]$Location = "West Europe",
    #used to construc other resources names
    [string]$resourcesBaseName,
    [string]$VSTSToken = $env:VSTSToken,
    [string]$VSTSUrl = $env:VSTSUrl,
    #if not specified otherwise - PIP is deployed at destroyable RG; otherwise it could be located at other RG, guaranteeing that it is left after reprovisioning
    [string]$pipRg,
    [int]$vmssCapacity = 1,
    [string]$vmssSkuName = "Standard_D4s_v3",
    [string]$vstsPoolName = "Default",
    [string]$vstsAgentPackageUri = "https://vstsagentpackage.azureedge.net/agent/2.140.2/vsts-agent-win-x64-2.140.2.zip",
    [string]$vmssDiskStorageAccount = "StandardSSD_LRS",
    [int]$vmssDataDiskSize = 64,
    #by default we will attach a dataDisk
    [bool]$attachDataDisk = $true,
    #we want to be as secured as possible
    [bool]$attachNsg = $true,
    #Provide an address range using CIDR notation (e.g. 192.168.99.0/24); an IP address (e.g. 192.168.99.0); or a list of address ranges or IP addresses (e.g. 192.168.99.0/24,10.0.0.0/24,44.66.0.0/24).
    [string]$allowedIps,
    #Provide a single port, such as 80; a port range, such as 1024-65535; or a comma-separated list of single ports and/or port ranges, such as 80,1024-65535. This specifies on which ports traffic will be allowed or denied by this rule. Provide an asterisk (*) to allow traffic on any port.
    [string]$allowedPorts = "3389",
    #defines, if we shall deploy to existing VNet or to provision new VNet
    [bool]$deployToExistingVnet = $false,
    #defining names for subnet and vnet
    [string]$subnetName = $resourcesBaseName + "-subnet",
    [string]$vnetName = $resourcesBaseName + "-vnet",
    #VNet RG must be defined, as we are destroying RG with Agent each time
    [string]$vnetResourceGroupName
)


Import-Module $PSScriptRoot\functions\helpers.psm1
#Construct resources names
$AgentPoolResourceGroup = GenerateResourceGroupName -baseName $resourcesBaseName;
$pipName = $resourcesBaseName + "-pip";
if ([string]::IsNullOrWhiteSpace($pipRg)) {
    #public IP resource group have not been specified -> deploying in renewable one
    $pipRg = $AgentPoolResourceGroup;
}
$lbName = $resourcesBaseName + "-lb";
$vmssScaleSetName = GenerateVmssName -baseName $resourcesBaseName;

if ([string]::IsNullOrWhiteSpace($allowedIps)) {
    #allowed IPs is not defined - so, we could not deploy an NSG
    $attachNsg = $false;
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Get-AzureRmResourceGroup -Name $AgentPoolResourceGroup -ev notPresent -ea 0

if (-Not $notPresent) {
    "Removing $AgentPoolResourceGroup"
    Remove-AzureRmResourceGroup -Name $AgentPoolResourceGroup -Force
}

Write-Host "Creating new resource group $AgentPoolResourceGroup"
New-AzureRmResourceGroup -Name $AgentPoolResourceGroup -Location $Location

if ($deployToExistingVnet -and (-not [string]::IsNullOrWhiteSpace($vnetResourceGroupName))) {
    $vnet = Get-AzureRmVirtualNetwork -ResourceGroupName $vnetResourceGroupName -Name $vnetName;
    $subnet = Get-AzureRmVirtualNetworkSubnetConfig -VirtualNetwork $vnet -Name $subnetName;
} else {
    Write-Host "Create a virtual network subnet"
    $subnet = New-AzureRmVirtualNetworkSubnetConfig `
        -Name $subnetName `
        -AddressPrefix 10.0.0.0/24
    
    Write-Host "Create a virtual network"
    $vnet = New-AzureRmVirtualNetwork `
        -ResourceGroupName $AgentPoolResourceGroup `
        -Name $vnetName `
        -Location $Location `
        -AddressPrefix 10.0.0.0/16 `
        -Subnet $subnet `
        -Force    
}

Get-AzureRmPublicIpAddress -Name $pipName -ResourceGroupName $pipRg -ev pipNotPresent -ea 0;
if ($pipNotPresent){
    Write-Host "Create a public IP address"
    $publicIP = New-AzureRmPublicIpAddress `
        -ResourceGroupName $pipRg `
        -Location $Location `
        -AllocationMethod Static `
        -Name $pipName `
        -Force
} else {
    $publicIP = Get-AzureRmPublicIpAddress -Name $pipName -ResourceGroupName $pipRg
}

Write-Host "Create a frontend and backend IP pool"
$frontendIP = New-AzureRmLoadBalancerFrontendIpConfig `
    -Name "FrontEndPool" `
    -PublicIpAddress $publicIP
$backendPool = New-AzureRmLoadBalancerBackendAddressPoolConfig `
    -Name "BackEndPool"

Write-Host "Create a Network Address Translation (NAT) pool"
$inboundNATPool = New-AzureRmLoadBalancerInboundNatPoolConfig `
    -Name "RDPRule" `
    -FrontendIpConfigurationId $frontendIP.Id `
    -Protocol TCP `
    -FrontendPortRangeStart 50001 `
    -FrontendPortRangeEnd 59999 `
    -BackendPort 3389

Write-Host "Create the load balancer"
$lb = New-AzureRmLoadBalancer `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name $lbName `
    -Location $Location `
    -FrontendIpConfiguration $frontendIP `
    -BackendAddressPool $backendPool `
    -InboundNatPool $inboundNATPool `
    -Force

Write-Host "Create a load balancer health probe on port 80"
Add-AzureRmLoadBalancerProbeConfig -Name "HealthProbe" `
    -LoadBalancer $lb `
    -Protocol TCP `
    -Port 80 `
    -IntervalInSeconds 15 `
    -ProbeCount 2

Write-Host "Create a load balancer rule to distribute traffic on port 80"
Add-AzureRmLoadBalancerRuleConfig `
    -Name "LoadBalancerRule" `
    -LoadBalancer $lb `
    -FrontendIpConfiguration $lb.FrontendIpConfigurations[0] `
    -BackendAddressPool $lb.BackendAddressPools[0] `
    -Protocol TCP `
    -FrontendPort 80 `
    -BackendPort 80

Write-Host "Update the load balancer configuration"
Set-AzureRmLoadBalancer -LoadBalancer $lb

Write-Host "Create IP address configurations"
$ipConfig = New-AzureRmVmssIpConfig `
    -Name "IPConfig" `
    -LoadBalancerBackendAddressPoolsId $lb.BackendAddressPools[0].Id `
    -LoadBalancerInboundNatPoolsId $inboundNATPool.Id `
    -SubnetId $subnet.Id

Write-Host "Create a config object"
$vmssConfig = New-AzureRmVmssConfig `
    -Location $Location `
    -SkuCapacity $vmssCapacity `
    -SkuName $vmssSkuName `
    -UpgradePolicyMode Automatic

"Set the image created by Packer"
$image = Get-AzureRMImage -ImageName $ManagedImageName -ResourceGroupName $ManagedImageResourceGroupName
Set-AzureRmVmssStorageProfile $vmssConfig `
    -OsDiskCreateOption FromImage `
    -ManagedDisk $vmssDiskStorageAccount `
    -OsDiskCaching "None" `
    -OsDiskOsType Windows `
    -ImageReferenceId $image.id

Write-Host "Set up information for authenticating with the virtual machine"
Set-AzureRmVmssOsProfile $vmssConfig `
    -AdminUsername $VMUser `
    -AdminPassword $VMUserPassword `
    -ComputerNamePrefix $VMName

Write-Host "Attach the virtual network to the config object"

if ($attachNsg) {
    Write-Host "Attaching Network Security group to VMSS";
    #we want to attach NSG to NIC
    #define a name for it first
    $nsgName = $resourcesBaseName + "-nsg";
    #create NSG
    $nsg = New-AzureRmNetworkSecurityGroup -Name $nsgName -ResourceGroupName $AgentPoolResourceGroup -Location $Location;
    #add rule to allow ports
    $nsg | Add-AzureRmNetworkSecurityRuleConfig -Name Allow_Ports -Access Allow -Protocol Tcp -Direction Inbound -Priority 110 -SourceAddressPrefix $allowedIps.Split(',') -SourcePortRange * -DestinationAddressPrefix * -DestinationPortRange $allowedPorts.Split(',') | Set-AzureRmNetworkSecurityGroup

    #add NIC to VMSS
    Add-AzureRmVmssNetworkInterfaceConfiguration `
    -VirtualMachineScaleSet $vmssConfig `
    -Name "network-config" `
    -Primary $true `
    -IPConfiguration $ipConfig `
    -NetworkSecurityGroupId $nsg.Id
} else {
    Add-AzureRmVmssNetworkInterfaceConfiguration `
    -VirtualMachineScaleSet $vmssConfig `
    -Name "network-config" `
    -Primary $true `
    -IPConfiguration $ipConfig
}

Write-Host "Create the scale set with the config object (this step might take a few minutes)"
New-AzureRmVmss `
-ResourceGroupName $AgentPoolResourceGroup `
-Name $vmssScaleSetName `
-VirtualMachineScaleSet $vmssConfig
if ($attachDataDisk) {
    $vmss = Get-AzureRmVmss `
        -ResourceGroupName $AgentPoolResourceGroup `
        -VMScaleSetName $vmssScaleSetName;
    Add-AzureRmVmssDataDisk `
        -VirtualMachineScaleSet $vmss `
        -CreateOption Empty `
        -Lun 0 `
        -DiskSizeGB $vmssDataDiskSize `
        -StorageAccountType $vmssDiskStorageAccount
    Update-AzureRmVmss `
      -ResourceGroupName $AgentPoolResourceGroup `
      -Name $vmssScaleSetName `
      -VirtualMachineScaleSet $vmss
}


Write-Host "Deploying Agent script to VM"

$StorageAccountName = $resourcesBaseName + "storage"
$StorageAccountName = $StorageAccountName -replace '-',''
$ContainerName = "scripts"

$StorageAccountAvailability = Get-AzureRmStorageAccountNameAvailability -Name $StorageAccountName

if ($StorageAccountAvailability.NameAvailable) {
    Write-Host "Creating storage account $StorageAccountName in $ManagedImageResourceGroupName"
    $storage = New-AzureRmStorageAccount -ResourceGroupName $ManagedImageResourceGroupName -AccountName $StorageAccountName -Location $Location -SkuName "Standard_LRS"
}
else {
    Write-Host "Storage account $StorageAccountName in $ManagedImageResourceGroupName already exists"
    $storage = Get-AzureRmStorageAccount -ResourceGroupName $ManagedImageResourceGroupName -Name $StorageAccountName;
}

$StorageAccountKey = (Get-AzureRmStorageAccountKey -ResourceGroupName $ManagedImageResourceGroupName -Name $StorageAccountName).Value[0]
$StorageContext = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $StorageAccountKey

$container = Get-AzureStorageContainer -Context $StorageContext |  where-object {$_.Name -eq "scripts"}
if ( -Not $container) {
    Write-Host "Creating container $ContainerName in $StorageAccountName"
    New-AzureStorageContainer -Name $ContainerName -Context $StorageContext -Permission blob
}
else {
    Write-Host "Container $ContainerName in $StorageAccountName already exists"
}

$FileName = "AddAgentToVM.ps1";
$currentDatePostfix = Get-Date -format "MMddyyyyHHmm";
$blobName = "addAgentToVM" + $currentDatePostfix + ".ps1"
$basePath = $PSScriptRoot;
#if ($env:SYSTEM_DEFAULTWORKINGDIRECTORY) {
    #$basePath = "$env:SYSTEM_DEFAULTWORKINGDIRECTORY/VSTSHostedAgentPool"
#}
$LocalFile = "$basePath/scripts\$FileName"

Write-Host "Uploading file $LocalFile to $StorageAccountName"
Set-AzureStorageBlobContent `
    -Container $ContainerName `
    -Context $StorageContext `
    -File $Localfile `
    -Blob $blobName `
    -ErrorAction Stop -Force | Out-Null

$publicSettings = @{
    "fileUris"         = @("https://$StorageAccountName.blob.core.windows.net/$ContainerName/$blobName");
    "commandToExecute" = "PowerShell -ExecutionPolicy Unrestricted .\$blobName -VSTSToken $VSTSToken -VSTSUrl $VSTSUrl -windowsLogonAccount $VMUser -windowsLogonPassword $VMUserPassword -poolName $vstsPoolName -vstsAgentPackageUri $vstsAgentPackageUri -prepareDataDisks $attachDataDisk";
};

Write-Host "Get information about the scale set"
$vmss = Get-AzureRmVmss `
    -ResourceGroupName $AgentPoolResourceGroup `
    -VMScaleSetName $vmssScaleSetName

Write-Host "Use Custom Script Extension to install VSTS Agent"
Add-AzureRmVmssExtension -VirtualMachineScaleSet $vmss `
    -Name "VSTS_Agent_Install" `
    -Publisher "Microsoft.Compute" `
    -Type "CustomScriptExtension" `
    -TypeHandlerVersion 1.8 `
    -ErrorAction Stop `
    -Setting $publicSettings

Write-Host "Update the scale set and apply the Custom Script Extension to the VM instances"
Update-AzureRmVmss `
    -ResourceGroupName $AgentPoolResourceGroup `
    -Name $vmssScaleSetName `
    -VirtualMachineScaleSet $vmss

Write-Host "Setting tags on created or existing resources";    
SetCustomTagOnResource -resourceId $vmss.Id -resourceName $vmssScaleSetName;
SetCustomTagOnResource -resourceId $lb.Id -resourceName $lbName;
SetCustomTagOnResource -resourceId $publicIP.Id -resourceName $pipName;
SetCustomTagOnResource -resourceId $image.Id -resourceName $ManagedImageName;
if ($attachNsg) {
    SetCustomTagOnResource -resourceId $nsg.Id -resourceName $nsgName;
}
SetCustomTagOnResource -resourceId $storage.Id -resourceName $StorageAccountName;
if (-not $deployToExistingVnet) {
    #we shall set custom tags on vnet only in case we are deploying it ourselves
    SetCustomTagOnResource -resourceId $vnet.Id -resourceName $vnetName;
}


Write-Host "Finished creating VM Scale Set and installing Agent"
