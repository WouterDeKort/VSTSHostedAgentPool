function SetCustomTagOnResource {
    param (
        $resourceId,
        #Get-AzureRmResource does not fetches us resource name :(
        $resourceName
    )

    process {
        Write-Verbose "Starting tags settings for resource $resourceId";
        $azureResourceInfo = Get-AzureRmResource -ResourceId $resourceId -ev resourceNotPresent -ea 0;
        #do not why, but resource retrieval fails sometimes
        if ($resourceNotPresent) {
            Write-Verbose "Could not get resource for $resourceId";
        } 
        else 
        {
            $rType = $azureResourceInfo.resourceType;
            $rRgName = $azureResourceInfo.ResourceGroupName;
            Write-Verbose "Settings tags for $resourceId named $resourceName, belonging to type $rType in resource group $rRgName";
            Set-AzureRmResource -Tag @{ billingCategory="DevProductivity"; environment="Dev"; resourceType="AzureDevOps" } -ResourceName $resourceName -ResourceType $rType -ResourceGroupName $rRgName -Force;
        }

        Write-Verbose "Ended tags settings"
    }
}


function GenerateResourceGroupName {
    param (
        $baseName
    )

    $generatedName = $baseName + "-rg";
    Write-Verbose "GenerateResourceGroupName: resource group name is $generatedName";
    return $generatedName;
}

function GenerateVmssName {
    param (
        $baseName
    )

    $generatedName = $baseName + "-vmss";
    Write-Verbose "GenerateVmssName: VMSS name is $generatedName";
    return $generatedName;
}