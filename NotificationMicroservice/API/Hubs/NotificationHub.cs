using Domain.Constants;
using Microsoft.Azure.SignalR.Management;
using System.Security.Claims;

namespace API.Hubs
{
    public class NotificationHub : ServerlessHub
    {
        [FunctionName("NegotiateSignalR")]
        public async Task<IActionResult> Negotiate([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/notifications/signalr/negotiate")] HttpRequest req)
        {
            try
            {
                var userClaims = req.GetDefaultUserClaims();

                var connectionInfo = await NegotiateAsync(new NegotiationOptions
                {
                    UserId = userClaims.Id,
                    Claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userClaims.Name),
                        new Claim(ClaimTypes.Email, userClaims.Email),
                        new Claim(ClaimTypes.Role, userClaims.Role)
                    }
                });

                return new OkObjectResult(connectionInfo);
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }

        [FunctionName(nameof(Subscribe))]
        public async Task Subscribe([SignalRTrigger] InvocationContext invocationContext, string groupId, CancellationToken cancellationToken)
        {
            await Groups.AddToGroupAsync(invocationContext.ConnectionId, groupId, cancellationToken);
        }

        [FunctionName(nameof(Unsubscribe))]
        public async Task Unsubscribe([SignalRTrigger] InvocationContext invocationContext, string groupId, CancellationToken cancellationToken)
        {
            await Groups.RemoveFromGroupAsync(invocationContext.ConnectionId, groupId, cancellationToken);
        }

        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected([SignalRTrigger] InvocationContext invocationContext, CancellationToken cancellationToken)
        {
            var userRole = invocationContext.Claims[ClaimTypes.Role];
            if (userRole == UserRole.EmployeeKitchen)
            {
                await Groups.AddToGroupAsync(invocationContext.ConnectionId, NotificationGroups.KitchenNotificationGroup, cancellationToken);
            }
        }
    }
}
