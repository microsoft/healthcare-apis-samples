## CLI/Bash
##variables
resourcegroupname=xxx
location=e.g. southcentralus
workspacename=xxx
fhirservicename=xxx
iotconnectorname=xxx
externalfqeventhubnamespace=xxx.servicebus.windows.net
externaleventhubname=xxx
externalconsumergroup=xxx
tenantid=yourtenantid

##login to azure
#az login
# get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\iottemplate.json' --parameters region=$location workspaceName=$workspacename fhirServiceName=$fhirservicename iotConnectorName=$iotconnectorname  externalFQEventHubNamespace=$externalfqeventhubnamespace externalEventHubName=$externaleventhubname externalConsumerGroup=$externalconsumergroup tenantid=$tenantid