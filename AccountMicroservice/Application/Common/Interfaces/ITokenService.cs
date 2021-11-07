using Application.Accounts.Commands.Auth;

namespace Application.Common.Interfaces
{
    public interface ITokenService
    {
        public string GenerateAccessToken(Account account);
        public Task<string> GenerateRefreshTokenAsync(Account account, CancellationToken cancellationToken = default);
        public Task<AuthResultDto> RefreshAccessTokenAsync(string refreshToken, Role userRole, CancellationToken cancellationToken = default);
    }
}
