namespace Application.Common.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadQrStickerAsync(Stream stream, string fileName = default, CancellationToken cancellationToken = default);
        Task DeleteAsync(string blobUrl);
    }
}
