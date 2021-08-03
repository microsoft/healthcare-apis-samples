$resourcegroupname="xxx"
$fhirserviceurl="yourfhirserviceurl"
$importername ="xxx"
$location="e.g. South Central US"
$tenantid="yourtenantid"
$subscriptionid="xxx"
$tenantid="yourtenantid"
$fhirclientid="yourclientid"
$fhirclientsecret="yourclientsecret"
$importertemplate="src/templates/importer.json"

##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId xxx
#Set-AzContext -Subscription xxx
#Connect-AzAccount -Tenant 'xxxx-xxxx-xxxx-xxxx' -SubscriptionId 'yyyy-yyyy-yyyy-yyyy'
#Get-AzContext -ListAvailable
#Get-AzContext 

## Create a resource group
New-AzResourceGroup -Name $resourcegroupname  -Location $location -Force 

# deploy the importer as Azure Function
New-AzResourceGroupDeployment -TemplateUri $importertemplate -appNameImporter $importername -ResourceGroupName $resourcegroupname -fhirServiceUrl $fhirserviceurl -aadFHIRClientId $fhirclientid -aadFHIRClientSecret $fhirclientsecret

# grant the importer Azure Function with access to the storage
$storagerolescope="/subscriptions/" + $subscriptionid+ "/resourceGroups/" + $resourcegroupname + "/providers/Microsoft.Storage/storageAccounts/" + $importername + "sa/blobServices/default"
#find FHIR service managed identity AAD object id
#$fhiraadobjectid="xxx"
$fhiraadobjectid=(Get-AzADServicePrincipal -ServicePrincipalName yourclientid).Id
New-AzRoleAssignment -ObjectId $fhiraadobjectid -RoleDefinitionName "Storage Blob Data Contributor" -Scope  $storagerolescope
