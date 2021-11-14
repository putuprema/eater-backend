using System.Net;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly CosmosService _cosmosService;

        public PaymentRepository(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task<Payment> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Payments.ReadItemAsync<Payment>(orderId, new PartitionKey(orderId), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    return null;

                throw ex;
            }
        }

        public async Task<Payment> UpsertAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            payment.UpdatedOn = DateTime.UtcNow;
            return await _cosmosService.Payments.UpsertItemAsync(payment, new PartitionKey(payment.OrderId), cancellationToken: cancellationToken);
        }
    }
}
