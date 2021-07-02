resourcegroupname=rg-himss-demo
fhirserviceurl=https://ws1-fhirdata.fhir.azurehealthcareapis.com
importername=importerbx2
location=southcentralus
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47
subscriptionid=cc148bf2-42fb-4913-a3fb-2f284a69eb89
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47
fhirclientid=b4b81ba7-7d13-4d10-ae2c-c097c5dd79c9
fhirclientsecret=1~uOXvbD-9cXsQCc~7N3~Djcd1l77uu5d7
importertemplate="src\\templates\\importer.json"

##login to azure
#az login
#get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

# deploy the importer as Azure Function
az deployment group create --resource-group $resourcegroupname --template-file $importertemplate --parameters appNameImporter=$importername fhirServiceUrl=$fhirserviceurl aadFHIRClientId=$fhirclientid aadFHIRClientSecret=$fhirclientsecret

# grant the importer Azure Function with access to the storage
importerstoragename=$importername'sa'
storagerolescope="https://management.azure.com/subscriptions/$subscriptionid/resourceGroups/$resourcegroupname/providers/Microsoft.Storage/storageAccounts/$importerstoragename/blobServices/default"
#find FHIR service managed identity AAD object id
fhiraadobjectid="e6c37ed1-76c2-4a0f-b865-9c341a82b026"
#az ad sp show --id b4b81ba7-7d13-4d10-ae2c-c097c5dd79c9 --query "[objectId]" -o table
az role assignment create --assignee-object-id $fhiraadobjectid --role "Storage Blob Data Contributor" --scope $storagerolescope

