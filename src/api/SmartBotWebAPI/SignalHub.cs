using Microsoft.AspNetCore.SignalR;


namespace SmartBotWebAPI
{
    public class SignalHub : Hub
    {
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


        public async Task ReceiveRawMatrix(string user, ushort[] message, ushort avgDistance)
        {
            await Clients.Others.SendAsync("ReceiveMatrix", $"API SignalHub -> {user}", InterpolateData(message), avgDistance);
        }


        private static ushort[] InterpolateData(ushort[] data)
        {
            if (data.Length != 64)
            {
                throw new ArgumentException("Data length must be 64 for an 8x8 image.");
            }

            int targetSize = 16;
            ushort[] interpolated = new ushort[targetSize * targetSize];

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
    }


}