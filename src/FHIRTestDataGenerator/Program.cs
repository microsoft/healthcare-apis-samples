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
using System.Collections;
using Newtonsoft.Json.Linq;

namespace HealthcareAPIsSamples.FHIRTDG
{
    class Program
    {
        //Get FHIR data source settings
        static string _strStorageConnection;
        static string _strContainer;
        static string _fhirFileFilter;
        static int _fhirFileCountStart;
        static int _fhirFileCountEnd;
        static string _content = null;
        static int _jsonfilecount = 1;

        //JMeter Performance Test Data
        static string _jsonfilelocation;
        static int _fhirresouretotal = 0;
        static string _splitters = "";
        static Dictionary<string, int>[] _fhirResourceGoal = new Dictionary<string, int>[4];
        static Dictionary<string, int>[] _fhirResourceCount = new Dictionary<string, int>[4];
        static List<string> _fhirResourceTestDataRAW = new List<string>();
        static List<string>[] _fhirResourceTestDataProcessed = new List<string>[4];
        static int[] _rowindex = new int[4] { 1, 1, 1, 1 };
        static string[] _operations = new string[4] { "Create", "Read", "Update", "Delete" };
        static int _crudindex = 0;
        static bool _boolCRUDCountContinue = false;
        static bool _boolDownloadJsonFile = false;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            //Get FHIR data source settings
            _strStorageConnection = config["azuretorageconnection"];
            _strContainer = config["ndjsonstoragecontainer"];

            _fhirFileFilter = config["filefilter"];
            _fhirFileCountStart = int.Parse(config["filecountstart"]);
            _fhirFileCountEnd = int.Parse(config["filecountend"]);

            _jsonfilelocation = config["jsonfilelocation"];
            _fhirresouretotal = int.Parse(config["fhirresourcetotal"]);

            //Read settings from config file
            for (int a = 0; a < 4; a++)
            {
                _fhirResourceCount[a] = new Dictionary<string, int>();
                _fhirResourceGoal[a] = new Dictionary<string, int>();
                _fhirResourceTestDataProcessed[a] = new List<string>();


                for (int i = 1; i <= _fhirresouretotal; i++)
                {
                    var _fhircrud = config["fhircrud" + i.ToString()];

                    _fhirResourceCount[a].Add(_fhircrud.Split(",")[0], 0);
                    _fhirResourceGoal[a].Add(_fhircrud.Split(",")[0], int.Parse(_fhircrud.Split(",")[a+1]));

                }
            }

            await ProcessSyntheaData();

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

        static private async Task ProcessSyntheaData()
        {
            //reset counters
            _jsonfilecount = 1;

            var blobContainerClient = GetContainerReference(_strStorageConnection, _strContainer, false);

            //JMeter performance test data

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
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                //Create JMeter performance test data
                System.IO.Directory.CreateDirectory(_jsonfilelocation);
                var streamReader = new StreamReader(download.Content);
                while (!streamReader.EndOfStream)
                {
                    //read one line at a time and process it
                    _content = await streamReader.ReadLineAsync();

                    JObject _objContent = JObject.Parse(_content);
                    if (_objContent == null)
                    {
                        throw new Exception("json file is empty, or missing bundle, entry block, transaction");
                    }

                    var _rt = (string)_objContent["resourceType"];
                    var _id = (string)_objContent["id"];
                    _boolDownloadJsonFile = false;

                    for (int i = 0; i < 4; i++)
                    {
                        if (_fhirResourceGoal[i].ContainsKey(_rt))
                        {
                            if (_fhirResourceCount[i][_rt] < _fhirResourceGoal[i][_rt])
                            {
                                _fhirResourceCount[i][_rt]++;
                                _fhirResourceTestDataRAW.Add(_rt + "," + _operations[i] + "," + _id);
                                _boolDownloadJsonFile = true;
                            }
                        }
                    }

                    if (_boolDownloadJsonFile)
                    {
                        await File.WriteAllTextAsync(_jsonfilelocation + "\\" + _id + ".json", _objContent.ToString());
                    }

                    //exit the while loop if no more test data is needed
                    _boolCRUDCountContinue = false;

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < _fhirResourceGoal[i].Count; j++)
                        {
                            if (_fhirResourceGoal[i].ElementAt(j).Value != _fhirResourceCount[i].ElementAt(j).Value)
                                _boolCRUDCountContinue = true;
                        }
                    }

                    //stop while loop
                    if (!_boolCRUDCountContinue) break;
                }


                //stop for loop
                if (!_boolCRUDCountContinue) break;

                _jsonfilecount++;

            }

            //Process collected data

            //Add row header strings
            for (int a=0; a < 4; a++)
            {
                _fhirResourceTestDataProcessed[a].Add(_fhirResourceGoal[a].ElementAt(0).Key);

                for (int b=1; b< _fhirresouretotal; b++)
                {
                    _fhirResourceTestDataProcessed[a][0] = _fhirResourceTestDataProcessed[a][0] + "," + _fhirResourceGoal[a].ElementAt(b).Key;
                }

                for (int i = 0; i < _fhirresouretotal; i++)
                {

                Array.Fill(_rowindex, 1);

                var _list = _fhirResourceTestDataRAW.Where(item => item.Contains(_fhirResourceGoal[a].ElementAt(i).Key + "," + _operations[a]));

                //Loop through the list by resource type in the specified order in the config file
                foreach (string _s in _list)
                {
                    var _fhirrt = _s.Split(",")[0];
                    var _op = _s.Split(",")[1];
                    var _fhirid = _s.Split(",")[2];

                    _crudindex = Array.IndexOf(_operations, _op);

                    if (_fhirResourceTestDataProcessed[a].Count == 1 || _rowindex[a] >= _fhirResourceTestDataProcessed[a].Count)
                    {
                        _splitters = "";
                        for (int k = 0; k < i; k++)
                        {
                            _splitters = _splitters + ",";
                        }
                        _fhirResourceTestDataProcessed[a].Add(_splitters + _fhirid);

                    }
                    else
                    {
                        var _txt = _fhirResourceTestDataProcessed[a][_rowindex[a]].Split(',');

                        _splitters = "";
                        for (int k = 0; k < i - _txt.Length + 1; k++)
                        {
                            _splitters = _splitters + ",";
                        }

                        _fhirResourceTestDataProcessed[a][_rowindex[a]] = _fhirResourceTestDataProcessed[a][_rowindex[a]] + _splitters + _fhirid;
                    }

                    _rowindex[a]++;

                }
                }

                //Save data to local file
                using StreamWriter file = new(_jsonfilelocation + "\\" + _operations[a] + ".csv");
                foreach (string line in _fhirResourceTestDataProcessed[a])
                {
                        await file.WriteLineAsync(line);
                }
            }

            //for (int a = 0; a < 4; a++)
            //{ 
            //    for (int b = 0; b < _fhirResourceTestDataProcessed[a].Count; b++)
            //    {
            //        Console.WriteLine($"{_fhirResourceTestDataProcessed[a][b]}");
            //    }
            //}

        }
        static private void CreateEmptyFile(string filename)
        {
            File.Create(filename).Dispose();
        }

    }

    
}


