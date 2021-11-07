using Application.Products.Commands.BulkUpdateCategory;
using Application.Products.Queries.GetProducts;

namespace Application.Common.Interfaces
{
    public interface IProductRepository
    {
        Task<BulkUpdateCategoryResult> BulkUpdateCategoryDataAsync(ProductCategory category, BulkUpdateCategoryContinuation continuation, CancellationToken cancellationToken = default);
        Task<PagedResultSet<Product>> FindAllAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
        Task<Product> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Product> UpsertAsync(Product product, CancellationToken cancellationToken = default);
        Task<Product> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<IDictionary<string, IEnumerable<Product>>> GetFeaturedProductsPerCategoryAsync(IEnumerable<string> categoryIds, CancellationToken cancellationToken = default);
    }
}
