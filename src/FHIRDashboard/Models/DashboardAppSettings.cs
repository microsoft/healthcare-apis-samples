// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace FHIRDashboard.Models
{
    public class  DashboardAppSettings
    {
        public  string AzureAuthority { get; set; }
        public  string FHIRResource { get; set; }
        public  string TenantId { get; set; }
        public  string ClientId { get; set; }
        public  string ClientSecret { get; set; }
        public  string UseMsi { get; set; }

    }


}
