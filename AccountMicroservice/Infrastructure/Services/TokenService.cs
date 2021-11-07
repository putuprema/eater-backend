using Application.Accounts.Commands.Auth;
using Application.Common.Utils;
using Infrastructure.Config;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAccountRepository _accountRepository;

        public TokenService(IOptions<JwtConfig> jwtOpts, IRefreshTokenRepository refreshTokenRepository, IAccountRepository accountRepository)
        {
            _jwtConfig = jwtOpts.Value;
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            _refreshTokenRepository = refreshTokenRepository;
            _accountRepository = accountRepository;
        }

        public string GenerateAccessToken(Account account)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.DisplayName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.EmployeeRole != null ? $"{account.Role}_{account.EmployeeRole}" : account.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.ValidIssuer,
                expires: DateTime.Now.AddSeconds(_jwtConfig.Lifetime),
                claims: authClaims,
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(Account account, CancellationToken cancellationToken = default)
        {
            var refreshToken = new RefreshToken { UserId = account.Id, Token = Utils.GenerateRandomToken() };
            await _refreshTokenRepository.UpsertTokenAsync(refreshToken, cancellationToken);
            return refreshToken.Token;
        }

        public async Task<AuthResultDto> RefreshAccessTokenAsync(string refreshToken, Role userRole, CancellationToken cancellationToken = default)
        {
            var refreshTokenObj = await _refreshTokenRepository.GetTokenAsync(refreshToken, cancellationToken);
            if (refreshTokenObj == null)
                return null;

            var account = await _accountRepository.GetByIdAndRoleAsync(refreshTokenObj.UserId, userRole, cancellationToken);
            if (account == null)
                return null;

            _ = _refreshTokenRepository.TryDeleteTokenAsync(refreshToken, cancellationToken);

            return new AuthResultDto
            {
                AccessToken = GenerateAccessToken(account),
                RefreshToken = await GenerateRefreshTokenAsync(account, cancellationToken)
            };
        }
    }
}
