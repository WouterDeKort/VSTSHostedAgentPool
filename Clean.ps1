[CmdletBinding()]
Param(
    [string]$ManagedImageName,
    [string]$ManagedImageResourceGroupName,
    [string]$AgentPoolResourceGroup,
    [switch]$RemovePackerResourceGroups,
    [switch]$RemoveManagedImages,
    [switch]$RemoveAgentPoolResourceGroup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ( $RemovePackerResourceGroups) {
    "Removing all temporary Packer resource groups"
    Get-AzureRmResourceGroup | Where-Object ResourceGroupName -like packer-resource-group-* | Remove-AzureRmResourceGroup -Force
}
else {
    "Skip removing Packer resource groups"
}

if ( $RemoveManagedImages) {
    "Remove Managed Image $ManagedImageName in $ManagedImageResourceGroupName"
    Remove-AzureRmImage -ResourceGroupName $ManagedImageResourceGroupName -ImageName $ManagedImageName -Force
}
else {
    "Skip removing managed images"
}

if ( $RemoveAgentPoolResourceGroup) {
    "Remove agent pool resource group $AgentPoolResourceGroup"

    Get-AzureRmResourceGroup -Name $AgentPoolResourceGroup -ev notPresent -ea 0

    if (-Not $notPresent) {
        Remove-AzureRmResourceGroup -Name $AgentPoolResourceGroup -Force
    }
}
else {
    "Skip removing agent pool resource group"
}