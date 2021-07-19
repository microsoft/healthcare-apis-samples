## CLI/Bash
##variables
resourcegroupname=rg-himss-demo
location=southcentralus
workspacename=ws1
fhirservicename=fhir55
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47
subscriptionid=cc148bf2-42fb-4913-a3fb-2f284a69eb89
storageaccountname=stor55
storageaccountconfirm=true

##login to azure
#az login
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\fhirtemplate.json' --parameters region=$location workspaceName=$workspacename fhirServiceName=$fhirservicename tenantid=$tenantid  storageAccountName=$storageaccountname storageAccountConfirm=$storageaccountconfirm