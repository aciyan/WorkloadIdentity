namespace TestService.Business.Abstraction
{
    public interface IManagedIdentityBlobManager
    {
        Task<bool> UploadBlobAsync(string containerName,
            string blobName,
            byte[] content,
            IDictionary<string, string> metadata = null,
            IDictionary<string, string> tags = null,
            string contentType = null);
        Task<byte[]> DownloadBlobAsync(string containerName, string blobName);
    }
}
