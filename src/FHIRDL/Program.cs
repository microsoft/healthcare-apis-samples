// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading;
using System.Collections;
using Microsoft.Extensions.Configuration;
using Azure;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace HealthcareAPIsSamples
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
        static int _fhirMaxRetry;

        static string _fhiraudience;
        static string _fhirtenantId;
        static string _fhirclientId;
        static string _fhirclientSecret;
        static string _fhirlogin;
        static string _authority;

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
            _fhirFileFilter = config["filefilter"];
            _fhirFileCountStart = int.Parse(config["filecountstart"]);
            _fhirMaxRetry = int.Parse(config["maxretry"]);

            _fhiraudience = config["fhiraudience"];
            _fhirtenantId = config["fhirtenantid"];
            _fhirclientId = config["fhirclientid"];
            _fhirclientSecret = config["fhirclientsecret"];
            _fhirlogin = config["fhirloginauthority"];
            _authority = _fhirlogin + "/" + _fhirtenantId;

            var menuOption = "";
            bool menuOptionValid = false;
            var menuItems = "\n**********************************************************\n";
            menuItems += "FHIR Data Loader (FHIRDL)\n";
            menuItems += "**********************************************************\n\n";
            menuItems += "Update appsettings.json before running the tool.\n";
            menuItems += "0. Convert Synthea FHIR Bundles in Azure Storage\n";
            menuItems += "1. Load FHIR Bundles (json files after urn:uuid conversion)\n";
            menuItems += "2. Load FHIR Bundles (ndjson files after urn:uuid conversion)\n";
            menuItems += "3. Export FHIR Resources to Azure Storage\n";
            menuItems += "4. Delete FHIR Resources based on $export NDJson files\n";
            menuItems += "q. Exit the program\n";

            while (!menuOptionValid)
                switch (menuOption)
                {
                    case "0":
                        await ProcessSyntheaData(true);
                        break;
                    case "1":
                        await ProcessSyntheaData(false);
                        break;
                    case "2":
                        //LoadNDJsonData();
                        break;
                    case "3":
                        //ExportFHIRData();
                        break;
                    case "4":
                        //DeleteFHIRData();
                        break;
                    case "q":
                        return;
                    default:
                        Console.WriteLine(menuItems);
                        menuOption = Console.ReadLine();
                        break;
                }

            try
            {
                if (menuOption == "2")
                {
                    //_fhirExportContainer
                    _requestUrl = _fhiraudience + "/$export?_container=" + _fhirExportContainer;

                    _msg = $"BundleLoader: Export FHIR resources start time: " + DateTime.Now + "\n";
                    _logStorage = _msg + "\n"; //Initialize the log message

                    _request = new HttpRequestMessage(HttpMethod.Get, _requestUrl);
                    _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accesstoken);
                    _request.Headers.Add("Accept", "application/fhir+json");
                    _request.Headers.Add("Prefer", "respond-async");
                    _request.Content = new StringContent("application/json");

                    _response = _client.SendAsync(_request).Result;

                    switch (_response.StatusCode)
                    {
                        case HttpStatusCode.Accepted:
                            {
                                _msg = $"FHIRDataLoader: Export has started ...";
                                _logStorage += _msg + "\n";

                                //https://xxx.azurehealthcareapis.com/_operations/export/fcbb62d8-e5ae-48ae-83bd-41c4ed88fc75
                                _exportResponseLocation = _response.Content.Headers.ContentLocation.ToString();
                                _fhirExportContainer = _exportResponseLocation.Substring(_exportResponseLocation.LastIndexOf("/")+1);

                                _msg = $"FHIRDataLoader: Export FHIR content location: " + _exportResponseLocation;
                                _logStorage += _msg + "\n";


                                _msg = $"FHIRDataLoader: Export FHIR storage container: " + _fhirExportContainer;
                                _logStorage += _msg + "\n";


                                bool _exportDone = false;
                                while (_exportDone)
                                {
                                    HttpRequestMessage _checkMsg = new HttpRequestMessage(HttpMethod.Get, _exportResponseLocation);
                                    _checkMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accesstoken);
                                    HttpResponseMessage _checkresponse = _client.SendAsync(_checkMsg).Result;
                                    if (_checkresponse.StatusCode == HttpStatusCode.OK)
                                    {
                                        _msg = $"FHIRDataLoader: Export FHIR resources finish time: " + DateTime.Now + "\n";
                                        _logStorage += _msg + "\n";
                                        _exportDone = true;

                                    }
                                        
                                    else
                                    {
                                        Console.WriteLine("FHIRDataLoader: Export waiting for 5 seconds...");
                                        Thread.Sleep(5000);
                                    }

                                }

                                _msg = $"FHIRDataLoader: Export has completed ...";
                                _logStorage += _msg + "\n";
                                Console.WriteLine(_msg);

                                break;
                            }
                        case HttpStatusCode.OK:
                            {
                            break;
                            }
                        default:
                            throw new Exception("FHIRDataLoader: FHIR Export connection failed.");
                            //break;
                    }

                    _msg = $"FHIRDataLoader: Export FHIR resources finish time: " + DateTime.Now + "\n";
                    _logStorage += _msg + "\n"; 

                }

                if (menuOption == "3")
                {
                    //Use FHIR export feature to create jdson files which are stored in a storage container (GUI based)

                    // Create a BlobServiceClient object which will be used to create a container client
                    BlobServiceClient blobServiceClient = new BlobServiceClient(_fhirStorageConnection);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_fhirExportContainer);


                    ArrayList _alVirtualFolders = new ArrayList();
                    ArrayList _alBlobs = new ArrayList();
                    //Dictionary<string, string> _dictBlobs = new Dictionary<string, string>();

                    ////List all containers in the storage account
                    //await foreach (BlobContainerItem _bci in blobServiceClient.GetBlobContainersAsync())
                    //{
                    //    //if (_bci.Name.Length == 36)  //list GUI based containers only
                    //    {
                    //        _dictcontainers.Add(_bci.Name, _bci.Properties.LastModified.ToString());
                    //    }
                    //}

                    // Call the listing operation and return pages of the specified size.
                    var resultSegment = containerClient.GetBlobsByHierarchyAsync(prefix: null, delimiter: "/")
                        .AsPages(default, null);

                    int _vindex = 1;
                    Console.WriteLine();

                    // Enumerate the blobs returned for each page.
                    await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                    {
                        // A hierarchical listing may return both virtual directories and blobs.
                        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                        {
                            if (blobhierarchyItem.IsPrefix)
                            {
                                //_dictcontainers.Add(_fhirExportContainer, blobhierarchyItem.Prefix);

                                _alVirtualFolders.Add(blobhierarchyItem.Prefix);

                                // Write out the prefix of the virtual directory.
                                Console.WriteLine($"{_vindex}:  {blobhierarchyItem.Prefix}");

                                // Call recursively with the prefix to traverse the virtual directory.
                                //await ListBlobsHierarchicalListing(container, blobhierarchyItem.Prefix, null);
                            }
                            //else
                            //{
                            //    // Write out the name of the blob.
                            //    Console.WriteLine("Blob name: {0}", blobhierarchyItem.Blob.Name);
                            //}

                            _vindex++;
                        }

                        //Console.WriteLine();
                    }

                    //if (_dictcontainers.Count == 0)
                    //    throw new Exception("FHIRDataLoader: No FHIR export container found. Please export the resources first.");

                    //var _sortedContainers = from p in _dictcontainers orderby p.Value descending select p;

                    //int _intSelectOption = 0;
                    //ArrayList _alContainers = new ArrayList();

                    //int _cc;


                    //while ((_intSelectOption <= 0) || (_intSelectOption > _dictcontainers.Count))
                    //{
                    //    Console.WriteLine("\nFHIRDataLoader: Please enter a number to select the FHIR export folder (with ndjson files)\n");
                    //    _alContainers.Clear();
                    //    _cc = 0;
                    //    foreach (KeyValuePair<string, string> _s in _sortedContainers)
                    //    {
                    //        _alContainers.Add(_s.Key);
                    //        _cc++;
                    //        Console.WriteLine("{0}: {1} -- created on {2}", _cc.ToString(), _s.Key, _s.Value);
                    //    }

                    //    Console.WriteLine("\n");
                    //    var _subMenuOption = Console.ReadLine();
                    //    int.TryParse(_subMenuOption, out _intSelectOption);
                    //}

                    //_fhirExportContainer = (string)_alContainers[(_intSelectOption - 1)];            

                    //// Create the container and return a container client object
                    //containerClient = blobServiceClient.GetBlobContainerClient(_fhirExportContainer);
                    //if (!containerClient.Exists())
                    //    throw new Exception("FHIRDataLoader: FHIR export storage location not found.");

                    //Use Dictionary to store resource id and resource type 
                    Dictionary<string, FHIRDLHelper.ResourceRefPair> _dict = new Dictionary<string, FHIRDLHelper.ResourceRefPair>();

                    //Console.WriteLine("FHIRDataLoader: Resources in container " + _fhirExportContainer + " are to be deleted.");
                    //Console.WriteLine("FHIRDataLoader: Deleting FHIR resources started");

                    int _intSelectOption = 0;
                    while (_intSelectOption <= 0 || _intSelectOption > _alVirtualFolders.Count)
                    {
                        Console.WriteLine("\nPlease select the virtual directory to start the delete job");
                        var _subMenuOption = Console.ReadLine();
                        int.TryParse(_subMenuOption, out _intSelectOption);
                    }

                    var resultSegment2 = containerClient.GetBlobsByHierarchyAsync(prefix: (string)_alVirtualFolders[_intSelectOption - 1], delimiter: "/")
                        .AsPages(default, null);
                    await foreach (Page<BlobHierarchyItem> blobPage in resultSegment2)
                    {
                        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                        {
                            if (!blobhierarchyItem.IsPrefix)
                            {
                                //_dictBlobs.Add(_fhirExportContainer, blobhierarchyItem.Blob.Name);
                                _alBlobs.Add(blobhierarchyItem.Blob.Name);
                                //Console.WriteLine("Blob name: {0}", blobhierarchyItem.Blob.Name);
                            }

                        }
                    }

                    // List all blobs in the container
                    for (int i = 0; i < _alBlobs.Count; i++)
                    //await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                    {
                        _msg = $"njdson file: " + _alBlobs[i]; // blobItem.Name;
                        _logStorage = _msg + "\n"; //Initialize the log message

                        _msg = $"FHIRDataLoader: start time: " + DateTime.Now;
                        _logStorage += _msg + "\n"; 
                        //Console.WriteLine(_msg);

                        BlobClient blobClient = containerClient.GetBlobClient(_alBlobs[i].ToString());
                        BlobDownloadInfo download = await blobClient.DownloadAsync();
                        var streamReader = new StreamReader(download.Content);

                        while (!streamReader.EndOfStream)
                        {
                            _content = await streamReader.ReadLineAsync();
                            JObject _objLineContent = JObject.Parse(_content);

                            string _rt = (string)_objLineContent["resourceType"];
                            string _id = (string)_objLineContent["id"];

                            //save id and resource type to dictionary for each ndjson file
                            _dict.TryAdd(_id, new FHIRDLHelper.ResourceRefPair
                            {
                                Id = _id,
                                ResourceType = _rt
                            });
                            //Console.WriteLine($"{_alBlobs[i]} {_rt} {_id}");
                        }

                        //Delete all resources for each ndjson file
                        //_msg = await FHIRDLHelper.DeleteFHIRResource(_fhiraudience, _accesstoken, _dict);
                        _logStorage += _msg + "\n";

                        ////archive and delete ndjson files
                        //if (!string.IsNullOrEmpty(_fhirNDJsonProcessedContainer))
                        //{
                        //    //copy to archive container
                        //    BlobContainerClient destContainerClient = blobServiceClient.GetBlobContainerClient(_fhirNDJsonProcessedContainer);
                        //    if (!destContainerClient.Exists())
                        //        destContainerClient = blobServiceClient.CreateBlobContainer(_fhirNDJsonProcessedContainer);
                        //    BlobClient destBlob = destContainerClient.GetBlobClient(blobItem.Name);
                        //    //await destBlob.StartCopyFromUriAsync(blobClient.Uri);
                        //    await destBlob.UploadAsync(download.Content, true);

                        //    _msg = $"ndjson file archived ...";
                        //    _logStorage += _msg + "\n";
                        //    Console.WriteLine(_msg);


                        //    //delete the blob
                        //    await blobClient.DeleteAsync();

                        //    _msg = $"ndjson file deleted ...";
                        //    _logStorage += _msg + "\n";
                        //    Console.WriteLine(_msg);

                        //}


                        _msg = $"FHIRDataLoader: finish time: " + DateTime.Now;
                        _logStorage += _msg + "\n";
                        //Console.WriteLine(_msg);

                        //await AzureStorageHelper.LogToStorage(_fhirStorageConnection, _fhirLogsContainer, blobItem.Name + ".log.txt", _logStorage);
                    }

                    Console.WriteLine("FHIRDataLoader: Deleting FHIR resources finished !!! \n");

                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n");

            }
            finally
            {

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
            var blobContainerClientConvert = GetContainerReference(_bundleStorageConnection, _bundleContainerconvert, true);
            var blobContainerClient = GetContainerReference(_bundleStorageConnection, _bundleContainer, false);
            if (convertjson)
            {
                Console.WriteLine($"\nConverting FHIR data.");
                
            }
            else
            {
                Console.WriteLine($"\nUploading FHIR data to {_fhiraudience}.");
                //use the container where files have been converted
                blobContainerClient = blobContainerClientConvert;
            }

            if (!string.IsNullOrEmpty(_fhirFileFilter))
                Console.Write($"Accessing files in storage folder {_fhirFileFilter}. ");

            Console.Write($"Starting at file #{_fhirFileCountStart}. ");

            Console.WriteLine($"Please wait...\n");

            
            // List all blobs in the container
            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                //Only process filtered json files
                if (!blobItem.Name.Contains(_fhirFileFilter))
                    continue;

                //Resume loading manually
                if (_jsonfilecount < _fhirFileCountStart)
                {
                    _jsonfilecount++;
                    continue;
                }

                Console.WriteLine($"Processing file #{_jsonfilecount} {blobItem.Name}");

                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                BlobDownloadInfo download = await blobClient.DownloadAsync();
                var streamReader = new StreamReader(download.Content);
                _content = await streamReader.ReadToEndAsync();

                _fhirLoaderResponse = await FHIRDLHelper.ProcessFHIRResource(_fhiraudience, _accesstoken, _content, convertjson);

                _retry = 1;

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

                        Console.WriteLine($"Get a new access token");

                    }

                    _fhirLoaderResponse = await FHIRDLHelper.ProcessFHIRResource(_fhiraudience, _accesstoken, _content, convertjson);
                    _retry++;
                }

                if (convertjson)
                {
                    //copy the json bundle file to a new storage container
                    byte[] byteArray = Encoding.UTF8.GetBytes(_fhirLoaderResponse);
                    MemoryStream stream = new MemoryStream(byteArray);
                    BlobClient blobClientConvert = blobContainerClientConvert.GetBlobClient(blobItem.Name + ".ndjson");
                    await blobClientConvert.UploadAsync(stream, true);

                }

                _jsonfilecount++;

                }


            }



    }


}


