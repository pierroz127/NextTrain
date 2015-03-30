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
    public class BlobManager : ITweetIdManager
    {
        public void Save(ulong id)
        {
            CloudBlockBlob blockBlob = getBlockBlob("lastid.txt");
            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(id.ToString())))
            {
                blockBlob.UploadFromStream(memStream);
            }
        }

        public FSharpOption<UInt64> Read()
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
