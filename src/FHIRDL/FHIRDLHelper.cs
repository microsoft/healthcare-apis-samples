// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

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
    }
}