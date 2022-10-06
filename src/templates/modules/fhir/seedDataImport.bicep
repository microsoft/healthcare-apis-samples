param name string
param workspaceName string
param fhirName string
param fhirUrl string
param fhirId string
param location string
param identity string
param storageName string
param seedDataPath string = 'https://ahdssampledata.blob.core.windows.net/fhir/synthea-ndjson-100/'

param utcValue string = utcNow()

@description('Used to pull keys from existing deployment storage account')
resource deployStorageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' existing = {
  name: storageName
}

@description('Setup access between FHIR and the function app via role assignment')
module fhir_storage_role_assignment '../../shared/roleAssignment.bicep'= {
  name: 'fhir-storage-managed-identity-access'
  params: {
    resourceId: deployStorageAccount.id
    // Storage Blob Data Contributor
    roleId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    principalId: fhirId
  }
}


module enable_import './toggle_import.bicep' = {
  name: 'enable-import-${fhirUrl}'
  params: {
    workspaceName: workspaceName
    fhirName: fhirName
    resourceLocation: location
    storageName: storageName
    enableImport: true
  }
}

@description('Deploymenet script to seed sample data with $import')
resource seedFhirDataImport 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'seedFhirDataImport'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity}': {}
    }
  }
  properties: {
    azCliVersion: '2.26.0'
    forceUpdateTag: utcValue
    containerSettings: {
      containerGroupName: 'seedFhirDataImport-${name}-ci'
    }
    storageAccountSettings: {
      storageAccountName: deployStorageAccount.name
      storageAccountKey: listKeys(deployStorageAccount.id, '2019-06-01').keys[0].value
    }
    timeout: 'PT2H'
    cleanupPreference: 'OnExpiration'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'FHIR_URL'
        value: fhirUrl
      }
      {
        name: 'SEED_DATA_PATH'
        value: seedDataPath
      }
    ]
    scriptContent: loadTextContent('scripts/seed-fhir-data-import.sh')
  }

  dependsOn: [enable_import, fhir_storage_role_assignment]
}

module disable_import './toggle_import.bicep' = {
  name: 'enable-import-${fhirUrl}'
  params: {
    workspaceName: workspaceName
    fhirName: fhirName
    resourceLocation: location
    storageName: storageName
    enableImport: false
  }

  dependsOn:[seedFhirDataImport]
}
