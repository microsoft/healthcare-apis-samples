## PowerShell
##variables
$resourcegroupname="xxx"
$location="e.g. South Central US"
$workspacename="xxx"
$fhirservicename="xxx"
$iotconnectorname="xxx"
$externalfqeventhubnamespace = "xxx.servicebus.windows.net"
$externaleventhubname="xxx"
$externalconsumergroup="xxx"
$tenantid="yourtenantid"

##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId xxx
#Set-AzContext -Subscription xxx
#Connect-AzAccount -Tenant 'xxxx-xxxx-xxxx-xxxx' -SubscriptionId 'yyyy-yyyy-yyyy-yyyy'
#Get-AzContext -ListAvailable
#Get-AzContext 


##create resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

##deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/iottemplate.json" -region $location -workspaceName $workspacename -fhirServiceName $fhirservicename -iotConnectorName $iotconnectorname  -externalFQEventHubNamespace $externalfqeventhubnamespace -externalEventHubName $externaleventhubname -externalConsumerGroup $externalconsumergroup -tenantid $tenantid
