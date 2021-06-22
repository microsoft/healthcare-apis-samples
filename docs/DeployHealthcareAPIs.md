# How to Deploy Services with Healthcare APIs


You can deploy healthcare service instances using one of the following options:

- Azure Portal
- Healthcare APIs
- Azure ARM templates

To deploy a service instance from the Azure portal, please refer to the technical documentation.

To deploy a service instance using Healthcare APIs, open the sample Rest Client file in Visual Studio Code, modify the values in it and run the Get/Post/Put/Delete requests. For more info on how to use the Rest Client tool, review the [Rest Client doc](/docs/UseVSCodeExtensionRestClient.md).

To deploy a service instance using ARM templates for Healthcare APIs, you can run PowerShell and CLI commands along with the template. The sample code below shows how to use PowerShell to create a FHIR service instance using the [template file](/src/templates/fhiremplate.json).

```
#Define variables
$rgname="xxx"
$location="xxx e.g. South Central US"

Connect-AzAccount 

#List current context
Get-AzContext -ListAvailable
#To connect to a specified subscription
Connect-AzAccount SubscriptionId xxx
#To change the context to a specified subscription
Set-AzContext -Subscription xxx

#Create a resource group
New-AzResourceGroup -Name $rgname -Location $location

#Deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $rgname -TemplateFile fhirtemplate.json
```