#/bin/bash -e

STORAGE_ACCOUNT_NAME=syntheahedistest2
STORAGE_CONTAINER_NAME=import
STORAGE_PREFIX=

SUBSCRIPTION_ID=17af5f40-c564-4afe-ada0-fe7193bd474a
RESOURCE_GROUP_NAME=applied-team-shared
WORKSPACE_NAME=applied
FHIR_SERVICE_NAME=synthea-hedis-test2

# Enables import on the FHIR Service
function enable_import() {
    FHIR_MANAGEMENT_URL="https://management.azure.com/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.HealthcareApis/workspaces/${WORKSPACE_NAME}/fhirservices/${FHIR_SERVICE_NAME}?api-version=2022-01-31-preview"
    CURRENT_FHIR_SERVICE=`az rest --method get --uri "$FHIR_MANAGEMENT_URL"`

    FHIR_SERVICE_WITH_IMPORT=`echo $CURRENT_FHIR_SERVICE | jq \
        --arg storageAccountName "$STORAGE_ACCOUNT_NAME" \
        '.properties.importConfiguration.enabled |= true
        | .properties.importConfiguration.initialImportMode |= true
        | .properties.importConfiguration.integrationDataStore |= $storageAccountName
        | del(.etag)'`

    az rest --method put \
        --uri "$FHIR_MANAGEMENT_URL" \
        --body "$FHIR_SERVICE_WITH_IMPORT"
}

function disable_import() {
    FHIR_MANAGEMENT_URL="https://management.azure.com/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.HealthcareApis/workspaces/${WORKSPACE_NAME}/fhirservices/${FHIR_SERVICE_NAME}?api-version=2022-01-31-preview"
    CURRENT_FHIR_SERVICE=`az rest --method get --uri "$FHIR_MANAGEMENT_URL"`

    FHIR_SERVICE_WITH_IMPORT=`echo $CURRENT_FHIR_SERVICE | jq \
        --arg storageAccountName "$STORAGE_ACCOUNT_NAME" \
        '.properties.importConfiguration.enabled |= false
        | .properties.importConfiguration.initialImportMode |= false
        | .properties.importConfiguration.integrationDataStore |= $storageAccountName
        | del(.etag)'`

    az rest --method put \
        --uri "$FHIR_MANAGEMENT_URL" \
        --body "$FHIR_SERVICE_WITH_IMPORT"
}

# Checks the current status of import on the FHIR Service
function check_status() {
    FHIR_MANAGEMENT_URL="https://management.azure.com/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.HealthcareApis/workspaces/${WORKSPACE_NAME}/fhirservices/${FHIR_SERVICE_NAME}?api-version=2022-01-31-preview"
    CURRENT_FHIR_SERVICE=`az rest --method get --uri "$FHIR_MANAGEMENT_URL"`
    CURRENT_STATUS=`echo "$CURRENT_FHIR_SERVICE" | jq -r '.properties.provisioningState'`

    echo $CURRENT_STATUS
}

IMPORT_BLOBS=
NUM_IMPORT_BLOBS=
function get_import_files()
{
    echo "Searching storage account ${STORAGE_ACCOUNT_NAME}, container ${STORAGE_CONTAINER_NAME}, with prefix ${STORAGE_PREFIX} for ndjson files to import..."
    
    IMPORT_BLOBS=`az storage blob list --account-name "$STORAGE_ACCOUNT_NAME" --container-name "$STORAGE_CONTAINER_NAME" --num-results "*" --query "[?ends_with(name, '.ndjson')].name" --only-show-errors --output tsv`

    NUM_IMPORT_BLOBS=`echo "$IMPORT_BLOBS" | wc -l`
}

function send_import_request() {

FILE_NAME="import-${2}.json"
cat <<EOT > $FILE_NAME
{
    "resourceType": "Parameters",
    "parameter": [
        {
            "name": "inputFormat",
            "valueString": "application/fhir+ndjson"
        },
        {
            "name": "mode",
            "valueString": "InitialLoad"
        }
EOT

read -r -d '' IMPORT_PARAMETER_TEMPLATE << EOM
{
    "name": "input",
    "part": [
        {
            "name": "type",
            "valueString": ""
        },
        {
            "name": "url",
            "valueUri": ""
        }
    ]
}
EOM

    while IFS= read -r FILE_PATH
    do 
        if [[ $name == *by ]]; then
            continue
        fi
        RESOURCE_TYPE=`echo $FILE_PATH | cut -d. -f1`
        FILE_URL=https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/${STORAGE_CONTAINER_NAME}/${FILE_PATH}

        #THIS_IMPORT_PARAMETER=`echo ${IMPORT_PARAMETER_TEMPLATE} | jq ''`
        FILE_IMPORT_PARAM=`echo $IMPORT_PARAMETER_TEMPLATE | jq \
            --arg resourceType "$RESOURCE_TYPE" \
            --arg fileUrl "$FILE_URL" \
            '.part[0].valueString |= $resourceType
            | .part[1].valueUri |= $fileUrl'`

        echo ",$FILE_IMPORT_PARAM" >> $FILE_NAME
    done <<< "$IMPORT_BLOBS"

    echo "]}" >> $FILE_NAME

    cat $FILE_NAME

    TOKEN=`az account get-access-token --resource "https://${WORKSPACE_NAME}-${FHIR_SERVICE_NAME}.fhir.azurehealthcareapis.com" | jq .accessToken --raw-output`

    curl -v -d "@${FILE_NAME}" "https://${WORKSPACE_NAME}-${FHIR_SERVICE_NAME}.fhir.azurehealthcareapis.com/\$import" \
        -H "Prefer: respond-async" \
        -H "Content-Type: application/fhir+json" \
        -H "Authorization: Bearer ${TOKEN}"
}

echo "Enabling import..."
#enable_import

echo "Fetching import blob information..."
get_import_files

echo "Found $NUM_IMPORT_BLOBS blobs to import. Building import request..."


for START in `seq 1 50 $NUM_IMPORT_BLOBS`
do
    END=$(($START+50-1))
    echo "Processing import for chunks $START to $END..."
    THIS_CHUNK=`echo "$IMPORT_BLOBS" | sed -n "${START},${END}p"`

    send_import_request "$THIS_CHUNK" "$START"
done
 