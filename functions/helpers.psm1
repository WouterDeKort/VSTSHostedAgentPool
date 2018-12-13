function SetCustomTagOnResource {
    param (
        $resourceId
    )

    process {
        Write-Verbose "Starting tags settings";
        $azureResourceInfo = Get-AzureRmResource -ResourceId $resourceId;
        Set-AzureRmResource -Tag @{ billingCategory="DevProductivity"; environment="Dev"; resourceType="AzureDevOps" } -ResourceName $azureResourceInfo.ResourceName -ResourceType $azureResourceInfo.resourceType -ResourceGroupName $azureResourceInfo.ResourceGroupName -Force;
        Write-Verbose "Ended tags settings"
    }
}