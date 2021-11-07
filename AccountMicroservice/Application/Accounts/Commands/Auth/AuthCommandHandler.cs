namespace Application.Accounts.Commands.Auth
{
    public class AuthCommandHandler : IRequestHandler<AuthCommand, AuthResultDto>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordEncoderService _passwordEncoderService;
        private readonly ITokenService _tokenService;

        public AuthCommandHandler(IAccountRepository accountRepository, IPasswordEncoderService passwordEncoderService, ITokenService tokenService)
        {
            _accountRepository = accountRepository;
            _passwordEncoderService = passwordEncoderService;
            _tokenService = tokenService;
        }

        public async Task<AuthResultDto> Handle(AuthCommand request, CancellationToken cancellationToken)
        {
            if (request.GrantType == GrantType.password.ToString())
            {
                var account = await _accountRepository.GetByEmailAndRoleAsync(request.Email, Enum.Parse<Role>(request.Role), cancellationToken: cancellationToken);
                if (account == null || !_passwordEncoderService.Matches(request.Password, account.Password))
                    return null;

                return new AuthResultDto
                {
                    AccessToken = _tokenService.GenerateAccessToken(account),
                    RefreshToken = await _tokenService.GenerateRefreshTokenAsync(account, cancellationToken),
                };
            }
            else
            {
                return await _tokenService.RefreshAccessTokenAsync(request.RefreshToken, Enum.Parse<Role>(request.Role), cancellationToken: cancellationToken);
            }
        }
    }
}
