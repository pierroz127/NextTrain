//   Copyright 2015 Pierre Leroy
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.IO;
using System.Text;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NextTrain.Lib;

namespace NextTrain.WorkerRole
{
    public class BlobManager : IDataManager
    {
        public void SaveId(ulong id)
        {
            CloudBlockBlob blockBlob = getBlockBlob("lastid.txt");
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(id.ToString())))
            {
                blockBlob.UploadFromStream(memStream);
            }
        }

        public FSharpOption<UInt64> ReadId()
        {
            var blockBlob = getBlockBlob("lastid.txt");

            if (blockBlob.Exists())
            {
                using (var memStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memStream);
                    UInt64 lastId;
                    if (UInt64.TryParse(Encoding.UTF8.GetString(memStream.ToArray()), out lastId))
                    {
                        return new FSharpOption<ulong>(lastId);
                    }
                }
            }
            return null;
        }

        public TagCache ReadCache()
        {
            var blockBlob = getBlockBlob("tagCache.json");
            if (blockBlob.Exists())
            {
                using (var memStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memStream);
                    TagCache tagCache = new TagCache();
                    string json = Encoding.UTF8.GetString(memStream.ToArray());
                    return tagCache.fromJson(json);
                }
            }
            return null;
        }

        public void SaveCache(TagCache tagCache)
        {
            CloudBlockBlob blockBlob = getBlockBlob("tagCache.json");
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(tagCache.toJson())))
            {
                blockBlob.UploadFromStream(memStream);
            }
        }

        public static Unit LogTweet(string msg)
        {
            StringBuilder str = new StringBuilder();
            var blob = getBlockBlob(string.Format("tweetLogs-{0}.csv", DateTime.UtcNow.ToString("yyyyMMdd")));
            if (blob.Exists())
            {
                str.Append(blob.DownloadText(Encoding.UTF8));
            }
            str.AppendLine(string.Format("{0};{1}", DateTime.UtcNow.ToString("u"), msg));
            blob.UploadText(str.ToString(), Encoding.UTF8);
            return null;
        }

        private static CloudBlockBlob getBlockBlob(string blobName)
        {
            CloudStorageAccount storageAccount = 
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            
            // create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            
            // retrieve reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference("nexttrain");
            
            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
            
            // Retrieve reference to the "lastId" blob named 
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            return blockBlob;
        }
    }
}
