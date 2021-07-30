## PowerShell
##variables
$resourcegroupname="xxx"
$location="e.g. southcentralus"
$workspacename="xxx"
$fhirservicename="xxx"
$tenantid="yourtenantid"
$subscriptionid="xxx"
$storageaccountname="xxx"
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

