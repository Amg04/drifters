using BLLProject.Repositories;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace PL.Hubs
{
    public class CameraHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;

        public CameraHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
              
                string groupName = null;
                if (user.ManagerId == null)
                    groupName = $"Group_{user.Id}"; 
                else if (user.ManagerId != null)
                    groupName = $"Group_{user.ManagerId}"; 

                if (groupName != null)
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            await base.OnConnectedAsync();

        }
    }
}

