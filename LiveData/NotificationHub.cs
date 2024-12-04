using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EcommApp.LiveData
{
    public class NotificationHub : Hub
    {

        public override Task OnConnectedAsync()
        {
            // Extract user ID from the JWT token claims
            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add this connection to a group named after the user's ID
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Remove this connection from the group
                Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            return base.OnDisconnectedAsync(exception);
        }

    }

}
