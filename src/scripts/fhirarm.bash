## CLI/Bash
##variables
resourcegroupname=rg-fhir-rest$RANDOM
location=southcentralus
workspacename=a5ws
servicename=a5fhir
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47
subscriptionid=cc148bf2-42fb-4913-a3fb-2f284a69eb89
storageaccountname=a5stor
storageaccountconfirm=true


##login to azure
#az login
# get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\fhirtemplate.json' --parameters region=$location workspaceName=$workspacename serviceName=$servicename tenantid=$tenantid  storageAccountName=$storageaccountname storageAccountConfirm=$storageaccountconfirm