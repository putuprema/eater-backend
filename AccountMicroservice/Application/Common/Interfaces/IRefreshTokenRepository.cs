namespace Application.Common.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> GetTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<bool> TryDeleteTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task UpsertTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    }
}
