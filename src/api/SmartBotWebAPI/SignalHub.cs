using Microsoft.AspNetCore.SignalR;

namespace SmartBotWebAPI
{
    public class SignalHub : Hub
    {
        private readonly ImageProcessor _imageProcessor = new ImageProcessor();

        public SignalHub(ImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor;
        }

        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"Received message from {user}: {message}");
            await Clients.All.SendAsync("ReceiveMessage", $"API SignalHub -> {user}", $"Received message:{message}");
        }

        public async Task ReceiveMessage(string user, string message)
        {
            Console.WriteLine($"Message from {user}: {message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }

        public async Task ReceiveRobotData2(string user, float[] measurements, ushort[] rawMatrix, ushort avgDistance)
        {
            var base64Img = _imageProcessor.GenerateHeatmapBase64Image(rawMatrix);
            await Clients.Others.SendAsync("ReceiveBase64Frame", $"SignalHub({user})", measurements, base64Img, avgDistance);

            await Clients.Others.SendAsync("ReceiveMatrix", $"SignalHub({user})", measurements, _imageProcessor.InterpolateData(rawMatrix, 32), avgDistance);
        }

        public async Task ReceiveRobotData(string user, float[] measurements, ushort[] rawMatrix, ushort avgDistance)
        {
            var interpolatedMatrix = _imageProcessor.InterpolateData(rawMatrix, 32);

            await Clients.Others.SendAsync("ReceiveMatrix", $"SignalHub({user})",  measurements, interpolatedMatrix, avgDistance);
        }

        //public async Task ReceiveRawMatrix(string user, ushort[] message)
        //{
        //    var (interpolatedData, avgDistance) = _imageProcessor.InterpolateDataAvgTuple(message, 32);

        //    await Clients.Others.SendAsync("ReceiveMatrix", $"API SignalHub -> {user}", interpolatedData, avgDistance);
        //}


    }

}