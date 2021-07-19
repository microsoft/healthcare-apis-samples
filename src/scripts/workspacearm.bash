## CLI/Bash
##variables
resourcegroupname=rg-csstest$RANDOM
location=southcentralus
workspacename=ws22
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47
subscriptionid=cc148bf2-42fb-4913-a3fb-2f284a69eb89


##login to azure
#az login
# get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\workspacetemplate.json' --parameters region=$location workspaceName=$workspacename