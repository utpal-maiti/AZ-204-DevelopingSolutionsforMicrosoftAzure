using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CSSTDEvaluation;
using CSSTDModels;
using System.IO;
using Azure.Storage.Blobs;
#region "Advanced/Expert Storage Instructions"
/* 
* 1. The storage account connection string is passed in via the class constructor and assigned to the ConnectionString property. 
* 2. Implement the constructor to initialize a private variable of type CloudBlobClient to be used by all other methods 
* 3. Implement the UploadFile method to: 
*      a. Ensure that a container named by the containerName parameter exists and its access is set accoring to the isPrivate parameter 
*      b. Upload the file represented by the fileData parameter to the container named by the containerName parameter 
* 4. Implement the GetSAS method to: 
*      a. Generate a SAS token for the specified container.  The token should support read operations for at least 24 hours. 
*      b. Return the SAS token 
* 5. Implement the GetFileList method to: 
*      a. Retrieve the URLs of the files in the container named by the containerName parameter 
*      b. If the isPrivate parameter is true, call the GetSAS method to generate a SAS token 
*      c. Populate a List<BlobData> object with the name and URL of each file in the container.   
*      d. If the isPrivate parameter is true, the URL must contain the SAS token. 
*      e. Return the List<BlobData> object 
*  
* */#endregion  
#region "Data Structures"
/*         namespace CSSTDModels        
{            
    [Description("Object for transferring blob data into and out of the StorageContext class.")]            
    public class BlobFileData            
    {                
        [Description("Blob name.  Used for data in and data out.")]                
        public string Name { get; set; }                  
        
        [Description("Full Blob URL including SAS if appropriate. Used for data out.")]                
        public string URL { get; set; }                  
        
        [Description("SAS token.  Used for data out.")]                
        public string SAS { get; set; }                  
        
        [Description("Binary file contents used for upload.")]                
        public byte[] Contents { get; set; }                  
        
        [Description("Blob metadata used for advanced and expert challenges.")]                
        public List<string> Tags { get; set; }            
    }        
} 
* */#endregion  
namespace CSSTDSolution.Models
{
    public class StorageContext : IStorageContext
    {
        private string connectionString;
        public StorageContext(string connectionString) { this.connectionString = connectionString; }
        public void UploadFile(string containerName, BlobFileData fileData, bool isPrivate)
        {
            var container = new BlobContainerClient(this.connectionString, containerName);
            if (!container.Exists())
            {
                container.Create();
                var accessType = isPrivate ? Azure.Storage.Blobs.Models.PublicAccessType.None : Azure.Storage.Blobs.Models.PublicAccessType.Blob; container.SetAccessPolicy(accessType);
            }
            using (MemoryStream blobStream = new MemoryStream(fileData.Contents))
            { container.UploadBlob(fileData.Name, blobStream); }
        }
        public string GetSAS(string containerName)
        {
            // Create an instance of the BlobContainerClient class
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);

            // Create an instance of the List<BlobSignedIdentifier> class
            List<BlobSignedIdentifier> accessPolicies = new List<BlobSignedIdentifier>();

            // Create a BlobAccessPolicy named Read_Policy
            BlobAccessPolicy readPolicy = new BlobAccessPolicy
            {
                Permissions = "r", // Read access
                PolicyStartsOn = DateTimeOffset.UtcNow,
                PolicyExpiresOn = DateTimeOffset.UtcNow.AddDays(1) // Expires in 24 hours
            };

            // Add the access policy to the accessPolicies list
            accessPolicies.Add(new BlobSignedIdentifier
            {
                Id = "readpolicy",
                AccessPolicy = readPolicy
            });

            // Set the access policy on the container
            container.SetAccessPolicy(publicAccessType: PublicAccessType.None, signedIdentifiers: accessPolicies);

            // Create a SAS token for the container based on the Read_Policy
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24),
                StartsOn = DateTimeOffset.UtcNow
            };

            // Set permissions to Read
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // Sign the SAS token using the StorageSharedKeyCredential
            BlobSasQueryParameters sasQueryParameters = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("<account-name>", "<account-key>"));

            // Return the SAS token
            string sasToken = "?" + sasQueryParameters.ToString();
            return container.Uri + sasToken;
        }

        public List<BlobFileData> GetFileList(string containerName, bool isPrivate)
        {
            var results = new List<BlobFileData>();
            var container = new BlobContainerClient(this.connectionString, containerName);
            var sas = isPrivate ? GetSAS(containerName) : "";
            foreach (var blob in container.GetBlobs())
            {
                var blobClient = container.GetBlobClient(blob.Name);
                results.Add(new BlobFileData { Name = blobClient.Name, URL = blobClient.Uri.AbsoluteUri, SAS = sas });
            }
            return results;
        }
    }
}
