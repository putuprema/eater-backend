namespace Application.Common.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account> UpsertAsync(Account account, CancellationToken cancellationToken = default);
        Task<Account> GetByEmailAndRoleAsync(string email, Role role = Role.CUSTOMER, CancellationToken cancellationToken = default);
        Task<Account> GetByIdAndRoleAsync(string id, Role role = Role.CUSTOMER, CancellationToken cancellationToken = default);
    }
}
