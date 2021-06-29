## PowerShell
##variables
$resourcegroupname="rg-jupiter-ps"
$location="South Central US"
$workspacename="a1ws"
$servicename="a1fhir"
$tenantid="72f988bf-86f1-41af-91ab-2d7cd011db47"
$subscriptionid="cc148bf2-42fb-4913-a3fb-2f284a69eb89"
$storageaccountname="a5stor"
$storageaccountconfirm="true"

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
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/fhirtemplate.json" -region $location -workspaceName $workspacename -serviceName $servicename -tenantid $tenantid  -storageAccountName $storageaccountname -storageAccountConfirm $storageaccountconfirm

