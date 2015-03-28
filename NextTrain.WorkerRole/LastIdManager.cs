using System;
using System.IO;
using System.Text;
using Microsoft.FSharp.Core;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NextTrain.Lib;

namespace NextTrain.WorkerRole
{
    public class LastIdManager : ITweetIdManager
    {
        public void Save(ulong id)
        {
            CloudBlockBlob blockBlob = getLastIdBlockBlob();
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(id.ToString())))
            {
                blockBlob.UploadFromStream(memStream);
            }
        }

        public FSharpOption<UInt64> Read()
        {
            var blockBlob = getLastIdBlockBlob();

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

        private CloudBlockBlob getLastIdBlockBlob()
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
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("lastid.txt");
            return blockBlob;
        }
    }
}
