[CmdletBinding()]
Param(
    [string]$resourcesBaseName,
    [Parameter(Mandatory=$true)]
    $Action

)

$ResourceGroup = $resourcesBaseName + "-rg";
$ScaleSet = $resourcesBaseName + "-vmss";

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Get-AzureRmResourceGroup -Name $ResourceGroup -ErrorVariable notPresent -ErrorAction SilentlyContinue | Out-Null
if ( $notPresent) {
    "Resource group $ResourceGroup does not exist. Exiting script"
    exit
}

try {
    Get-AzureRmVmss -ResourceGroupName $ResourceGroup -VMScaleSetName $ScaleSet | Out-Null
}
catch {
    "Scale set $ScaleSet does not exist. Exiting script"
    exit
}

If ($Action -eq "Start") {
    Start-AzureRmVmss -ResourceGroupName $ResourceGroup -VMScaleSetName $ScaleSet
}
ElseIf ($Action -eq "Stop") {
    Stop-AzureRmVmss -ResourceGroupName $ResourceGroup -VMScaleSetName $ScaleSet -Force
}
Else {
    Write-Error "Unrecognized action $Action"
}
