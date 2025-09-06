using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PL.Hubs
{
    public class CameraHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            //var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (userId != null)
            //{
            //    // كل مستخدم يدخل يُضاف لمجموعته الخاصة حسب userId
            //    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            //}
            //await base.OnConnectedAsync();
        }
    }
}

