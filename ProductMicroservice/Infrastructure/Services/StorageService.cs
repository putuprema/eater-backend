using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infrastructure.Config;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class StorageService : IStorageService
    {
        private readonly BlobStorageConfig _blobStorageConfig;
        private readonly BlobServiceClient _blobSericeClient;

        public StorageService(BlobServiceClient blobServiceClient, IOptions<BlobStorageConfig> blobStorageConfig)
        {
            _blobSericeClient = blobServiceClient;
            _blobStorageConfig = blobStorageConfig.Value;
        }

        private void ValidateFileSize(IFormFile file)
        {
            if (file.Length / 1000000 > _blobStorageConfig.MaxFileSizeMb)
            {
                throw new BadRequestException("Uploaded file is too large");
            }
        }

        public async Task<string> UploadProductPhotoAsync(IFormFile file, string fileName = default, CancellationToken cancellationToken = default)
        {
            if (!file.ContentType.StartsWith("image/"))
            {
                throw new BadRequestException("Uploaded file is not an image");
            }

            ValidateFileSize(file);

            var containerClient = _blobSericeClient.GetBlobContainerClient("products");
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blob = containerClient.GetBlobClient(fileName ?? file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream,
                    options: new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                    },
                    cancellationToken);
            }

            return blob.Uri.AbsoluteUri;
        }

        public async Task DeleteAsync(string blobUrl)
        {
            string[] urlSplit = blobUrl
                .Replace("https://", "")
                .Split('/');

            var container = _blobSericeClient.GetBlobContainerClient(urlSplit[1]);
            await container.DeleteBlobIfExistsAsync(urlSplit[2]);
        }
    }
}
