// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace HealthcareAPIsSamples
{
    public static class FHIRDLHelper
    {
        public static async Task<string> GetAADAccessToken(string authority, string clientId, string clientSecret, string audience, bool msi)
        {
            try
            {
                if (msi)
                {
                    AzureServiceTokenProvider _azureServiceTokenProvider;
                    _azureServiceTokenProvider = new AzureServiceTokenProvider();
                    //var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    return await _azureServiceTokenProvider.GetAccessTokenAsync(audience).ConfigureAwait(false);

                }
                else
                {
                    AuthenticationContext _authContext;
                    ClientCredential _clientCredential;
                    AuthenticationResult _authResult;

                    _authContext = new AuthenticationContext(authority);
                    _clientCredential = new ClientCredential(clientId, clientSecret);
                    _authResult = _authContext.AcquireTokenAsync(audience, _clientCredential).Result;
                    return _authResult.AccessToken;
                }

            }
            catch (Exception e)
            {
                return null;
            }

        }

        public static bool isTokenExpired(string bearerToken)
        {
            if (bearerToken == null) return true;
            // Remove end of file escape character (\0)
            var jwt = bearerToken.Substring(0, bearerToken.Length - 2);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(bearerToken);
            var tokenExpiryDate = token.ValidTo;

            // If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
            if (tokenExpiryDate == DateTime.MinValue) return true;

            // If the token is in the past then you can't use it
            if (tokenExpiryDate < DateTime.UtcNow) return true;
            return false;
        }

        public static string ConvertJsonFiles(string content)
        {
            string _s = null;
            JObject _obj = null;
            string _id = null;
            string _rt = null;
            string _fullUrl = null;
            string _jsonString = "";

            try
            {
                JObject _objContent = JObject.Parse(content);
                if (_objContent == null || _objContent["entry"] == null || ((string)_objContent["resourceType"] != "Bundle") || ((string)_objContent["type"] != "transaction"))
                {
                    throw new Exception("json file is empty, or missing bundle, entry block, transaction");
                }

                JArray _entries = (JArray)_objContent["entry"];

                //Use Dictionary to store resource id and resource type 
                Dictionary<string, ResourceRefPair> _dict = new Dictionary<string, ResourceRefPair>();

                foreach (JToken _tok in _entries)
                {
                    _obj = (JObject)_tok["resource"];
                    _s = _obj.ToString(Formatting.None);
                    _fullUrl = (string)_tok["fullUrl"];
                    _id = (string)_obj["id"];
                    _rt = (string)_obj["resourceType"];


                        //save id and resource type to dictionary for FHIR "reference" lookup
                        _dict.TryAdd(_fullUrl, new ResourceRefPair
                        {
                            Id = _id,
                            ResourceType = _rt
                        });

                        //convert urn:uuid:id => resourcetype:id
                        //skip if JToken object doesn't contain reference 
                        if (_s.IndexOf("reference") >= 0)
                        {
                            ReplaceUUIDWithResourceType(_tok, _dict);
                            _s = (_tok["resource"]).ToString(Formatting.None);
                        }

                        //add each json file to one line for ndjson file
                        _jsonString += _s + "\n";

                }

                return _jsonString;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e.Message}");
                return null;
            }
        }

        public static async Task<string> LoadFHIRResource(string fhirresource, string accesstoken, string content)
        {
            string _requestUrl = null;
            string _id = null;
            string _rt = null;

            try
            {
                JObject _objContent = JObject.Parse(content);
                if (_objContent == null || _objContent["resourceType"] == null)
                {
                    throw new Exception("json file is empty, or invalid");
                }

                HttpClient _client = new HttpClient();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);

                HttpContent _hc = new StringContent(content, Encoding.UTF8, "application/json");

                _rt = (string)_objContent["resourceType"];
                _id = (string)_objContent["id"];
                _requestUrl = fhirresource + "/" + _rt + "/" + _id;

                HttpResponseMessage _response = await _client.PutAsync(_requestUrl, _hc);

                switch (_response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        break;
                    //case HttpStatusCode.Unauthorized:
                    default:
                        Console.WriteLine($"http status code {_response.StatusCode} resource type {_rt} resource id {_id}");
                        return null;
                }
                return _requestUrl;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: {e.Message}");
                return null;
            }
        }

        public static void ReplaceUUIDWithResourceType(JToken tok, Dictionary<string, ResourceRefPair> dict)
        {

            switch (tok.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:

                    foreach (JToken _c in tok.Children())
                    {
                        ReplaceUUIDWithResourceType(_c, dict);
                    }

                    break;
                case JTokenType.Property:
                    JProperty prop = (JProperty)tok;

                    if (prop.Value.Type == JTokenType.String &&
                        prop.Name == "reference" &&
                        dict.TryGetValue(prop.Value.ToString(), out ResourceRefPair v))
                    {
                        prop.Value = v.ResourceType + "/" + v.Id;
                    }
                    else
                    {
                        ReplaceUUIDWithResourceType(prop.Value, dict);
                    }
                    break;
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Float:
                case JTokenType.Integer:
                case JTokenType.Date:
                    break;
                default:
                    //throw new NotSupportedException($"Invalid token type {tok.Type} encountered");
                    break;
            }

        }

        public class ResourceRefPair
        {
            public string Id { get; set; }
            public string ResourceType { get; set; }
        }
    }
}