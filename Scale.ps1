[CmdletBinding()]
Param(
    [string]$AgentPoolResourceGroup = $env:AgentPoolResourceGroup,
    $Capacity = $env:vmssCapacity
)

Set-StrictMode -Version Latest

"Get current scale set"
$vmss = Get-AzureRmVmss -ResourceGroupName $AgentPoolResourceGroup -VMScaleSetName "ScaleSet"

"Set and update the capacity of your scale set"
$vmss.sku.capacity = $Capacity
Update-AzureRmVmss -ResourceGroupName $AgentPoolResourceGroup -Name "ScaleSet" -VirtualMachineScaleSet $vmss