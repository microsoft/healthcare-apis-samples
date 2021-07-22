# Deploy the Healthcare APIs

You can deploy each service through the Azure portal, or using scripts that you can find in the repo.

>[!Important]
>Make sure that you have installed the latest versions of PowerShell, Azure Az PowerShell Module, and Azure CLI. You can check the versions: 
>- PowerShell `$PSVersionTable.PSVersion`
>- Azure PowerShell Module `Get-InstalledModule -Name Az -AllVersions`
>- az `cli --version`

- [Install latest version of PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell)
- [Install lastest version of Azure PowerShell](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps)
- [Install latest version of Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

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

>[!Warning]
>Make sure that you have been granted adequate permissions to create resources in a subscription. Otherwise you may receive errors as one shown.
>
>*The client 'xxx' with object id 'xxx' does not have authorization to perform action 'Microsoft.Resources/subscriptions/resourcegroups/write' over scope '/subscriptions/xxx/resourcegroups/xxx' or the scope is invalid. If access was recently granted, please refresh your credentials.*
