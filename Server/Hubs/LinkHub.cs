using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public static class LinkHubMethods
    {
        public static readonly string Request = "REQUEST";
    }

    public static class LinkHubExtensions
    {
        public static Task SendRequest(this IHubContext<LinkHub> hub, string file)
        {
            return hub.Clients.All.SendAsync(LinkHubMethods.Request, file);
        }
    }

    public class LinkHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client {Clients.Caller} connected");
            return Task.CompletedTask;
        }
    }
}
