// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Linq;
using System.Threading;

namespace HealthcareAPIsSamples.FHIRDL
{
    class Program
    {
        //Get FHIR data source settings
        static string _bundleStorageConnection;
        static string _bundleContainer;
        static string _bundleContainerconvert;  // urn:uuid converted to resource type

        //Get FHIR settings
        static bool _fhirserversecurity;
        static string _fhirStorageConnection;
        static string _fhirLogsContainer;
        static string _fhirExportContainer;
        static string _fhirFileFilter;
        static int _fhirFileCountStart;
        static int _fhirFileCountEnd;
        static int _fhirMaxRetry;

        static string _fhiraudience;
        static string _fhirtenantId;
        static string _fhirclientId;
        static string _fhirclientSecret;
        static string _fhirlogin;
        static string _authority;

        // The Event Hubs client types are safe to cache and use as a singleton for the lifetime
        // of the application, which is best practice when events are being published or read regularly.
        static EventHubProducerClient producerClient;
        static string _streventhubconnection;
        static string _streventhubname;
        static string _deviceid;
        static string _patientid;

        static string _accesstoken = null;
        static string _requestUrl = null;
        static HttpClient _client = new HttpClient();
        static HttpRequestMessage _request;
        static HttpResponseMessage _response;

        static string _exportResponseLocation;
        static string _logStorage = null;
        static string _msg = null;
        static string _content = null;
        static string _fhirLoaderResponse = "";

        static int _jsonfilecount = 1;
        static int _retry = 1;

        static string menuOption = "";
        static bool menuOptionLoop = true;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            //Get FHIR data source settings
            _bundleStorageConnection = config["bundlestorageconnection"];
            _bundleContainer = config["bundlecontainer"];
            _bundleContainerconvert = config["bundlecontainerconvert"];  // urn:uuid converted to resource type

            //Get FHIR settings
            _fhirserversecurity = bool.Parse(config["fhirserversecurity"]);
            _fhirStorageConnection = config["fhirstorageconnection"];
            _fhirLogsContainer = config["fhirlogs"];
            _fhirExportContainer = config["fhirexport"];
            //_fhirFileFilter = config["filefilter"];
            //_fhirFileCountStart = int.Parse(config["filecountstart"]);
            //_fhirFileCountEnd = int.Parse(config["filecountend"]);
            _fhirMaxRetry = int.Parse(config["maxretry"]);

            _fhiraudience = config["fhiraudience"];
            _fhirtenantId = config["fhirtenantid"];
            _fhirclientId = config["fhirclientid"];
            _fhirclientSecret = config["fhirclientsecret"];
            _fhirlogin = config["fhirloginauthority"];
            _authority = _fhirlogin + "/" + _fhirtenantId;

            _streventhubconnection= config["eventhubconnection"];
            _streventhubname = config["eventhubname"];
            _deviceid = config["deviceid"];
            _patientid = config["patientid"];

            await DisplayMenuOptions();

        }

        private static async Task DisplayMenuOptions()
        {
            var menuItems = "\n**********************************************************\n";
            menuItems += "FHIR Data Loader (FHIRDL)\n";
            menuItems += "**********************************************************\n\n";
            menuItems += "Update appsettings.json before running the tool.\n";
            menuItems += "1. Convert Synthea FHIR Bundles in Azure Storage\n";
            menuItems += "2. Load FHIR Bundles (ndjson files after urn:uuid conversion)\n";
            menuItems += "3. Send simulated IoT data (heart rate)\n";
            menuItems += "q. Exit the program\n";

            while (menuOptionLoop)
                switch (menuOption)
                {
                    case "1":
                        menuOption = "";
                        await ProcessSyntheaData(true);
                        break;
                    case "2":
                        menuOption = "";
                        await ProcessSyntheaData(false);
                        break;
                    case "3":
                        menuOption = "";
                        await SendSimulatedEventHubData();
                        break;
                    case "q":
                        return;
                    default:
                        Console.WriteLine(menuItems);
                        menuOption = Console.ReadLine();
                        break;
                }
        }
        private static BlobContainerClient GetContainerReference(string storageConnection, string containerName, bool createcontainer)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnection);
                BlobContainerClient blobContainerClient;
                blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                if (createcontainer)
                {
                    blobContainerClient.CreateIfNotExists();
                }

                if (!blobContainerClient.Exists())
                {
                    Console.WriteLine($"Storage container {containerName} not found.");
                    return null;

                }
                return blobContainerClient;
            }
            catch
            {
                Console.WriteLine($"Unable to parse stroage reference or connect to storage account {storageConnection}.");
                return null;
            }
        }

        static private async Task ProcessSyntheaData(bool convertjson)
        {
            //reset counters
            _jsonfilecount = 1;
            _retry = 1;
            _fhirLoaderResponse = null;

            //get input data
            Console.WriteLine($"\nAzure Storage folder where json files are stored, e.g. fhir");
            _fhirFileFilter = Console.ReadLine();

            if (_fhirFileFilter == "exit")
            {
                return;
            }

            Console.WriteLine($"\nThe file position to start with (integer)");
            _fhirFileCountStart = int.Parse(Console.ReadLine());
            Console.WriteLine($"\nThe file position to end with (integer)");
            _fhirFileCountEnd = int.Parse(Console.ReadLine());

            var blobContainerClientConvert = GetContainerReference(_bundleStorageConnection, _bundleContainerconvert, true);
            var blobContainerClient = GetContainerReference(_bundleStorageConnection, _bundleContainer, false);
            
            if (convertjson)
            {
                Console.WriteLine($"\nConverting FHIR data...");
            }
            else
            {
                Console.WriteLine($"\nUploading FHIR data to {_fhiraudience}...");
                //use the container where files have been converted
                blobContainerClient = blobContainerClientConvert;

                if (_fhirserversecurity)
                {
                    Console.WriteLine($"Get AAD access token");
                    _accesstoken = await FHIRDLHelper.GetAADAccessToken(_authority, _fhirclientId, _fhirclientSecret, _fhiraudience, false);
                }
            }

            if (!string.IsNullOrEmpty(_fhirFileFilter))
                Console.Write($"Accessing files in storage folder {_fhirFileFilter}. ");

            Console.Write($"Starting at file #{_fhirFileCountStart} ending at file #{_fhirFileCountEnd}. ");

            Console.WriteLine($"Please wait...\n");

            
            // List all blobs in the container
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                //Skip json files not included by the filter
                if (!blobItem.Name.Contains(_fhirFileFilter))
                    continue;

                //Resume loading
                if (_jsonfilecount < _fhirFileCountStart)
                {
                    _jsonfilecount++;
                    continue;
                }

                //Stop loading 
                if (_jsonfilecount > _fhirFileCountEnd)
                {
                    Console.WriteLine($"All files within the specified range {_fhirFileCountStart} - {_fhirFileCountEnd} have been processed \n");
                    return;
                }
                
                Console.WriteLine($"Processing file #{_jsonfilecount} {blobItem.Name}");

                //// begin test only
                //_jsonfilecount++;
                //continue;
                ////end test only

                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                ////begin delete files
                //Console.WriteLine($"Delete file #{_jsonfilecount} {blobItem.Name}");
                //await blobClient.DeleteAsync();
                //_jsonfilecount++;
                //continue;
                ////end delete files

                BlobDownloadInfo download = await blobClient.DownloadAsync();

                ////begin copy files to a new folder
                //var newPrefix = "newfoldname";
                //var item = blobItem.Name;
                //var newBlobItemName = newPrefix + item.Substring(item.IndexOf("/"));
                //Console.WriteLine($"New file #{_jsonfilecount} {newBlobItemName}");
                //BlobClient destBlob = blobContainerClient.GetBlobClient(newBlobItemName);
                //await destBlob.UploadAsync(download.Content, true);
                //_jsonfilecount++;
                //continue;
                ////end copy files to a new folder

                var streamReader = new StreamReader(download.Content);

                if (convertjson)
                {
                    //read the entire json file when converting json files
                    _content = await streamReader.ReadToEndAsync();
                    _fhirLoaderResponse = FHIRDLHelper.ConvertJsonFiles(_content);

                    while (_fhirLoaderResponse is null)
                    {
                        if (_retry > _fhirMaxRetry)
                        {
                            Console.WriteLine($"Retry has exceeded the max limit. Exit now.");
                            break;
                        }

                        Console.WriteLine($"*******************  Retry due to error *******************");

                        _fhirLoaderResponse = FHIRDLHelper.ConvertJsonFiles(_content);
                        _retry++;
                    }

                    //copy the json bundle file to a new storage container
                    byte[] byteArray = Encoding.UTF8.GetBytes(_fhirLoaderResponse);
                    MemoryStream stream = new MemoryStream(byteArray);
                    BlobClient blobClientConvert = blobContainerClientConvert.GetBlobClient(blobItem.Name + ".ndjson");
                    await blobClientConvert.UploadAsync(stream, true);
                }
                else
                {
                    while (!streamReader.EndOfStream)
                    {
                        //read one line at a time and process it
                        _content = await streamReader.ReadLineAsync();
                        _fhirLoaderResponse = await FHIRDLHelper.LoadFHIRResource(_fhiraudience, _accesstoken, _content);

                        while (_fhirLoaderResponse is null)
                        {
                            if (_retry > _fhirMaxRetry)
                            {
                                Console.WriteLine($"Retry has exceeded the max limit. Exit now.");
                                break;
                            }

                            Console.WriteLine($"*******************  Retry due to error *******************");

                            if (_fhirserversecurity)
                            {
                                if (FHIRDLHelper.isTokenExpired(_accesstoken))
                                    _accesstoken = await FHIRDLHelper.GetAADAccessToken(_authority, _fhirclientId, _fhirclientSecret, _fhiraudience, false);
                                Console.WriteLine($"Get a new AAD access token");
                            }

                            _fhirLoaderResponse = await FHIRDLHelper.LoadFHIRResource(_fhiraudience, _accesstoken, _content);
                            _retry++;
                        }
                    }
                }

                _jsonfilecount++;

                }


            }


        static private async Task SendSimulatedEventHubData()
        {
            //https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send

            string _eventdata;
            // Create a producer client that you can use to send events to an event hub
            producerClient = new EventHubProducerClient(_streventhubconnection, _streventhubname);
            try
            {
                Console.WriteLine($"Sennding data to Azure Event Hub for IoT Connector\n");
                // Create event data
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                while (true)
                {
                    var _heartrate = new Random().Next(70, 100);

                    //https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
                    _eventdata = $"{{\"heartRate\": \"{_heartrate}\",\"endDate\": \"{DateTime.Now.ToUniversalTime().ToString("O")}\",\"deviceId\": \"{_deviceid}\",\"patientId\": \"{_patientid}\"}}";

                    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(_eventdata))))
                    {
                        // if it is too large for the batch
                        throw new Exception($"Event {_eventdata} cannot be sent.");
                    }

                    // Use the producer client to send the batch of events to the event hub
                    await producerClient.SendAsync(eventBatch);
                    
                    Console.WriteLine($"Event {_eventdata}");
                    Thread.Sleep(1000);

                }

            }

            finally
            {
                await producerClient.DisposeAsync();
            }

        }
        
    }


}


