## PowerShell
##variables
$resourcegroupname="rg-himss-demo"
$location="southcentralus"
$workspacename="ws1"
$fhirservicename="a1fhir"
$tenantid="72f988bf-86f1-41af-91ab-2d7cd011db47"
$subscriptionid="cc148bf2-42fb-4913-a3fb-2f284a69eb89"
$storageaccountname="a1stor"
$storageaccountconfirm=1

##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId $subscriptionid
Set-AzContext -Subscription $subscriptionid
Connect-AzAccount -Tenant $tenantid -SubscriptionId $subscriptionid
#Get-AzContext 


##create resource group
New-AzResourceGroup -Name $resourcegroupname -Location $location

##deploy the resource
New-AzResourceGroupDeployment -ResourceGroupName $resourcegroupname -TemplateFile "src/templates/fhirtemplate.json" -region $location -workspaceName $workspacename -fhirServiceName $fhirservicename -tenantid $tenantid -storageAccountName $storageaccountname -storageAccountConfirm $storageaccountconfirm

