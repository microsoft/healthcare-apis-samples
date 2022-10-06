param name string
param ahdsWorkspaceName string
param fhirName string
param tenantId string = subscription().tenantId
param resourceLocation string
param resourceTags object
param logAnalyticsWorkspaceId string

param seedWithData bool = false
@allowed([
  '$import'
  'cli'
])
param seedMethod string = '$import'

param attachedStorageAccountName string = '${ahdsWorkspaceName}stor'

@description('Deploys Azure Health Data Services and FHIR Service')
module fhir 'fhir.bicep'= {
  name: 'ahds-with-fhir-${ahdsWorkspaceName}'
  params: {
    workspaceName: ahdsWorkspaceName
    fhirServiceName: fhirName
    tenantId: tenantId
    location: resourceLocation
    tags: resourceTags
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId ?? ''
  }
}

var managedIdentityName = '${name}-fhirseed-identity'

@description('Creates the managed identity needed for deployment screpts')
module managed_identity './managedIdentity.bicep' = if (seedWithData) {
  name: 'ManagedIdentity'
  params: {
    managedIdentityName: managedIdentityName
    location: resourceLocation
  }
}

@description('Setup access between FHIR and the deployment script managed identity')
module deploymment_script_role_assignment_template '../..//shared/roleAssignment.bicep'= {
  name: 'fhirIdentity-deployment'
  params: {
    resourceId: fhir.outputs.fhirId
    roleId: '5a1fc7df-4bf1-4951-a576-89034ee01acd'
    principalId: managed_identity.outputs.identityPrincipalId
  }
}

@description('Deploys Azure Health Data Services and FHIR Service')
module synthea_data './seedDataCli.bicep'= if (seedWithData && seedMethod == 'cli') {
  name: 'seed-data-cli-${name}'
  params: {
    name: name
    fhirUrl: fhir.outputs.fhirServiceUrl
    fhirId: fhir.outputs.fhirId
    location: resourceLocation
    identity: managed_identity.outputs.identityId
    storageName: attachedStorageAccountName
  }
  dependsOn: [fhir, deploymment_script_role_assignment_template]
}

output fhirUrl string = fhir.outputs.fhirServiceUrl
output fhirId string = fhir.outputs.fhirId
