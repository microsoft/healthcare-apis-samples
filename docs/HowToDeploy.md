# How to Deploy Services with Healthcare APIs


You can deploy healthcare service instances using one of the following options:

- Azure Portal
- Healthcare APIs
- Azure ARM templates

To deploy a service instance from the Azure portal, please refer to the technical documentation.

To deploy a service instance using Healthcare APIs, you can use [ARM Rest API](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-rest). Open the sample Rest Client file in Visual Studio Code, modify the values in it and run the Get/Post/Put/Delete requests. For more info on how to use the Rest Client tool, review the [Rest Client doc](/docs/UseVSCodeExtensionRestClient.md).

To deploy a service instance using ARM templates for Healthcare APIs, you can run [PowerShell](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-powershell) and [CLI](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-cli) commands along with the template. The sample code below shows how to use PowerShell to create a FHIR service instance using the [template file](/src/templates/fhiremplate.json).

```
#Define variables
$resourcegroupname="xxx"
$location="xxx e.g. South Central US"

#login to Azure and verify or set subscription
Connect-AzAccount 

#List current context
Get-AzContext -ListAvailable
#To connect to a specified subscription
Connect-AzAccount SubscriptionId xxx
#To change the context to a specified subscription
Set-AzContext -Subscription xxx

#Create a resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

#Deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile fhirtemplate.json
```