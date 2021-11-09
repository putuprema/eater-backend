using Application.Products.Queries.GetFeaturedProducts;
using Application.Products.Queries.GetProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IFeaturedProductsService
    {
        Task<IEnumerable<FeaturedProductsDto>> GetFeaturedProducts(CancellationToken cancellationToken = default);
        Task PopulateFeaturedProductsCache(CancellationToken cancellationToken = default);
    }
}
