 
[CmdletBinding()]
Param(
    [string]$VSTSToken = $env:VSTSToken,
    [string]$VSTSUrl = $env:VSTSUrl,
    $agentPoolPattern = "AgentVM"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$apiVersion = "3.0-preview.1"

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "", $VSTSToken)))

$uri = "${VSTSUrl}/_apis/distributedtask/pools?api-version=${apiVersion}"

"Calling $uri"

$allPoolsResult = Invoke-RestMethod -Uri $uri -Method Get -ContentType "application/json" -Headers @{Authorization = ("Basic {0}" -f $base64AuthInfo)}
$allPoolsResult 

foreach ($poolRec in $allPoolsResult.value) {
 
    "Processing Agent Pool $poolRec.name"
 
    # Get agents of an agent pool (Request method: Get):  
    $uri = "${VSTSUrl}/_apis/distributedtask/pools/$( $poolRec.id )/agents?api-version=${apiVersion}"
    $thisPoolResult = Invoke-RestMethod -Uri $uri -Method Get -ContentType "application/json" -Headers @{Authorization = ("Basic {0}" -f $base64AuthInfo)}
 
    foreach ($agentRec in $thisPoolResult.value) {
 
        "Processing Agent $agentRec.name"
 
        if ($agentRec.name -match $agentPoolPattern) {
            if ($agentRec.status -eq "offline") {
                Write-Host "Deleting Agent '$( $agentRec.name )'" -ForegroundColor Red
 
                #Delete an agent from an agent pool (Request method: Delete):
                $uri = "${VSTSUrl}/_apis/distributedtask/pools/$( $poolRec.id )/agents/$( $agentRec.id )?api-version=${apiVersion}"
                Invoke-RestMethod -Uri $uri -Method Delete -ContentType "application/json" -Headers @{Authorization = ("Basic {0}" -f $base64AuthInfo)}
            }
        }
    }
}
