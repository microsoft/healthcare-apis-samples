# Deploy the Healthcare APIs

You can deploy each service through the Azure portal, or using scripts that you can find in the repo.

## Deploy workspace
- [PowerShell script](/src/scripts/workspacearm.ps1)
- [CLI script](/src/scripts/workspacearm.bash)
- [REST API call](/src/scripts/workspacerest.http)
- [workspace template](/src/templates/workspacetemplate.json)

## Deploy workspace and FHIR
- [PowerShell script](/src/scripts/fhirarm.ps1)
- [CLI script](/src/scripts/fhirarm.bash)
- [REST API call](/src/scripts/fhirrest.http)
- [fhir template](/src/templates/workspacetemplate.json)

Note: To deploy the FHIR service only, you can remove the workspace resource and the dependency on it in the template.

## Deploy workspace and DICOM
- [PowerShell script](/src/scripts/dicomarm.ps1)
- [CLI script](/src/scripts/dicomarm.bash)
- [REST API call](/src/scripts/dicomrest.http)
- [DICOM template](/src/templates/dicomtemplate.json)

Note: To deploy the DICOM service only, you can remove the workspace resource and the dependency on it in the template.

## Deploy workspace, the FHIR service and Iot Connector
- [PowerShell script](/src/scripts/iotarm.ps1)
- [CLI script](/src/scripts/iotarm.bash)
- [REST API call](/src/scripts/iotrest.http)
- [IoT Connector template](/src/templates/iottemplate.json)

Note: To deploy the IoT Connector only, you can remove the workspace resource, the fhir resource, and the dependency on them in the template.
```