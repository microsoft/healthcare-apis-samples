// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace Microsoft.Health.FHIR.Helper
{
    public static class AADHelper
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

        public static string GetResourceByToken(string accesstoken, string resourceurl)
        {
            try
            {
                using HttpClient _client = new HttpClient();
                HttpRequestMessage _request;
                HttpResponseMessage _response;
                _request = new HttpRequestMessage(HttpMethod.Get, resourceurl);
                _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                _response = _client.SendAsync(_request).Result;

                if (_response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return _response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string GetKVSecret(string akv, string secretname)
        {
            try
            {

                // use MSI or set up environment variables for standalone app
                // close and restart the VS solutin after setup or the variables are null
                var client = new SecretClient(new Uri(akv), new DefaultAzureCredential(true));

                KeyVaultSecret secret = client.GetSecret(secretname);


                return secret.Value;
            }
            catch (Exception exp)
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
    }
}