using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace SmartBotBlazorApp
{
    public class ImageProcessor
    {
        internal string GenerateHeatmapBase64Image(ushort[] depthData)
        {
            int width = 8, scaledWidth = 64;

            // Tworzenie obrazu
            using var image = new Image<Rgba32>(width, width);

            // Normalizacja głębokości
            ushort minDepth = 10, maxDepth = 3500;
            double scale = 1.0 / (maxDepth - minDepth);

            for (int x1 = 0; x1 < width; x1++)
            {
                for (int y1 = 0; y1 < width; y1++)
                {
                    int index = x1 * width + y1;
                    double normalizedValue = (depthData[index] - minDepth) * scale;
                    normalizedValue = Math.Clamp(normalizedValue, 0, 1);

                    // Ustawienie koloru pikseli
                    var heatmapColor = GetHeatmapColor(normalizedValue);
                    image[x1, y1] = heatmapColor;
                }
            }

            // Skalowanie
            image.Mutate(ctx => ctx.Resize(scaledWidth, scaledWidth));

            // Zapisanie jako PNG i konwersja do Base64
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private static Rgba32 GetHeatmapColor(double value)
        {
            if (value < 0.25)
            {
                // Red → Yellow
                double t = value / 0.25;
                return new Rgba32(255, (byte)(t * 255), 0);
            }
            else if (value < 0.5)
            {
                // Yellow → Green
                double t = (value - 0.25) / 0.25;
                return new Rgba32((byte)(255 - t * 255), 255, 0); 
            }
            else if (value < 0.75)
            {
                // Green → Blue
                double t = (value - 0.5) / 0.25;
                return new Rgba32(0, (byte)(255 - t * 255), (byte)(t * 255));
            }
            else
            {
                // Blue → Purple
                double t = (value - 0.75) / 0.25; 
                return new Rgba32((byte)(t * 255), 0, 255);
            }
        }

        internal ushort[] InterpolateData(ushort[] data, int targetSize = 32)
        {
            if (data.Length != 64)
            {
                throw new ArgumentException("Data length must be 64 for an 8x8 image.");
            }

            ushort[] interpolated = new ushort[targetSize * targetSize];
            int inputSize = 8;
            double scale = (double)(inputSize - 1) / targetSize;

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
                }
            }

            return interpolated;
        }
    }
}
