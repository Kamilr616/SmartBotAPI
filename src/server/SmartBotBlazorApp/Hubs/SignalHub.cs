using Microsoft.AspNetCore.SignalR;
using SmartBotBlazorApp.Data;


namespace SmartBotBlazorApp.Hubs
{
    public class SignalHub : Hub
    {
        private readonly ImageProcessor _imageProcessor;
        private readonly MeasurementService _measurementService;

        public SignalHub(MeasurementService measurementService, ImageProcessor imageProcessor)
        {
            _measurementService = measurementService;
            _imageProcessor = imageProcessor;
        }


        [HubMethodName("SendMessage")]
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, $"Message:{message}");
        }

        [HubMethodName("ReceiveMessage")]
        public async Task ReceiveMessage(string user, string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }

        //DEPRECIATED
        //[HubMethodName("SendMovementCommand")]
        //public async Task SendMovementCommand(string user, string command)
        //{
        //   double motorA = 0; 
        //   double motorB = 0;

        //    switch (command)
        //   {
        //        case "UP":
        //            motorA = 255;
        //            motorB = 255;
        //            break;
        //        case "DOWN":
        //            motorA = -255;
        //            motorB = -255;
        //            break;
        //        case "LEFT":
        //            motorA = -255;
        //            motorB = 255;
        //            break;
        //        case "RIGHT":
        //            motorA = 255;
        //            motorB = -255;
        //            break;
        //        case "STOP":
        //            motorA = 0;
        //            motorB = 0;
        //            break;
        //        default:
        //            motorA = 0;
        //            motorB = 0;
        //            break;
        //        }

        //await Clients.Others.SendAsync("ReceiveRobotCommand", motorA, motorB);

        //}

        [HubMethodName("SendMovementCommand")]
        public async Task SendMovementCommand(string user, int motorA,int motorB)
        {

            await Clients.Others.SendAsync("ReceiveRobotCommand", motorA, motorB);

        }


        [HubMethodName("ReceiveRobotData")]
        public async Task ReceiveRobotData(string user, double[] measurements, ushort[] rawMatrix, ushort avgDistance)
        {
            var roundedMeasurements = _measurementService.RoundMeasurements(measurements, 2);

            await Task.WhenAll(
                _measurementService.SaveMeasurementsToDatabase(user, measurements, avgDistance),
                Clients.Others.SendAsync("ReceiveBase64Frame", user, roundedMeasurements, _imageProcessor.GenerateHeatmapBase64Image(rawMatrix), avgDistance),
                Clients.Others.SendAsync("ReceiveMatrix", user, roundedMeasurements, _imageProcessor.InterpolateData(rawMatrix, 32), avgDistance)
            );
        }
       
    }
}