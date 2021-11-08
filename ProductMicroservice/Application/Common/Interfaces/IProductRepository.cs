using Application.Products.Commands.BulkUpdateCategory;
using Application.Products.Queries.GetProducts;

namespace Application.Common.Interfaces
{
    public interface IProductRepository
    {
        Task<BulkUpdateCategoryResult> BulkUpdateCategoryDataAsync(ProductCategory category, BulkUpdateCategoryContinuation continuation, CancellationToken cancellationToken = default);
        Task<PagedResultSet<Product>> FindAllAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
        Task<PagedResultSet<Product>> GetByCategoryIdAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
        Task<Product> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Product> UpsertAsync(Product product, CancellationToken cancellationToken = default);
        Task<Product> UpsertProductByCategoryAsync(Product product, CancellationToken cancellationToken = default);
        Task<Product> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<Product> DeleteProductByCategoryAsync(string id, string categoryId, CancellationToken cancellationToken = default);
    }
}
