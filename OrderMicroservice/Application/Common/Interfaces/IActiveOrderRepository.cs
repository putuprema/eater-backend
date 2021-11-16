namespace Application.Common.Interfaces
{
    public interface IActiveOrderRepository
    {
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActiveOrder>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ActiveOrder> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ActiveOrder> UpsertAsync(ActiveOrder session, CancellationToken cancellationToken = default);
    }
}
