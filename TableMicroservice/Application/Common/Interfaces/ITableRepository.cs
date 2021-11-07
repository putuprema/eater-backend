namespace Application.Common.Interfaces
{
    public interface ITableRepository
    {
        Task GenerateTablesAsync(int quantity, CancellationToken cancellationToken = default);
        Task<IEnumerable<Table>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Table> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Table> SetActiveAsync(string tableId, bool active, CancellationToken cancellationToken = default);
        Task<Table> UpsertAsync(Table table, CancellationToken cancellationToken = default);
    }
}
