using System;
using System.Collections.Generic;
using FHIRDashboard.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FHIRDashboard.Pages
{
    public class ConditionModel : PageModel
    {
        private readonly ILogger<ConditionModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;

        public ConditionModel(IOptions<DashboardAppSettings> settings, ILogger<ConditionModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {
            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Conditions";
            
            List<Condition> _devices = new List<Condition>();

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


                var q = new SearchParams()
                    //.Where("name=Ewout")
                    //.Include("Patient:organization")
                    //.SummaryOnly()
                    .LimitTo(100);

                Bundle result = _fhirClient.Search<Condition>(q);

                // Paging through a Bundle
                while (result != null)
                {
                    foreach (var e in result.Entry)
                    {
                        Condition d = (Condition)e.Resource;
                        // do something with the resource
                        //_dtext += d.Id + " /// ";
                        _devices.Add(d);
                    }
                    result = _fhirClient.Continue(result, PageDirection.Next);
                }

                //ViewBag.Message = _dtext;

                ViewData["Conditions"] = _devices;

            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

