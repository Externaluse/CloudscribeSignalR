using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cloudscribe.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;


namespace CloudscribeSignalR.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<SignalRHub> _signalRHub;
        /// <summary>
        /// inject the hubcontext into the home controller
        /// </summary>
        /// <param name="signalRHub"></param>
        public HomeController(IHubContext<SignalRHub> signalRHub)
        {
            _signalRHub = signalRHub;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpGet]
        [Route("/api/ApiExampleMethod")]
        public async Task<IActionResult> ApiExampleMethod(string notify = "all")
        {
            switch (notify)
            {                                    
                case "onlyme":
                    await _signalRHub.Clients.User(User.GetUserIdAsGuid().ToString()).SendAsync("Notify",
                        JsonConvert.SerializeObject(new { title = "Here is a notification to the caller only", text = "Here goes text" }));
                    return new OkObjectResult(new { Id = 123, Name = "Yeah, notification!" });
                case "butme":
                    // TODO: Doesn't work yet, because AllExcept and GroupsExcept have changed again; issue filed with SignalR
                    await _signalRHub.Clients.AllExcept(new List<string>() { User.GetUserIdAsGuid().ToString() }).SendAsync("Notify",
                        JsonConvert.SerializeObject(new { title = "Here is a notification to everone but the caller", text = "Here goes text" }),
                        5);
                    return new OkObjectResult(new { Id = 123, Name = "Yeah, notification!" });
                case "desktop":
                    await _signalRHub.Clients.All.SendAsync("NotifyDesktop",
                        JsonConvert.SerializeObject(new { title = "Here is a Desktop permission, provided you've granted them.", text = "Otherwise they appear the same as the normal ones. Try to click me. Even without, I'll transform in a bit." }),
                        1);
                    return new OkObjectResult(new { Id = 123, Name = "Yeah, notification!" });
                default:
                    await _signalRHub.Clients.All.SendAsync("Notify",
                        JsonConvert.SerializeObject(new { title = "Here is a notification from ApiExampleMethod to everyone", text = "Here goes text" }),
                        1);
                    return new OkObjectResult(new { Id = 123, Name = "Yeah, notification!" });

            }
            
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        
    }
}
