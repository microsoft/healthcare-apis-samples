# Azure FHIR Importer Function

The Azure function app will monitor a `fhirimport` container in the attached storage account and ingest patient bundles in json or ndjson format into the FHIR service. 

- For json formatted bundles generated from Synthea, a string conversion from "UUID" to a corresponding resource type is usually required. 
- For json formatted bundles that are ready to be imported, the conversion option can be disabled by setting the UUIDtoResourceTypeConversion flag to "false".
- For ndjson formatted bundles, no conversion is performed and the UUIDtoResourceTypeConversion flag is not used.

The function can be deployed with the [CLI scripts](../templates/importer.json) template, which will use an Azure Function App (App Service).

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



