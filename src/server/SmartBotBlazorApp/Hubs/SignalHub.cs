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
            EnsureOperatorCaller();
            await Clients.All.SendAsync("ReceiveMessage", user, $"Message:{message}");
        }

        [HubMethodName("ReceiveMessage")]
        public async Task ReceiveMessage(string user, string message)
        {
            EnsureOperatorCaller();
            await Clients.Caller.SendAsync("ReceiveMessage", "API", "Message received!");
        }

        [HubMethodName("SendMovementCommand")]
        public async Task SendMovementCommand(string user, int motorA, int motorB)
        {
            EnsureOperatorCaller();
            RobotMessageValidator.ValidateMovementCommand(motorA, motorB);
            await Clients.Others.SendAsync("ReceiveRobotCommand", motorB, motorA);
        }

        [HubMethodName("ReceiveRobotData")]
        public async Task ReceiveRobotData(string user, double[] measurements, ushort[] rawMatrix, ushort avgDistance)
        {
            EnsureRobotCaller();
            RobotMessageValidator.ValidateTelemetry(user, measurements, rawMatrix);
            var roundedMeasurements = _measurementService.RoundMeasurements(measurements, 2);

            await Task.WhenAll(
                _measurementService.SaveMeasurementsToDatabase(user, measurements, avgDistance),
                Clients.Others.SendAsync("ReceiveBase64Frame", user, roundedMeasurements, _imageProcessor.GenerateHeatmapBase64Image(rawMatrix), avgDistance),
                Clients.Others.SendAsync("ReceiveMatrix", user, roundedMeasurements, _imageProcessor.InterpolateData(rawMatrix, 32), avgDistance)
            );
        }

        private void EnsureOperatorCaller()
        {
            if (!SignalHubAccess.IsOperator(Context.User))
            {
                throw new HubException("This hub method requires an authenticated dashboard operator.");
            }
        }

        private void EnsureRobotCaller()
        {
            if (!SignalHubAccess.IsRobot(Context.User))
            {
                throw new HubException("This hub method requires an authenticated robot connection.");
            }
        }
    }
}
