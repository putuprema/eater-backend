namespace Application.Common.Interfaces
{
    public interface IQrCodeService
    {
        Task<string> GenerateQrStickerAsync(Table table, CancellationToken cancellationToken = default);
    }
}
