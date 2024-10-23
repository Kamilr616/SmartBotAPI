using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartBotWebAPI;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace SmartBotWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly IHubContext<SignalHub> _hubContext;

        // Injecting into the controller
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
                    using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                    {
                        await HandleWebSocketCommunication(webSocket);
                    }
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
            ushort[] result = new ushort[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                result[i / 2] = BitConverter.ToUInt16(new byte[] { data[i], data[i + 1] }, 0);
            }
            return result;
        }

        private ushort[] InterpolateData(ushort[] data)
        {
            if (data.Length != 64)
            {
                throw new ArgumentException("Data length must be 64 for an 8x8 image.");
            }

            int targetSize = 16;
            ushort[] interpolated = new ushort[targetSize * targetSize]; // 1D array

            // Rozmiar wejściowej tablicy
            int inputSize = 8;

            // Iteracja po każdym pikselu w tablicy
            for (int i = 0; i < targetSize; i++)
            {
                for (int j = 0; j < targetSize; j++)
                {
                    // Obliczanie skali wejściowej do wyjściowej
                    double x = (double)i / targetSize * (inputSize - 1);
                    double y = (double)j / targetSize * (inputSize - 1);

                    int x0 = (int)x;
                    int y0 = (int)y;
                    int x1 = Math.Min(x0 + 1, inputSize - 1);
                    int y1 = Math.Min(y0 + 1, inputSize - 1);

                    // Wyznaczanie wartości pikseli do interpolacji
                    double q00 = data[y0 * inputSize + x0]; // lewy górny
                    double q01 = data[y0 * inputSize + x1]; // prawy górny
                    double q10 = data[y1 * inputSize + x0]; // lewy dolny
                    double q11 = data[y1 * inputSize + x1]; // prawy dolny

                    // Interpolacja
                    double value = (q00 * (x1 - x) * (y1 - y) +
                                    q01 * (x - x0) * (y1 - y) +
                                    q10 * (x1 - x) * (y - y0) +
                                    q11 * (x - x0) * (y - y0));

                    // Ensure value is clamped to valid range (0 to max depth value, e.g., 65535 for ushort)
                    interpolated[i * targetSize + j] = (ushort)Math.Round(value);
                }
            }
            return interpolated;
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

                            //var interpolatedFrame = InterpolateData(dataFrame);
                            //var avgDistance = dataFrame.Select(x => (double)x).Average();
                            
                            var interpolationTask = Task.Run(() => InterpolateData(dataFrame));
                            var averageTask = Task.Run(() =>
                            {
                                return (ushort)dataFrame.Select(x => (int)x).Average(x => x);
                            });

                            var jsonMatrix = JsonSerializer.Serialize(interpolationTask.Result);
                             
                            //_logger.LogInformation($"Average distance: {averageTask.Result} mm");

                            await _hubContext.Clients.All.SendAsync("ReceiveMatrix", "API WS", jsonMatrix, averageTask.Result);
                        }
                        else
                        {
                            _logger.LogWarning("Received binary data does not align with expected 16-bit values.");
                        }

                        // Respond with acknowledgment message
                       // var serverResponse = Encoding.UTF8.GetBytes("Binary frame decoded on server");
                       // await webSocket.SendAsync(new ArraySegment<byte>(serverResponse), WebSocketMessageType.Text, true, CancellationToken.None);

                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Text message received: {message}");

                        // Respond with acknowledgment message
                        var responseText = $"WS recieved text message:{message}";
                        // var serverResponse = Encoding.UTF8.GetBytes(responseText);
                        // await webSocket.SendAsync(new ArraySegment<byte>(serverResponse), WebSocketMessageType.Text, true, CancellationToken.None);

                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "API", responseText);
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
