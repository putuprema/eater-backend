namespace Application.Accounts.Commands.RegisterCustomer
{
    public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordEncoderService _passwordEncoderService;

        public RegisterCustomerCommandHandler(IAccountRepository accountRepository, IPasswordEncoderService passwordEncoderService)
        {
            _accountRepository = accountRepository;
            _passwordEncoderService = passwordEncoderService;
        }

        public async Task<Unit> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Email = request.Email,
                Password = _passwordEncoderService.Encode(request.Password),
                DisplayName = request.Name,
                Role = Role.CUSTOMER
            };

            await _accountRepository.UpsertAsync(account, cancellationToken);
            return Unit.Value;
        }
    }
}
