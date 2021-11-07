namespace Application.Accounts.Commands.Auth
{
    public enum GrantType { password, refresh_token }

    public class AuthCommand : IRequest<AuthResultDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string RefreshToken { get; set; }
        public string GrantType { get; set; }
    }

    public class AuthCommandValidator : AbstractValidator<AuthCommand>
    {
        public AuthCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required").When(x => x.GrantType == GrantType.password.ToString())
                .EmailAddress().WithMessage("Email is invalid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .When(x => x.GrantType == GrantType.password.ToString());

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Token is required")
                .When(x => x.GrantType == GrantType.refresh_token.ToString());

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role must be specified")
                .IsEnumName(typeof(Role)).WithMessage("Role invalid");

            RuleFor(x => x.GrantType)
                .NotEmpty().WithMessage("Grant type must be specified")
                .IsEnumName(typeof(GrantType)).WithMessage("Grant type invalid");
        }
    }
}
