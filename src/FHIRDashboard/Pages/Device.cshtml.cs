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
    public class DeviceModel : PageModel
    {
        private readonly ILogger<DeviceModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;

        public DeviceModel(IOptions<DashboardAppSettings> settings, ILogger<DeviceModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {
            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Devices";
            
            List<Device> _devices = new List<Device>();

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

                //.OrderBy("birthdate", SortOrder.Descending)

                Bundle result = _fhirClient.Search<Device>(q);

                // Paging through a Bundle
                while (result != null)
                {
                    foreach (var e in result.Entry)
                    {
                        Device d = (Device)e.Resource;
                        // do something with the resource
                        //_dtext += d.Id + " /// ";
                        _devices.Add(d);
                    }
                    result = _fhirClient.Continue(result, PageDirection.Next);
                }

                //ViewBag.Message = _dtext;

                ViewData["Devices"] = _devices;

            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

