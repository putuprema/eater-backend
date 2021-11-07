namespace Application.Accounts.Queries.GetIdentity
{
    public class GetIdentityQueryHandler : IRequestHandler<GetIdentityQuery, AccountDto>
    {
        private readonly IAccountRepository _accountRepository;

        public GetIdentityQueryHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<AccountDto> Handle(GetIdentityQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAndRoleAsync(request.UserId, request.Role, cancellationToken);
            return account.Adapt<AccountDto>();
        }
    }
}
