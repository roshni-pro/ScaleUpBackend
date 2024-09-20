using Azure.Storage.Blobs;
using ScaleUP.Services.MediaAPI.Constants;

namespace ScaleUP.Services.MediaAPI.Helper
{
    public static class AzureBlobService
    {
        public static async Task<string> UploadFile(Stream uploadFileStream, string fileName)
        {
            string Azureconnectionstring = EnvironmentConstants.Azureconnectionstring;
            string AzurecontainerName = EnvironmentConstants.AzurecontainerName;
            var serviceClient = new BlobServiceClient(Azureconnectionstring);
            var containerClient = serviceClient.GetBlobContainerClient(AzurecontainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
            return blobClient.Uri.OriginalString;
        }
    }
}
