## CLI/Bash
##variables
resourcegroupname=xxx
location=e.g. southcentralus
workspacename=xxx
tenantid=yourtenantid
subscriptionid=xxx


##login to azure
#az login
# get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\workspacetemplate.json' --parameters region=$location workspaceName=$workspacename