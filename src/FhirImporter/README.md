# Azure FHIR Importer Function

The Azure function app will monitor a `fhirimport` container in the attached storage account and ingest patient bundles in json or ndjson format into the FHIR service. 

- For json formatted bundles generated using [Synthea Patient Generator](https://github.com/synthetichealth/synthea), a string conversion from "UUID" to a corresponding resource type is usually required. See [sample data here](../../data).
- For json formatted bundles that are ready to be imported, the conversion option can be disabled by setting the UUIDtoResourceTypeConversion flag to "false".
- For ndjson formatted bundles, no conversion is performed and the UUIDtoResourceTypeConversion flag is not used.See [sample data here](../../data).

The function can be deployed with the [CLI scripts](../scripts/importer.bash) and an ARM template (../templates/importer.json) template, which will use an Azure Function App (App Service). The deployment scripts allow you to
- Create a new resource group or use an exising resource group
- Create a storage account including two containers, one for uploading files and one for storing files that are invalid and cannot be processed.
- Create an Azure Function (App Service) and the service plan in which the Azure Function is hosted.
- Delete the azure function running the command, 'az functionapp delete --name $importername --resource-group $resourcegroupname' and re-deploy the Azure Function after upgrades. With the default incremental deployment mode, the deleted resources will be created. See more details on [Azure Resource Manager deployment modes](https://docs.microsoft.com/azure/azure-resource-manager/templates/deployment-modes). 

It can also be deployed as a container using Azure Container Instances. For this approach, first build the Docker image:

```
docker build -t reponame/fhirimporter .
docker push reponame/fhirimporter
```

When deploying, the following environment variables should be set:

```
APPINSIGHTS_INSTRUMENTATIONKEY=<KEY>
Audience=<e.g. https://azurehealthcareapis.com>
Authority=<e.g. https://login.microsoftonline.com/TENANT-ID>
AzureWebJobsDashboard=<STORAGE ACCOUNT CONNECTION STRING>
AzureWebJobsStorage=<STORAGE ACCOUNT CONNECTION STRING>
ClientId=<CLIENT ID (service client)>
ClientSecret=<CLIENT SECRET>
FhirServerUrl=<e.g. https://myaccount.azurehealthcareapis.com>
MaxDegreeOfParallelism=<default 16>	
c=<default true>
```



