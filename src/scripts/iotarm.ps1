## PowerShell
##variables
$resourcegroupname="rg-iot-ps"
$location="South Central US"
$workspacename="a33ws"
$fhirservicename="a33fhir"
$iotconnectorname="a33iot"
$externalfqeventhubnamespace = "bxeventhubns.servicebus.windows.net"
$externaleventhubname="bxeventhub1"
$externalconsumergroup="bxeventhubcg1"


##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Set-AzContext -Subscription cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Connect-AzAccount -Tenant 'xxxx-xxxx-xxxx-xxxx' -SubscriptionId 'yyyy-yyyy-yyyy-yyyy'
#Get-AzContext -ListAvailable
#Get-AzContext 


##create resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

##deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/iottemplate.json" -region $location -workspaceName $workspacename -fhirServiceName $fhirservicename -iotConnectorName $iotconnectorname  -externalFQEventHubNamespace $externalfqeventhubnamespace -externalEventHubName $externaleventhubname -externalConsumerGroup $externalconsumergroup -tenantid $tenantid
