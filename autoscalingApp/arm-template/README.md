# Deploys web app with required settings defined

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https://github.com/akuryan/VSTSHostedAgentPool/tree/master/autoscalingApp/arm-template/azuredeploy.json" target="_blank">
<img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.png"/>
</a>
<a href="http://armviz.io/#/?load=https://github.com/akuryan/VSTSHostedAgentPool/tree/master/autoscalingApp/arm-template/azuredeploy.json" target="_blank">
<img src="https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/visualizebutton.png"/>
</a>

## Description
This template allows you to deploy an app service plan and a basic Windows web app with storage account, required to execute autoscaler app. Deploys to shared web app to minify spending - so, you'll need either to define bigger web app or find a way to ping it once in 5 minutes

## Example configuration for deployment

I did some preparations for publishing via Visual Studio or MsDeploy, but, for my deployments - I'll use just build in Release, which will output all required files to a folder ~\autoscalingApp\WebJob\ - then, I'll use simple MsDeploy command to deliver it on web app: 

```cmd
"$(msdeploy.Path)" -allowUntrusted="True" -enableRule:DoNotDeleteRule -verb:sync -source:contentPath="~\autoscalingApp\WebJob\" -dest:contentPath="$(deploy.iisSiteName)/App_Data/jobs/continuous/",computerName="https://$(deploy.iisSiteName).scm.azurewebsites.net:443/msdeploy.axd?site=$(deploy.iisSiteName)",username="$(user)",password="%userPwd%",authType="Basic"
```