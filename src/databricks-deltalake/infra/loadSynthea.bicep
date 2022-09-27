param name string
param fhirUrl string
param location string
param identity string

param utcValue string = utcNow()

@description('Deploymenet script to load sample Synthea data')
resource loadSyntheaData 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'loadSyntheaData'
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
      containerGroupName: '${name}-deploy'
    }
    timeout: 'PT2H'
    cleanupPreference: 'OnExpiration'
    retentionInterval: 'PT1H'
    environmentVariables: [
      {
        name: 'FHIR_URL'
        value: fhirUrl
      }
    ]
    scriptContent: loadTextContent('scripts/load-synthea-data.sh')
  }
}
