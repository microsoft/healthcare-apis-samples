//Define parameters
param workspaceName string
param fhirName string
param dicomName string
param iotName string
param tenantId string

//Define variables
var fhirservicename = '${workspaceName}/${fhirName}'
var dicomservicename = '${workspaceName}/${dicomName}'
var iotconnectorname = '${workspaceName}/${iotName}'
var iotdestinationname = '${iotconnectorname}/output1'
var loginURL = environment().authentication.loginEndpoint
var authority = '${loginURL}${tenantId}'
var audience = 'https://${workspaceName}-${fhirName}.fhir.azurehealthcareapis.com'

//output stringOutput1 string = authority
//output stringOutput2 string = audience

//Create a workspace
resource exampleWorkspace 'Microsoft.HealthcareApis/workspaces@2021-06-01-preview' = {
  name: workspaceName
  location: resourceGroup().location
}

//Use an existing workspace
// resource exampleExistingWorkspace 'Microsoft.HealthcareApis/workspaces@2021-06-01-preview' existing = {
//   name: workspaceName
// }

resource exampleFHIR 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-06-01-preview' = {
  name: fhirservicename
  location: resourceGroup().location
  kind: 'fhir-R4'
  identity: {
    type: 'SystemAssigned'
  }
  dependsOn: [
    exampleWorkspace  //exampleExistingWorkspace
  ]
  properties: {
    accessPolicies: []
    authenticationConfiguration: {
      authority: authority
      audience: audience
      smartProxyEnabled: false
    }
    }
}

//Use an existing FHIR service
// resource exampleExistingFHIR 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-06-01-preview' existing = {
//   name: fhirservicename
// }

//Create DICOM service
resource exampleDICOM 'Microsoft.HealthcareApis/workspaces/dicomservices@2021-06-01-preview' = {
  name: dicomservicename
  location: resourceGroup().location
  dependsOn: [
    exampleWorkspace
  ]
  properties: {}
}

//Use an existing DICOM service
// resource exampleExistingDICOM 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-06-01-preview' existing = {
//   name: dicomservicename
// }

//Create IoT connector
resource exampleIoT 'Microsoft.HealthcareApis/workspaces/iotconnectors@2021-06-01-preview' = {
  name: iotconnectorname
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  dependsOn: [
    exampleWorkspace
    //exampleExistingWorkspace
  ]
  properties: {
    ingestionEndpointConfiguration: {
      eventHubName: 'xxx'
      consumerGroup: 'xxx'
      fullyQualifiedEventHubNamespace: 'xxx.servicebus.windows.net'
            }
    deviceMapping: {
    content: {
    templateType: 'CollectionContent'
        template: [
                    {
                      templateType: 'JsonPathContent'
                      template: {
                              typeName: 'heartrate'
                              typeMatchExpression: '$..[?(@heartRate)]'
                              deviceIdExpression: '$.deviceId'
                              timestampExpression: '$.endDate'
                              values: [
                                {
                                      required: 'true'
                                      valueExpression: '$.heartRate'
                                      valueName: 'hr'
                                      }
                                      ]
                                }
                    }
                  ]
            }
          }
      }
    }


//Use an existing IoT 
// resource exampleExistingIoT 'Microsoft.HealthcareApis/workspaces/iotconnectors/fhirdestinations@2021-06-01-preview' existing = {
//   name: iotconnectorname
// }

//Create IoT destination
resource exampleIoTDestination 'Microsoft.HealthcareApis/workspaces/iotconnectors/fhirdestinations@2021-06-01-preview'  = {
  name:   iotdestinationname
  location: resourceGroup().location
  dependsOn: [
    exampleIoT
    //exampleExistingIoT
  ]
  properties: {
    resourceIdentityResolutionType: 'Create'
    fhirServiceResourceId: exampleFHIR.id //exampleExistingFHIR.id
    fhirMapping: {
                content: {
                    templateType: 'CollectionFhirTemplate'
                    template: [
                        {
                            templateType: 'CodeValueFhir'
                            template: {
                                codes: [
                                    {
                                        code: '8867-4'
                                        system: 'http://loinc.org'
                                        display: 'Heart rate'
                                    }
                                ]
                                periodInterval: 60
                                typeName: 'heartrate'
                                value: {
                                    defaultPeriod: 5000
                                    unit: 'count/min'
                                    valueName: 'hr'
                                    valueType: 'SampledData'
                                }
                            }
                        }
                    ]
                }
            }
        }
}



