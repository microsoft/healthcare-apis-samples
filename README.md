# Healthcare APIs Samples

This repo hosts samples for Healthcare APIs, including FHIR, DICOM, IoT Connector and data related services. The workspace is a top level logical container that resides within a resource group. It must be created first.

![image.png](/docs/images/workspace.png)

All sample scripts have been tested in the Rest Client in Visual Studio, unless otherwise noted.

## Deploy the Healthcare APIs

You can deploy each service through the Azure portal, or using scripts that you can find in the repo.

### Deploy workspace
- [PowerShell script](/src/scripts/workspacearm.ps1)
- [CLI script](/src/scripts/workspacearm.bash)
- [REST API call](/src/scripts/workspacerest.http)
- [workspace template](/src/templates/workspacetemplate.json)

### Deploy workspace and FHIR
- [PowerShell script](/src/scripts/fhirarm.ps1)
- [CLI script](/src/scripts/fhirarm.bash)
- [REST API call](/src/scripts/fhirrest.http)
- [fhir template](/src/templates/workspacetemplate.json)

Note: To deploy the FHIR service only, you can remove the workspace resource and the dependency on it in the template.

### Deploy workspace and DICOM
- [PowerShell script](/src/scripts/dicomarm.ps1)
- [CLI script](/src/scripts/dicomarm.bash)
- [REST API call](/src/scripts/dicomrest.http)
- [fhir template](/src/templates/dicomtemplate.json)

Note: To deploy the DICOM service only, you can remove the workspace resource and the dependency on it in the template.

### Deploy workspace, the FHIR service and Iot Connector
- [PowerShell script](/src/scripts/iotarm.ps1)
- [CLI script](/src/scripts/iotarm.bash)
- [REST API call](/src/scripts/iotrest.http)
- [fhir template](/src/templates/iottemplate.json)

Note: To deploy the IoT Connector only, you can remove the workspace resource, the fhir resource, and the dependency on them in the template.


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
