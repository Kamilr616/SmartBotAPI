using Microsoft.AspNetCore.SignalR;
using System.Text.Json;


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

        public async Task ReceiveRawMatrix(string user, ushort[] message)
        {
            var (interpolatedData, avgDistance) = InterpolateDataAvgTuple(message, 32);

            await Clients.Others.SendAsync("ReceiveMatrix", $"API SignalHub -> {user}", interpolatedData, avgDistance);
        }


        private static ushort[] InterpolateData(ushort[] data, int targetSize = 16)
        {
            if (data.Length != 64)
            {
                throw new ArgumentException("Data length must be 64 for an 8x8 image.");
            }

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

    }

}