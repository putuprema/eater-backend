namespace Application.Accounts.Commands.AddEmployee
{
    public class AddEmployeeCommandHandler : IRequestHandler<AddEmployeeCommand>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordEncoderService _passwordEncoderService;

        public AddEmployeeCommandHandler(IAccountRepository accountRepository, IPasswordEncoderService passwordEncoderService)
        {
            _accountRepository = accountRepository;
            _passwordEncoderService = passwordEncoderService;
        }

        public async Task<Unit> Handle(AddEmployeeCommand request, CancellationToken cancellationToken)
        {
            var account = new Account
            {
                Email = request.Email,
                Password = _passwordEncoderService.Encode(request.Password),
                DisplayName = request.Name,
                Role = Role.EMPLOYEE,
                EmployeeRole = Enum.Parse<EmployeeRole>(request.Role)
            };

            await _accountRepository.UpsertAsync(account, cancellationToken);
            return Unit.Value;
        }
    }
}
