using System;
using FHIRDashboard.Models;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHIRDashboard.Pages
{
    public class PatientDetailModel : PageModel
    {
        private readonly ILogger<PatientDetailModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;

        public PatientDetailModel(IOptions<DashboardAppSettings> settings, ILogger<PatientDetailModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {

            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Patient Detail";

            var id = Request.Query["id"];
            GlobalValues.CurrentPatient = id;

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


                var q = new SearchParams();
                q.Add("id", id);

                //Bundle result = _fhirClient.Search<Patient>(q);

                var result = _fhirClient.Get("Patient/" + id);

                // Paging through a Bundle
                if (result != null)
                    //ViewData["PatientDetail"] = result.Entry.FirstOrDefault().Resource;
                    ViewData["PatientDetail"] = result;
                else
                    ViewData["PatientDetail"] = null;

            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

