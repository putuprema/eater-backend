using Application.Payments.Commands.InitPayment;
using Application.Payments.Commands.PaymentNotification;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Integrations.Stripe
{
    public class StripeIntegrationService : IPaymentIntegrationService
    {
        private readonly SessionService _stripeSession;
        private readonly string _webhookSecret;

        public StripeIntegrationService(IConfiguration configuration)
        {
            StripeConfiguration.MaxNetworkRetries = 3;
            StripeConfiguration.ApiKey = configuration["Stripe:ApiKey"];

            _stripeSession = new SessionService();
            _webhookSecret = configuration["Stripe:WebhookSecret"];
        }

        public PaymentNotificationEvent HandlePaymentNotification(string payloadStr, string signature = default)
        {
            var validEventTypes = new[] { Events.CheckoutSessionCompleted, Events.CheckoutSessionExpired };

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(payloadStr, signature, _webhookSecret);

                if (!validEventTypes.Any(evt => evt == stripeEvent.Type))
                    return null;

                var checkoutSession = stripeEvent.Data.Object as Session;

                PaymentStatus status = stripeEvent.Type switch
                {
                    Events.CheckoutSessionCompleted => PaymentStatus.PAID,
                    Events.CheckoutSessionExpired => PaymentStatus.EXPIRED,
                    _ => PaymentStatus.UNPAID
                };

                return new PaymentNotificationEvent
                {
                    OrderId = checkoutSession.ClientReferenceId,
                    Status = status
                };
            }
            catch (StripeException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<InitPaymentResult> InitPaymentAsync(InitPaymentCommand request, CancellationToken cancellationToken = default)
        {
            var sessionCreateOpts = new SessionCreateOptions
            {
                CustomerEmail = request.CustomerEmail,
                ClientReferenceId = request.OrderId,
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = "https://google.com",
                CancelUrl = "https://google.com",
                LineItems = request.Items.Select(i => new SessionLineItemOptions
                {
                    Quantity = i.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = i.Name,
                            Images = new() { i.Image },
                        },
                        UnitAmount = i.UnitPrice * 100,
                        Currency = "idr"
                    }
                }).ToList()
            };

            var session = await _stripeSession.CreateAsync(sessionCreateOpts, cancellationToken: cancellationToken);
            return new InitPaymentResult
            {
                Token = session.Id,
                RedirectUrl = session.Url
            };
        }
    }
}
