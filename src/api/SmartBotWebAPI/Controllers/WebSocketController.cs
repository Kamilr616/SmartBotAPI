using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartBotWebAPI;
using System.Net.WebSockets;
using System.Text;


namespace SmartBotWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly IHubContext<SignalHub> _hubContext;

        public WebSocketController(ILogger<WebSocketController> logger, IHubContext<SignalHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet("ws")]
        public async Task GetWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    await HandleWebSocketCommunication(webSocket);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while handling WebSocket.");
                    HttpContext.Response.StatusCode = 500; // Internal Server Error
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // Bad Request if not a WebSocket request
            }
        }

        private ushort[] ConvertData(byte[] data, int length)
        {
            var result = new ushort[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                result[i / 2] = BitConverter.ToUInt16(new byte[] { data[i], data[i + 1] }, 0);
            }
            return result;
        }

        private static (ushort[] interpolated, ushort avgValue) InterpolateDataAvgTuple(ushort[] data, int targetSize = 32)
        {
            if (data.Length != 64)
            {
                throw new ArgumentException("Data length must be 64 for an 8x8 image.");
            }

            ushort[] interpolated = new ushort[targetSize * targetSize];
            int inputSize = 8;
            double scale = (double)(inputSize - 1) / targetSize;
            uint totalValue = 0;

            for (int i = 0; i < targetSize; i++)
            {
                double x = i * scale;
                int x0 = (int)x;
                int x1 = Math.Min(x0 + 1, inputSize - 1);
                double xDiff = x - x0;

                for (int j = 0; j < targetSize; j++)
                {
                    double y = j * scale;
                    int y0 = (int)y;
                    int y1 = Math.Min(y0 + 1, inputSize - 1);
                    double yDiff = y - y0;

                    double q00 = data[y0 * inputSize + x0];
                    double q01 = data[y0 * inputSize + x1];
                    double q10 = data[y1 * inputSize + x0];
                    double q11 = data[y1 * inputSize + x1];

                    double interpolatedValue = (q00 * (1 - xDiff) * (1 - yDiff) +
                                                q01 * xDiff * (1 - yDiff) +
                                                q10 * (1 - xDiff) * yDiff +
                                                q11 * xDiff * yDiff);

                    ushort interpolatedUShort = (ushort)Math.Clamp(Math.Round(interpolatedValue), 0, ushort.MaxValue);
                    interpolated[i * targetSize + j] = interpolatedUShort;

                    totalValue += interpolatedUShort;
                }
            }

            ushort avgValue = (ushort)(totalValue / (targetSize * targetSize));
            return (interpolated, avgValue);
        }

        private async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            var buffer = new byte[128];

            try
            {
                while (true)
                {
                    // Receive message from client
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Close the WebSocket connection gracefully
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        _logger.LogInformation("WebSocket connection closed gracefully.");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Decode binary data (example: 8x8 distance data frame where each value is 2 bytes)
                        if (result.Count % 2 == 0) // Ensure the data size is a multiple of 2
                        {
                            var dataFrame = ConvertData(buffer, result.Count);

                            var (interpolatedData, avgDistance) = InterpolateDataAvgTuple(data: dataFrame);

                            await _hubContext.Clients.All.SendAsync("ReceiveMatrix", "API Websocket", new double[] { 1, -2, 0, -1.5, 5, -0.5, 22.5},  interpolatedData, avgDistance);

                            //_logger.LogInformation($"Average distance: {avgDistance} mm");
                        }
                        else
                        {
                            _logger.LogWarning("Received binary data does not align with expected 16-bit values.");
                        }

                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Text message received: {message}");

                        // Respond with acknowledgment message
                        var responseText = $"API WS recieved text message:{message}";
                        // var serverResponse = Encoding.UTF8.GetBytes(responseText);
                        // await webSocket.SendAsync(new ArraySegment<byte>(serverResponse), WebSocketMessageType.Text, true, CancellationToken.None);

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "API Websocket", responseText);
                    }
                }
            }
            catch (WebSocketException wsEx)
            {
                _logger.LogError(wsEx, "WebSocket error occurred.");
                await CloseWebSocketAsync(webSocket, WebSocketCloseStatus.InternalServerError, "WebSocket error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                await CloseWebSocketAsync(webSocket, WebSocketCloseStatus.InternalServerError, "Internal error occurred");
            }
        }

        private async Task CloseWebSocketAsync(WebSocket webSocket, WebSocketCloseStatus closeStatus, string statusDescription)
        {
            if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
                _logger.LogInformation("WebSocket connection closed due to an error.");
            }
        }
    }
}
