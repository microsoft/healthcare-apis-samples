## CLI/Bash
##variables
resourcegroupname=xxx
location=e.g. southcentralus
workspacename=xxx
fhirservicename=xxx
tenantid=yourtenantid
subscriptionid=xxx
storageaccountname=xxx
storageaccountconfirm=true

##login to azure
#az login
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\fhirtemplate.json' --parameters region=$location workspaceName=$workspacename fhirServiceName=$fhirservicename tenantid=$tenantid  storageAccountName=$storageaccountname storageAccountConfirm=$storageaccountconfirm