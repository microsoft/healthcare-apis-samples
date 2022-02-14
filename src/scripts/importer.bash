deploymentname=xxx
resourcegroupname=xxx
workspacename=xxx
fhirservicename=xxx
importername=xxx
location=e.g. southcentralus
tenantid=yourtenantid
subscriptionid=xxx
tenantid=yourtenantid
fhirclientid=yourclientid
fhirclientsecret=yourclientsecret
importertemplate="src\\templates\\importer.json"

fhirserviceurl="https://$workspacename-$fhirservicename.fhir.azurehealthcareapis.com"
#importertemplate="src\\templates\\importer.json"
importertemplate=https://raw.githubusercontent.com/microsoft/healthcare-apis-samples/main/src/templates/importer.json


##login to azure
#az login
#get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

# deploy the importer as Azure Function
az deployment group create --name $deploymentname --resource-group $resourcegroupname --template-file $importertemplate --parameters appNameImporter=$importername fhirServiceUrl=$fhirserviceurl aadFHIRClientId=$fhirclientid aadFHIRClientSecret=$fhirclientsecret --rollback-on-error
#az functionapp delete --name $importername --resource-group $resourcegroupname

# grant the importer Azure Function with access to the storage
importerstoragename=$importername'sa'
storagerolescope="https://management.azure.com/subscriptions/$subscriptionid/resourceGroups/$resourcegroupname/providers/Microsoft.Storage/storageAccounts/$importerstoragename/blobServices/default"
#find FHIR service managed identity AAD object id
$fhiraadobjectid=(az ad sp show --id yourclientid --query objectId)
#fhiraadobjectid="xxx"
az role assignment create --assignee-object-id $fhiraadobjectid --role "Storage Blob Data Contributor" --scope $storagerolescope

