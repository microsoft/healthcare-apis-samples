# How to Use Rest Client Extension in Visual Studio Code

The [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension in Visual Studio Code is a similar tool to Postman that you can use to interact with Azure API for FHIR.

You can install the extension in Visual Studio Code easily. Select the Extensions icon on the left side and search "Rest Client". Click the Rest Client entry from the search results and press the Install button.

![image.png](images/restclient/install.png)

After installing the Rest Client extension, you are ready to make restful API calls. You can find more details on how to use the tool in the [installation file](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).

**Create an ".http" File and Define Variables**

Create a new file in Visual Studio Code. Enter a Get request command line in the file, and save it as "test.http". The file suffix ".http" automatically activates the Rest Client environment. Click on "Send Request" which is supplied by the tool to get the metadata. 

![image.png](images/restclient/sendrequest.png)

Before making restful API calls to the FHIR server (other than getting the metadata), you must complete the following prerequisite.

>[!Important] 
  Complete [application registration](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir/fhir-app-registration). Make a note of the Azure tenant id, client id, client secret and the service url.

You can now get an access token issued by Azure Active Directory for Azure API for FHIR. While you can use values such as client id directly in the restful API calls, it is a good practice that you define a few variables for these values and use the variables instead. 

Copy and paste the code to the file. The line with "###" is a comment.

```
### REST Client
@fhirurl =https://xxx.azurehealthcareapis.com
@clientid =5fa1....
@clientsecret =....
@tenantid =....
```

**Get AAD Access Token**

Copy and past the code to the file. The line starting with "#" contains a variable that captures the http response containing the access token. The variable, @token, is used to store the access token. 

>[!Note] 
>The grant_type of client_credentials is used to obtain an access token. To use authorization code flow, it is recommended that you use Postman. 

```
### Get access token 
# @name getAADToken 
POST https://login.microsoftonline.com/{{tenantid}}/oauth2/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&resource={{fhirurl}}
&client_id={{clientid}}
&client_secret={{clientsecret}}

### Extract access token from getAADToken request
@token = {{getAADToken.response.body.access_token}}

```

Click on "Send Request". You will see an http response that contains the access token if successful. 

![image.png](images/restclient/config.png)

**Get FHIR Data**

Copy and past the code to the file. The line with "Authorization" is the header info for the Get call.

```
### Get Patient 
Get {{fhirurl}}/Patient/7165139....
Authorization: Bearer {{token}}
```

![image.png](images/restclient/getpatient.png)

**Troubleshooting**

If you are unable to get metadata, which does not require access token based on the HL7 specification, check that the FHIR server is running.

If you are unable to get an access token, make sure that the client application is registered properly.

If you are unable to get data from the FHIR server, make sure that the client application (or the service principal) has been granted access permissions such as "FHIR Data Contributor".

