using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadProductPhotoAsync(IFormFile file, string fileName = default, CancellationToken cancellationToken = default);
        Task DeleteAsync(string blobUrl);
    }
}
