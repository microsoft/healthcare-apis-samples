param name string
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

@description('Deploymenet script to load sample data with the Fhir Loader CLI')
resource seedFhirDataCli 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'seedFhirDataCli'
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
      containerGroupName: 'seedFhirDataCli-${name}-ci'
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
    scriptContent: loadTextContent('scripts/seed-fhir-data-cli.sh')
  }
}
