# Cloudscribe & SignalR
Playground for the amazing .NET Core Cloudscribe project template; today: integrating SignalR for notifcations using pnotify
## Resources
- Cloudscribe (the .NET Core Identity/Multi-Tenancy/Simple Content/Blog/and so much more project template) 
  - https://github.com/cloudscribe
  - https://gitter.im/joeaudette/cloudscribe
  - https://www.cloudscribe.com/
- SignalR 
  - https://github.com/aspnet/SignalR
  - 
## Purpose
I found it relatively hard to get started with the all-new .Net Core vanilla-js SignalR library (beginner in C# and generally amateur programmer), but have to say the guys over at https://github.com/aspnet/SignalR have done such an amazing job that it was worth the effort.

This repository is more a test and playground for myself, but it may help others to get started quicker. I'll make a few notes about how I fared on particular tasks so you can:
## Follow along
Whilst trying to write a markdown README for Github, I looked for a Visual Studio-integrated editor and found [a great one from Mads Kristensen](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor#overview), that even supports the Github flavoured MD.

1) The easiest way to get started with Cloudscribe is the [Visual Studio project template available from the VS Marketplace](https://marketplace.visualstudio.com/items?itemName=joeaudette.cloudscribeProjectTemplate).

   This install uses a simple Single Tenant, NoDb configuration, and the Simple Content module is not included.

2) Update the NuGet packages, and build

3) I suppose you've got npm installed; I have now added a package.json to get @aspnet-signalr (essentially the client), gulp to copy those from ./node-modules to wwwroot/lib, and pnotify, a fantastic notification library, and grunt for copying the stuff over to wwwroot

4) Your Task Runner explorer should now list the various Copy* tasks and execute them after build. There's probably a better way to do that, your mileage may vary.

5) I'm now adding two SignalR Hub Endpoints, for simplicity I'll add them to the Controller folder but they shouldn't necessarily be there. Both derive from SignalR.Hub
- SignalRHeartbeat contains method "Heartbeat", which sends the current datetime to the client, and HeartBeatTock which accepts a message from the client to allow it to check if it is still connected. It does not require authentication, and is used for connection handling, reconnecting etc. We'll wire this up later.
- SignalRHub; which will be our main hub later, and which is decorated with `[Authorize]`. It overrides Hub.OnConnected in order to create a group with the user's Guid as group name, so we can resolve this in an injected HubContext from a Controller later; the user is the lone member of that group. It also sends the Guid to the client for local caching, which allows the client to listen or to ignore particular messages - no point in sending a notification to the person who's actioned something; he's just done it and will know, right?

6) Now SignalR needs to be added to the pipeline. In startup.cs, add to `public void ConfigureServices(IServiceCollection services)`: `services.AddSignalR();`

7) We can now configure our endpoints in startup.cs' `Configure`. The order of the middleware is important (thank you @joeaudette to help me figure this out!), so in order for ´[Authorize]´ to work make sure to add the Hubs **after** `app.UseCloudscribeCore()`:
```csharp
    app.UseSignalR(routes =>
    {
        routes.MapHub<SignalRHeartbeat>("/heartbeat");
        routes.MapHub<SignalRHub>("/signalr");
    });
```
Make sure that the URLs do not conflict with any existing controllers or routes that come after this.

8) Build and run - if you are me, it will not work: *Application startup exception: System.InvalidOperationException: Unable to find the SignalR service. Please add it by calling 'IServiceCollection.AddSignalR()'.*
Just to make you a part of my life: I have typed the code in 6) above, but only in this Readme. This is a good time to test if my stuff is copy&paste capable. And to go get another cup of coffee.

9) With sufficient levels of caffeine, you can now test the basic implementation. Navigate to yourUrl/heartbeat - you should get an empty page stating "Connection ID required". Navigate to /signalr and you should be redirect to the login page. So far, so good. Let's wire the client before whilst the kettle boils my next cup.

10) Now we'll need the javascript client file - and we can add pnotify in the same step since we'll need it later anyway. Add pnotify.css and .js; and signalr.js to the bundleconfig.json and for dev in _Layout.cshtml. A quick check in the browser's console reveals that PNotify and signalR are now defined.

### Heartbeat Hub

11) The "heartbeat" hub is trying to showcase periodic information sent to the client; be it a particular update, weather report or, god forbid, a new advert - or whatnot. In any case, it should illustrate my thinking for reconnection of a broken hub (server crashed, browser's host computer went to sleep and woke back up, etc...)
- in my example, I have wired this into the _Layout file *below* RenderScripts as a partial (`@await Html.PartialAsync("_signalRHHeartbeat")`), see Views/Shared/_signalRHeartbeat.cshtml 
- the javascript should be reasonably well documented, but here are the basics:
  - I call `registerSignalRHeartbeat()`. If no connection previously exists, it will start a connection (beware that Websockets don't appear to work behind IISExpress on localhost, therefore I use Server Sent Events as transport - you may want to change that to websockets for production) 
  - Once a connection is established, I register a `heartBeatTockTimer` (using setInterval). This will periodically send the HeartbeatTock to the server - whenever a call fails, it will call `registerSignalRHeartbeat()`, which if a connection exists will try to close it properly or, failing that, dispose of it, and try to re-establish the connection for the configured number of retries. After which it ungracefully throws an exception.
  - There are probably way better methods to do that, and the most recent version of signalR probably handles some stuff themselves, but I found this approach to be reliable enough to survive a number of cases, including putting my computer to sleep and continuing where I left of the next morning
  - Check the browser's console to see some trace messages from that code
### Notification Hub 
12) There is one major difference in the notification hub: It requires Authorization, you'll no doubt spot the `[Authorize]` on the class.
- this allows us to override OnConnectedAsync() and store the user ID as a group. From outside the hub itself, e.g. when injected to a controller through HubContext, Clients.Caller is not available. However, Clients.Group(theUserGuid) is. That will come in handy a little later.
- There are more ways to track connections across servers etc, e.g. by deriving from HubWithPresence, but I haven't played with that yet.
- The `Identification()` method sends the user's Guid back to the browser, so that we can access that later to choose if we want to display or ignore certain messages sent to "All"
- To demonstrate, I have decorated the About() method in the Home Controller with Authorize, and we'll wire the JS directly into the view's scripts section. We could obviously re-use the connection handling stuff, but to keep it simple I'll just wire up a basic hub connection in Views/Home/About.cshtml
- might be a good time to get another coffee and commit what we have so far.
