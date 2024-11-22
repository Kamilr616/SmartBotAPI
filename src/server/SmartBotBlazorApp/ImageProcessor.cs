using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace SmartBotBlazorApp
{
    public class ImageProcessor
    {
        public string GenerateHeatmapBase64Image(ushort[] depthData)
        {
            int width = 8, height = 8, scaledWidth = 32, scaledHeight = 32;

            // Tworzenie obrazu
            using var image = new Image<Rgba32>(width, height);

            // Normalizacja głębokości
            ushort minDepth = 1, maxDepth = 4000;
            double scale = 1.0 / (maxDepth - minDepth);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    double normalizedValue = (depthData[index] - minDepth) * scale;
                    normalizedValue = Math.Clamp(normalizedValue, 0, 1);

                    // Ustawienie koloru pikseli
                    var heatmapColor = GetHeatmapColor(normalizedValue);
                    image[x, y] = heatmapColor;
                }
            }

            // Skalowanie do 32x32
            image.Mutate(ctx => ctx.Resize(scaledWidth, scaledHeight));

            // Zapisanie jako PNG i konwersja do Base64
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

        private Rgba32 GetHeatmapColor(double value)
        {
            // Gradient kolorów (niebieski -> czerwony -> żółty -> biały)
            if (value < 0.33) return new Rgba32(0, (byte)(value * 3 * 255), 255); // Niebieski -> czerwony
            if (value < 0.66) return new Rgba32((byte)((value - 0.33) * 3 * 255), 255, 0); // Czerwony -> żółty
            return new Rgba32(255, (byte)(255 - (value - 0.66) * 3 * 255), 0); // Żółty -> biały
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
        internal (ushort[] interpolated, ushort avgValue) InterpolateDataAvgTuple(ushort[] data, int targetSize = 32)
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
