using Microsoft.AspNetCore.SignalR;


namespace SmartBotBlazorApp.Hubs
{
    public class SignalHub : Hub
    {
        private ImageProcessor _imageProcessor;

        public SignalHub(ImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
        }

        [HubMethodName("SendMessage")]
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Received message from {user}: {message}");
            await Clients.All.SendAsync("ReceiveMessage", $"API SignalHub -> {user}", $"Received message:{message}");
        }

        [HubMethodName("ReceiveMessage")]
        public async Task ReceiveMessage(string user, string message)
        {
            Console.WriteLine($"Message from {user}: {message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }

        [HubMethodName("ReceiveRobotData")]
        public async Task ReceiveRobotData(string user, double[] measurements, ushort[] rawMatrix, ushort avgDistance)
        {
            var base64Img = _imageProcessor.GenerateHeatmapBase64Image(rawMatrix);

            RoundMeasurements(measurements, 2);

            await Clients.Others.SendAsync("ReceiveBase64Frame", $"SignalHub({user})", measurements, base64Img, avgDistance);

            await Clients.Others.SendAsync("ReceiveMatrix", $"SignalHub({user})", measurements, _imageProcessor.InterpolateData(rawMatrix, 32), avgDistance);
        }
        private void RoundMeasurements(double[] measurementsArray, int precision = 2)
        {
            for (int i = 0; i < measurementsArray.Length; i++)
            {
                measurementsArray[i] = Math.Round(measurementsArray[i], precision, MidpointRounding.ToEven);
            }
        }

    }

}