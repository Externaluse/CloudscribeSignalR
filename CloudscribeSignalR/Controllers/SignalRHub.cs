using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using cloudscribe.Core.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CloudscribeSignalR.Controllers
{
    [Authorize]
    public class SignalRHub : Hub
    {
        /// <summary>
        /// Send the User's Guid to the client, so he can later accept or ignore messages sent to all - e.g. if the user initiated a task he may not need a notification that something has changed
        /// This will be cached on the client.
        /// </summary>
        /// <returns></returns>
        private async Task Identification() => await Clients.Group(Context.User.GetUserIdAsGuid().ToString()).SendAsync("Identification", Context.User.GetUserIdAsGuid().ToString());
        
        /// <summary>
        /// We're overriding OnConnected to: save the user as a "Group" with a lone member; that way we can target messages later through a hubContext in a controller etc
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // add the user ID as a group name, then we can invoke hubcontext.Group() later from a controller
            await Groups.AddAsync(Context.ConnectionId, Context.User.GetUserIdAsGuid().ToString() ?? new Guid().ToString());
            await base.OnConnectedAsync();
            await Identification();
        }
    }
}
