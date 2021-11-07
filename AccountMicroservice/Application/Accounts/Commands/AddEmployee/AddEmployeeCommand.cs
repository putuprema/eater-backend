namespace Application.Accounts.Commands.AddEmployee
{
    public class AddEmployeeCommand : IRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class AddEmployeeCommandValidator : AbstractValidator<AddEmployeeCommand>
    {
        private readonly IAccountRepository _accountRepository;

        public AddEmployeeCommandValidator(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;

            RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required");

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Email address is invalid")
                .MustAsync(BeUnique).WithMessage("Email address is already used");

            RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required");

            RuleFor(c => c.Role)
                .NotEmpty().WithMessage("Employee role must be specified")
                .IsEnumName(typeof(EmployeeRole)).WithMessage("Employee role invalid");
        }

        public async Task<bool> BeUnique(string emailAddress, CancellationToken cancellationToken)
        {
            return (await _accountRepository.GetByEmailAndRoleAsync(emailAddress, Role.EMPLOYEE, cancellationToken: cancellationToken)) == null;
        }
    }
}
