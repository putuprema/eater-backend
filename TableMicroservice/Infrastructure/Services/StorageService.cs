using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Infrastructure.Services
{
    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public StorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<string> UploadQrStickerAsync(Stream stream, string fileName = default, CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("qr-stickers");
            var blob = containerClient.GetBlobClient(fileName);

            await blob.UploadAsync(stream,
                    options: new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" }
                    },
                    cancellationToken);

            return blob.Uri.AbsoluteUri;
        }

        public async Task DeleteAsync(string blobUrl)
        {
            string[] urlSplit = blobUrl
                .Replace("https://", "")
                .Split('/');

            var container = _blobServiceClient.GetBlobContainerClient(urlSplit[1]);
            await container.DeleteBlobIfExistsAsync(urlSplit[2]);
        }
    }
}
