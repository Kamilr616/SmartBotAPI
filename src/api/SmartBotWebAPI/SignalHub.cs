using Microsoft.AspNetCore.SignalR;


namespace SmartBotWebAPI
{
    public class SignalHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Log the received message (optional)
            Console.WriteLine($"Received message from {user}: {message}");

            // Send the message to connected client
            await Clients.Caller.SendAsync("ReceiveMessage", "API", $"Message:{message} from:{user} received!");

           // await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task ReceiveMessage(string user, string message)
        {
            // Process the incoming message
            Console.WriteLine($"Message from {user}: {message}");

            // Optionally send a response or acknowledgment back to the client
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }
    }
}