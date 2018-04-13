using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CloudscribeSignalR.Controllers
{
    public class SignalRHeartbeat : Hub
    {
        // this accepts a "tock" message from the client and does absolutely nothing. However the client will notice if the connection is not active, and re-connect if necessary
        public async Task HeartBeatTock() => await Task.CompletedTask;
        // this sends a "tick" message, configured in the timer in startup cs; the datetime is not strictly required, it could even be empty data
        public async Task HeartBeat(DateTime now) => await Clients.All.SendAsync("Heartbeat", now);
    }
}