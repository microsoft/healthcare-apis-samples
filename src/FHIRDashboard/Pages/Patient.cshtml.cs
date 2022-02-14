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
    public class PatientModel : PageModel
    {
        private readonly ILogger<PatientModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;
        private string _audience;
        public PatientModel(IOptions<DashboardAppSettings> settings, ILogger<PatientModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        public void OnGet()
        {
            var t = AADTokenProvision.CheckAADToken(_config);
            _audience = GlobalValues.FHIRResource;
            _accesstoken = GlobalValues.AADToken;

            ViewData["Title"] = "FHIR Patients";
            string _ptext = "";
            List<Patient> _patients = new List<Patient>();

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

                //var q = new SearchParams()
                //    //.Where("name=Ewout")
                //    .Include("Patient:organization")
                //    .LimitTo(10)
                //    .SummaryOnly()
                //    .OrderBy("birthdate", SortOrder.Descending);

                var q = new SearchParams()
                    //.Where("name=Ewout")
                    //.Include("Patient:organization")
                    .LimitTo(100)
                    .SummaryOnly()
                    .OrderBy("birthdate", SortOrder.Descending);

                //var q = new Parameters()
                //    .Where("name: exact = ewout")
                //    .OrderBy("birthDate", SortOrder.Descending)
                //    .SummaryOnly().Include(“Patient.managingOrganization”)
                //    .LimitTo(20);
                //Bundle result = _fhirClient.Search(q);

                //q.Add("gender", "male");


                //SearchParams searchParams = new SearchParams();
                //searchParams.Add("name", "xxx");
                //searchParams.Add("_count", "50");
                //Hl7.Fhir.Model.Bundle patients = _fhirClient.Search<Patient>(searchParams);
                //Log.Info(TAG, "Retrieved patients: " + patients.Total);

                //RunOnUiThread(() =>
                //{
                //    ListView pxListView = FindViewById<ListView>(Resource.Id.PatientListView);
                //    ArrayAdapter adapter = pxListView.Adapter as ArrayAdapter;
                //    Log.Debug(TAG, "Adapter: " + adapter.ToString());
                //    adapter.Clear();
                //    adapter.AddAll(patients.Entry);
                //}


                Bundle result = _fhirClient.Search<Patient>(q);

                // Paging through a Bundle
                while (result != null)
                {
                    foreach (var e in result.Entry)
                    {
                        Patient p = (Patient)e.Resource;
                        // do something with the resource
                        //_ptext += p.Id + ":      " + p.Name[0].Family + ", " + p.Name[0].GivenElement[0] +" /// ";
                        _patients.Add(p);
                    }
                    result = _fhirClient.Continue(result, PageDirection.Next);
                }

                ViewData["Patients"] = _patients;


            }

            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception e)
            {

            }
    }

    } 
}

