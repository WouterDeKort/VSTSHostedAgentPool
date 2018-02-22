[CmdletBinding()]
Param(
    [string]$ResourceGroup,
    [string]$ScaleSet = "ScaleSet",
    [Parameter(Mandatory=$true)]
    $Action

)

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
    Write-Error "Unregonized action $Action"
}
