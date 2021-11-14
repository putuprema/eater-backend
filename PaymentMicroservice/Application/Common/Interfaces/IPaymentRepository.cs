namespace Application.Common.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
        Task<Payment> UpsertAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
