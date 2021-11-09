namespace Application.Common.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProductCategory> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ProductCategory> UpsertAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task<int> PopulateCategoryCacheAsync(CancellationToken cancellationToken = default);
    }
}
