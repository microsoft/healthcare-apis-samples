using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FHIRDashboard.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.FHIR.Helper;

namespace FHIRDashboard.Pages
{
    public class DeviceDetailModel : PageModel
    {
        private readonly ILogger<DeviceDetailModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;

        public DeviceDetailModel(IOptions<DashboardAppSettings> settings, ILogger<DeviceDetailModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {
            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Device Detail";

            var id = Request.Query["id"];

            try
            {
                //test FHIR R4 client begin
                var _fhirClient = new FhirClient(_audience);
                _fhirClient.PreferredFormat = Hl7.Fhir.Rest.ResourceFormat.Json;
                _fhirClient.UseFormatParam = true;

                _fhirClient.OnBeforeRequest += (object sender, BeforeRequestEventArgs e) =>
                {
                    // Replace with a valid bearer token for the server
                    e.RawRequest.Headers.Add("Authorization", "Bearer " + _accesstoken);
                    //e.RawRequest.Headers.Add("Accept", "application/json");
                };

                //_fhirClient.OnAfterResponse += (object sender, AfterResponseEventArgs e) =>
                //{
                //    Console.WriteLine("Received response with status: " + e.RawResponse.StatusCode);
                //};

                var q = new SearchParams();
                q.Add("id", id);

                //Bundle result = _fhirClient.Search<Patient>(q);

                var result = _fhirClient.Get("Device/" + id);

                // Paging through a Bundle
                if (result != null)
                    //ViewData["PatientDetail"] = result.Entry.FirstOrDefault().Resource;
                    ViewData["DeviceDetail"] = result;
                else
                    ViewData["DeviceDetail"] = null;

            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

