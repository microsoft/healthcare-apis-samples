$resourcegroupname="rg-himss-demo"
$fhirserviceurl="https://ws1-fhirdata.fhir.azurehealthcareapis.com"
$importername ="importerbx1"
$location="South Central US"
$tenantid="72f988bf-86f1-41af-91ab-2d7cd011db47"
$subscriptionid="cc148bf2-42fb-4913-a3fb-2f284a69eb89"
$tenantid="72f988bf-86f1-41af-91ab-2d7cd011db47"
$fhirclientid="b4b81ba7-7d13-4d10-ae2c-c097c5dd79c9"
$fhirclientsecret="1~uOXvbD-9cXsQCc~7N3~Djcd1l77uu5d7"
$importertemplate="src/templates/importer.json"

##login to azure
Connect-AzAccount 
#Connect-AzAccount SubscriptionId cc148bf2-42fb-4913-a3fb-2f284a69eb89
#Set-AzContext -Subscription cc148bf2-42fb-4913-a3fb-2f284a69eb89
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
$fhiraadobjectid="e6c37ed1-76c2-4a0f-b865-9c341a82b026"
#Get-AzADServicePrincipal -ServicePrincipalName b4b81ba7-7d13-4d10-ae2c-c097c5dd79c9
#Get-AzADApplication -ObjectId e28d5f65-d340-4c6a-9b9d-a7877d81832d | Get-AzADServicePrincipal
New-AzRoleAssignment -ObjectId $fhiraadobjectid -RoleDefinitionName "Storage Blob Data Contributor" -Scope  $storagerolescope
