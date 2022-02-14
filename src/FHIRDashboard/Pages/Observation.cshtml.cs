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
    public class ObservationModel : PageModel
    {
        private readonly ILogger<ObservationModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;

        public ObservationModel(IOptions<DashboardAppSettings> settings, ILogger<ObservationModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {
            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Observations";
            string _otext = "";
            List<Observation> _observations = new List<Observation>();

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
                    //.Where("code=8867-4,9279-1")
                    //.Where("code=bloodpressure")
                    //.Include("Patient:organization")
                    .SummaryOnly()
                    .LimitTo(100);

                //.OrderBy("birthdate", SortOrder.Descending);



                Bundle result = _fhirClient.Search<Observation>(q);

                int _count = 0;
                // Paging through a Bundle
                while (result != null)
                {
                    foreach (var e in result.Entry)
                    {
                        Observation o = (Observation)e.Resource;
                        // do something with the resource
                        //_otext += o.Id + " /// ";
                        //_count++;
                        _observations.Add(o);
                    }
                    result = _fhirClient.Continue(result, PageDirection.Next);
                }

                //_otext += "Total count: " + _count.ToString() + " /// ";
                ViewData["Observations"]= _observations;
            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

