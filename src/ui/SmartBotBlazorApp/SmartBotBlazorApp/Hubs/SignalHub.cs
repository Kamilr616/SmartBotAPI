using Microsoft.AspNetCore.SignalR;

namespace SmartBotBlazorApp.Hubs
{

    public class SignalHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Process the incoming message
            Console.WriteLine($"Message from {user}: {message}");

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task ReceiveMessage(string user, string message)
        {
            // Process the incoming message
            Console.WriteLine($"Message from {user}: {message}");

            // Optionally send a response or acknowledgment back to the client
            await Clients.Caller.SendAsync("Acknowledgment", "Message received!");
        }
    }
}