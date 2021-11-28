namespace Eater.Shared.Common
{
    public record DefaultUserClaims
    (
        string Id,
        string Name,
        string Email,
        string Role
    );
}
