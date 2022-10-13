
@description('Default prefix for created resources')
param resourePrefix string = 'fhirSynthea'

@description('Location to deploy resources')
param location string = resourceGroup().location

@description('Name for the Azure Health Data Services Workspace. Leave blank to autogenerate.')
param azureHealthDataServicesWorkspaceName string = ''

@description('Name for the FHIR Service to create with 1000 synthea patietns.')
param fhirServiceName string = 'sythea1000'

@description('Name of the log analytics workspace to create. Leave blank to autogenerate.')
param logAnalyticsWorkspaceName string = ''

var uniqueResourceString = substring(uniqueString(guid(resourceGroup().id), location), 0, 5)
var defaultAzureHealthDataServicesWorkspaceName = '${resourePrefix}-${uniqueResourceString}-la'
var defaultLogAnalyticsWorkspaceName = '${resourePrefix}-${uniqueResourceString}-la'

module fhir './modules/fhir/main.bicep' = {
  name: 'azure-health-data-services-with-fhir'
  params: {
    workspaceName: length(azureHealthDataServicesWorkspaceName) > 0 ? azureHealthDataServicesWorkspaceName : defaultAzureHealthDataServicesWorkspaceName
    fhirServiceName: fhirServiceName
    logAnalyticsWorkspaceId: logAnalytics.outputs.loagAnalyticsId
    name: resourePrefix
    resourceLocation: location
    resourceTags: {
      sample: 'fhir-with-1000-synthea-patients'
    }
    seedMethod:'$import'
    seedWithData: true
  }
}


module logAnalytics './modules/logAnalytics.bicep' = {
  name: 'log-analytics-for-azure-health-data-services'
  params: {
    location: location
    workspaceName: length(logAnalyticsWorkspaceName) > 0 ? logAnalyticsWorkspaceName : defaultLogAnalyticsWorkspaceName
  }
}
