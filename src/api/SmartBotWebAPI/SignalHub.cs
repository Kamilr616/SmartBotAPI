using Microsoft.AspNetCore.SignalR;


namespace SmartBotWebAPI
{
    public class SignalHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Received message from {user}: {message}");
            await Clients.All.SendAsync("ReceiveMessage", "API", $"Received message:{message}, from:{user}.");
        }

        public async Task ReceiveMessage(string user, string message)
        {
            Console.WriteLine($"Message from {user}: {message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }

        public async Task ReceiveRawMatrix(string user, string message, ushort avgDistance)
        {
            await Clients.Others.SendAsync("ReceiveMatrix", $"API SignalHub: {user}", message, avgDistance);
        }
    }
}