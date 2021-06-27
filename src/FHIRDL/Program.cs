// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading;
using System.Collections;
using Microsoft.Extensions.Configuration;
using Azure;

namespace HealthcareAPIsSamples
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            //Get FHIR data source settings
            string _bundleStorageConnection = config["bundlestorageconnection"];
            string _bundleContainer = config["bundlecontainer"];
            string _bundleContainerconverted = config["bundlecontainerconverted"];  // urn:uuid converted to resource type

            //Get FHIR settings
            bool _fhirserversecurity = bool.Parse(config["fhirserversecurity"]);
            string _fhirStorageConnection = config["fhirstorageconnection"];
            string _fhirLogsContainer = config["fhirlogs"];
            string _fhirExportContainer = config["fhirexport"];
            string _fhirFileFilter = config["filefilter"];
            int _fhirFileCountStart = int.Parse(config["filecountstart"]);
            int _fhirMaxRetry = int.Parse(config["maxretry"]);

            string _fhiraudience = config["fhiraudience"];
            string _fhirtenantId = config["fhirtenantid"];
            string _fhirclientId = config["fhirclientid"];
            string _fhirclientSecret = config["fhirclientsecret"];
            string _fhirlogin = config["fhirloginauthority"];
            //string _authority = "https://login.microsoftonline.com" + "/" + _fhirtenantId;
            string _authority = _fhirlogin + "/" + _fhirtenantId;

            string _accesstoken = null;
            string _requestUrl = null;
            HttpClient _client = new HttpClient();
            HttpRequestMessage _request;
            HttpResponseMessage _response;

            string _exportResponseLocation;
            string _logStorage = null;
            string _msg = null;
            string _content = null;
            string _fhirLoaderResponse = "";

            int _jsonfilecount = 1;
            int _logfilecount = 1;
            int _logfilesize = 5000;
            int _retry = 1;


            var menuOption = "";
            bool menuOptionValid = false;
            var menuItems = "\n**********************************************************\n";
            menuItems += "FHIR Data Loader (FHIRDL)\n";
            menuItems += "**********************************************************\n\n";
            menuItems += "Update appsettings.json before running the tool.\n";
            menuItems += "0. Convert Synthea FHIR Bundles in Azure Storage\n";
            menuItems += "1. Load FHIR Bundles (json files)\n";
            menuItems += "2. Load FHIR Bundles (ndjson files)\n";
            menuItems += "3. Export FHIR Resources to Azure Storage\n";
            menuItems += "4. Delete FHIR Resources based on $export NDJson files\n";
            menuItems += "q. Exit the program\n";

            while (!menuOptionValid)
                switch (menuOption)
                {
                    case "0":
                        ConvertSyntheaData();
                        break;
                    case "1":
                        LoadJsonData();
                        break;
                    case "2":
                        LoadNDJsonData();
                        break;
                    case "3":
                        ExportFHIRData();
                        break;
                    case "4":
                        DeleteFHIRData();
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
                    Dictionary<string, FHIRResourceHelper.ResourceRefPair> _dict = new Dictionary<string, FHIRResourceHelper.ResourceRefPair>();

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
                            _dict.TryAdd(_id, new FHIRResourceHelper.ResourceRefPair
                            {
                                Id = _id,
                                ResourceType = _rt
                            });
                            //Console.WriteLine($"{_alBlobs[i]} {_rt} {_id}");
                        }

                        //Delete all resources for each ndjson file
                        _msg = await FHIRResourceHelper.DeleteFHIRResource(_fhiraudience, _accesstoken, _dict);
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

        static private void ConvertSyntheaData()
        {

            BlobServiceClient blobServiceClient = new BlobServiceClient(_bundleStorageConnection);
                BlobContainerClient blobContainerClient;
                blobContainerClient = blobServiceClient.GetBlobContainerClient(_bundleContainer);
                if (!blobContainerClient.Exists())
                {
                    Console.WriteLine($"Bundle storage container not found.");
                    return;
                }

                //BlobContainerClient destContainerClient = blobServiceClient.GetBlobContainerClient(_fhirBundlesProcessedContainer);
                //if (!destContainerClient.Exists())
                //    destContainerClient = blobServiceClient.CreateBlobContainer(_fhirBundlesProcessedContainer);

                _msg += $"Loading started: {DateTime.Now}\n";
                // List all blobs in the container

                Console.WriteLine($"\nUploading FHIR data to {_fhiraudience}. ");
                Console.Write($"Uploading starting at file #{_fhirFileCountStart}. ");
                if (!string.IsNullOrEmpty(_fhirFileFilter))
                    Console.Write($"Filtering files with {_fhirFileFilter}. ");

                Console.WriteLine($"Please wait...\n");

                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    //Only process filtered json files
                    if (!blobItem.Name.Contains(_fhirFileFilter))
                        continue;

                    //Resume loading manually
                    if (_jsonfilecount < _fhirFileCountStart)
                    {
                        //Console.WriteLine($"Skipping file #{_jsonfilecount} {blobItem.Name}");
                        _jsonfilecount++;
                        continue;
                    }

                    _msg += $"Loading {blobItem.Name} \n";
                    Console.WriteLine($"Uploading file #{_jsonfilecount} {blobItem.Name}");

                    //if (AADHelper.isTokenExpired(_accesstoken))
                    //{
                    //    _accesstoken = await AADHelper.GetAADAccessToken(_authority, _fhirclientId, _fhirclientSecret, _fhiraudience, false);
                    //    Console.WriteLine($"Renew access token ...");
                    //}

                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    BlobDownloadInfo download = await blobClient.DownloadAsync();
                    var streamReader = new StreamReader(download.Content);
                    _content = await streamReader.ReadToEndAsync();

                    _fhirLoaderResponse = await FHIRDLHelper.LoadFHIRResource(_fhiraudience, _accesstoken, blobItem.Name, _content);

                    _retry = 1;

                    while (_fhirLoaderResponse != "Completed")
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

                        _fhirLoaderResponse = await FHIRDLHelper.LoadFHIRResource(_fhiraudience, _accesstoken, blobItem.Name, _content);

                        _retry++;
                    }

                    if (_jsonfilecount % _logfilesize == 0)
                    {
                        _msg += $"Loading finished: {DateTime.Now}\n";
                        await AzureStorageHelper.LogToStorage(_fhirStorageConnection, _fhirLogsContainer, "log" + _logfilecount.ToString() + "_" + Guid.NewGuid() + ".txt", _msg);
                        Console.WriteLine($"{_jsonfilecount} files processed");
                        _msg = $"Loading started: {DateTime.Now}\n";
                        _logfilecount++;
                    }

                    _jsonfilecount++;
                }

                if (_jsonfilecount % _logfilesize != 0)
                {
                    _msg += $"Loading finished: {DateTime.Now}\n";
                    await AzureStorageHelper.LogToStorage(_fhirStorageConnection, _fhirLogsContainer, "log" + _logfilecount.ToString() + "_" + Guid.NewGuid() + ".txt", _msg);

                }

            }



    }

        static private void LoadJsonData()
        {

        }

        static private void LoadNDJsonData()
        {

        }

        static private void ExportFHIRData()
        {

        }
        static private void DeleteFHIRData()
        {

        }

    private static CloudBlockBlob GetBlobReference(string connectionString, string containerName, string blobName)
    {
        CloudStorageAccount storageAccount;
        if (CloudStorageAccount.TryParse(connectionString, out storageAccount))
        {
            try
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                var container = cloudBlobClient.GetContainerReference(containerName);
                var blockBlob = container.GetBlockBlobReference(blobName);
                return blockBlob;
            }
            catch
            {
                log.LogCritical("Unable to get blob reference");
                return null;
            }
        }
        else
        {
            log.LogCritical("Unable to parse connection string and create storage account reference");
            return null;
        }

    }

}


}

