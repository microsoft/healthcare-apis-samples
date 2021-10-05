# How To Run the Test Data Generator (FHIRTDG)

The FHIRTDG tool automates the testing process to use Synthea data directly. It reads ndjson files stored in Azure Storage, downloads resources individually and create csv files for the CRUD operations. 

Download the tool and replace the values in the appsettings.json file. 

`
{
  //Prerequisite: ndjson files stored in Azure Storage

  "azuretorageconnection": "xxx",
  "ndjsonstoragecontainer": "e.g. synthea",
  "filefilter": "e.g. fhir",
  "filecountstart": 1,
  "filecountend": 999999,

  //Performance test
  "fhirresourcetotal": 5,
  "jsonfilelocation": "c:\\jmeter\\jsonfiles",
  "fhircrud1": "Patient,10,10,10,0",
  "fhircrud2": "Observation,6,4,8,0",
  "fhircrud3": "Encounter,5,5,7,0",
  "fhircrud4": "Claim,4,8,12,0",
  "fhircrud5": "MedicationRequest,12,5,5,0"
}
`
For example, "fhircrud1" specifies the values for "create", "read", "update","delete' operations for the Patient resource. "fhirresourcetotal" specifies how many FHIR resources you want to work with.

While it is possible specify different CRUD operation values for a given resource, it is recommended that you keep them the same for each resource to avoid possible http errors in the JMeter tool.

Note: You can run the FHIRTDG tool to create test data. Optionally, you can integrate the dotnet tool into your JMeter test script. 