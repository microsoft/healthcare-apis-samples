## CLI/Bash
##variables
resourcegroupname=rg-iot-cli$RANDOM
location=southcentralus
workspacename=ws$RANDOM
fhirservicename=fhir$RANDOM
iotconnectorname=iot$RANDOM
externalfqeventhubnamespace=bxeventhubns.servicebus.windows.net
externaleventhubname=bxeventhub1
externalconsumergroup=bxeventhubcg1
tenantid=72f988bf-86f1-41af-91ab-2d7cd011db47

##login to azure
#az login
# get the current default subscription using show
#az account show --output table
#az account set --subscription $subscriptionid

##create resource group
az group create --name $resourcegroupname --location $location

##deploy the resource
az deployment group create --resource-group $resourcegroupname --template-file 'src\\templates\\iottemplate.json' --parameters region=$location workspaceName=$workspacename fhirServiceName=$fhirservicename iotConnectorName=$iotconnectorname  externalFQEventHubNamespace=$externalfqeventhubnamespace externalEventHubName=$externaleventhubname externalConsumerGroup=$externalconsumergroup tenantid=$tenantid