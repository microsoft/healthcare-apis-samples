// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.FHIR.Helper;
using System.Threading.Tasks;

namespace FHIRDashboard.Models
{ 
    public static class AADTokenProvision
    {
        static private string _tenantId;
        static private string _audience;
        static private string _clientId;
        static private string _clientSecret;
        static private string _authority;
        static private string _accesstoken;
        static private bool _msi;

        public static async Task<string> CheckAADToken(DashboardAppSettings settings)
        {
            // AAD settings and token
            _tenantId = settings.TenantId;
            _audience = settings.FHIRResource;
            _clientId = settings.ClientId;
            _clientSecret = settings.ClientSecret;
            _authority = settings.AzureAuthority + "/" + _tenantId;
            _msi = bool.Parse(settings.UseMsi);

            if ( string.IsNullOrEmpty(_accesstoken) || AADHelper.isTokenExpired(_accesstoken))
            {
                _accesstoken = await AADHelper.GetAADAccessToken(_authority, _clientId, _clientSecret, _audience, _msi);
                GlobalValues.FHIRResource = _audience;
                GlobalValues.AADToken = _accesstoken;
            }

            return _accesstoken;

        }
    }
}