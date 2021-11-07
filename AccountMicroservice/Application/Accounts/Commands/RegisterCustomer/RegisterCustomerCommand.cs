namespace Application.Accounts.Commands.RegisterCustomer
{
    public class RegisterCustomerCommand : IRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
    {
        private readonly IAccountRepository _accountRepository;

        public RegisterCustomerCommandValidator(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;

            RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required");

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Email address is invalid")
                .MustAsync(BeUnique).WithMessage("Email address is already used");

            RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required");
        }

        public async Task<bool> BeUnique(string emailAddress, CancellationToken cancellationToken)
        {
            return (await _accountRepository.GetByEmailAndRoleAsync(emailAddress, cancellationToken: cancellationToken)) == null;
        }
    }
}
