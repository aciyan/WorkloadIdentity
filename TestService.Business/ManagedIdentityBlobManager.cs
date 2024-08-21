using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TestService.Business.Abstraction;

public class ManagedIdentityBlobManager: IManagedIdentityBlobManager
{
    #region  Private Properties

    // Blob Service Client
    private readonly BlobServiceClient _blobServiceClient;

    #endregion

    #region Public Constuctor

    public ManagedIdentityBlobManager(BlobServiceClient blobContainerClient)
    {
        _blobServiceClient = blobContainerClient;
    }

    #endregion

    #region Public Methods

    public async Task<bool> UploadBlobAsync(string containerName,
            string blobName,
            byte[] content,
            IDictionary<string, string> metadata = null,
            IDictionary<string, string> tags = null,
            string contentType = null)
    {
        if (content == null)
            return false;

        var blobClient = GetBlobClient(blobName, containerName);
        using var memoryStream = new MemoryStream(content);
        var result = await blobClient.UploadAsync(memoryStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders() { ContentType = contentType },
            Metadata = metadata,
            Tags = tags,
        });
            
        if(result == null)
        {
            throw new RequestFailedException($"Error while uploading {blobName} file to {containerName} container in Blob with exception");
        }
        return true;
    }

    public async Task<byte[]> DownloadBlobAsync(string containerName, string blobName)
    {
        var blobClient = GetBlobClient(blobName, containerName);
        if (!await blobClient.ExistsAsync())
        {
            throw new Exception($"Blob {blobName} not found in {containerName}.");
        }

        var response = await blobClient.DownloadStreamingAsync();
        using var memoryStream = new MemoryStream();
        using var responseValue = response.Value;
        responseValue.Content.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Get the blob client.
    /// </summary>
    /// <param name="blobName">Name of the blob .</param>
    /// <param name="containerName">Name of the container.</param>
    /// <returns>Created blob client</returns>
    private BlobClient GetBlobClient(string blobName, string containerName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentNullException(nameof(blobName));

        var blobContainerClient = GetBlobContainerClient(containerName);

        if(blobContainerClient == null)
        {
            throw new Exception("Blob container client is null");
        }
        return blobContainerClient.GetBlobClient(blobName);
    }

    /// <summary>
    /// Get the blob client.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <returns>Created blob client</returns>
    private BlobContainerClient GetBlobContainerClient(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentNullException(nameof(containerName));

        return _blobServiceClient.GetBlobContainerClient(containerName);
    }

    #endregion Private Methods
}
