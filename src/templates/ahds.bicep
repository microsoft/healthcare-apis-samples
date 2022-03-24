//Define parameters
param workspaceName string
param fhirName string
param dicomName string
param medtechName string
param tenantId string
param location string

//Define variables
var fhirservicename = '${workspaceName}/${fhirName}'
var dicomservicename = '${workspaceName}/${dicomName}'
var medtechservicename = '${workspaceName}/${medtechName}'
var medtechdestinationname = '${medtechservicename}/output1'
var loginURL = environment().authentication.loginEndpoint
var authority = '${loginURL}${tenantId}'
var audience = 'https://${workspaceName}-${fhirName}.fhir.azurehealthcareapis.com'

//output stringOutput1 string = authority
//output stringOutput2 string = audience

//Create a workspace
resource exampleWorkspace 'Microsoft.HealthcareApis/workspaces@2021-11-01' = {
  name: workspaceName
  location: location
}

//Use an existing workspace
// resource exampleExistingWorkspace 'Microsoft.HealthcareApis/workspaces@2021-11-01' existing = {
//   name: workspaceName
// }

resource exampleFHIR 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-11-01' = {
  name: fhirservicename
  location: location
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
// resource exampleExistingFHIR 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-11-01' existing = {
//   name: fhirservicename
// }

//Create DICOM service
resource exampleDICOM 'Microsoft.HealthcareApis/workspaces/dicomservices@2021-11-01' = {
  name: dicomservicename
  location: location
  dependsOn: [
    exampleWorkspace
  ]
  properties: {}
}

//Use an existing DICOM service
// resource exampleExistingDICOM 'Microsoft.HealthcareApis/workspaces/fhirservices@2021-11-01' existing = {
//   name: dicomservicename
// }

//Create IoT connector
resource exampleIoT 'Microsoft.HealthcareApis/workspaces/iotconnectors@2021-11-01' = {
  name: medtechservicename
  location: location
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
                              typeMatchExpression: '$..[?(@heartrate)]'
                              deviceIdExpression: '$.deviceid'
                              timestampExpression: '$.measurementdatetime'
                              values: [
                                {
                                      required: 'true'
                                      valueExpression: '$.heartrate'
                                      valueName: 'Heart rate'
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
// resource exampleExistingIoT 'Microsoft.HealthcareApis/workspaces/iotconnectors/fhirdestinations@2021-11-01' existing = {
//   name: iotconnectorname
// }

//Create IoT destination
resource exampleIoTDestination 'Microsoft.HealthcareApis/workspaces/iotconnectors/fhirdestinations@2021-11-01'  = {
  name:   medtechdestinationname
  location: location
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



