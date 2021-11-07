namespace Application.Accounts.Queries.GetIdentity
{
    public class GetIdentityQuery : IRequest<AccountDto>
    {
        public string UserId { get; set; }
        public Role Role { get; set; }
    }

    public class GetIdentityQueryValidator : AbstractValidator<GetIdentityQuery>
    {
        public GetIdentityQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user id is required");
            RuleFor(x => x.Role).IsInEnum().WithMessage("Role is invalid");
        }
    }
}
