using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SmartBotWebAPI
{
    internal class ImageProcessor
    {
        protected internal string GenerateHeatmapBase64Image(ushort[] depthData)
        {
            // Przekształć 1D na 2D dla obrazu 8x8
            int width = 8, height = 8;
            var depth2D = new ushort[height, width];
            for (int i = 0; i < depthData.Length; i++)
            {
                depth2D[i / width, i % width] = depthData[i];
            }

            // Ustal zakres głębi (zakładamy, że depthData zawiera wartości 1-4000)
            ushort minDepth = 1;
            ushort maxDepth = 4000;
            double scale = 1.0 / (maxDepth - minDepth);

            // Tworzenie bitmapy 8x8 z mapą ciepła
            using var bmp = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Normalizacja wartości głębi do przedziału 0-1
                    double normalizedValue = (depth2D[y, x] - minDepth) * scale;

                    // Generowanie koloru z mapy ciepła
                    Color heatmapColor = GetHeatmapColor(normalizedValue);
                    bmp.SetPixel(x, y, heatmapColor);
                }
            }

            // Skalowanie obrazu do 32x32
            using var scaledBmp = new Bitmap(32, 32);
            using (var graphics = Graphics.FromImage(scaledBmp))
            {
                graphics.InterpolationMode = InterpolationMode.Bilinear;
                graphics.DrawImage(bmp, 0, 0, 32, 32);
            }

            // Konwertowanie na Base64
            using var ms = new MemoryStream();
            scaledBmp.Save(ms, ImageFormat.Png);
            byte[] imageBytes = ms.ToArray();

            return Convert.ToBase64String(imageBytes);
        }

        protected static Color GetHeatmapColor(double value)
        {
            // Przypisywanie koloru na podstawie wartości znormalizowanej (0.0 - 1.0)
            // Mapujemy wartość na kolory od niebieskiego do czerwonego
            if (value <= 0.25)
            {
                // Przejście od niebieskiego do zielonego
                int g = (int)(value / 0.25 * 255);
                return Color.FromArgb(0, g, 255);
            }
            else if (value <= 0.5)
            {
                // Przejście od zielonego do żółtego
                int r = (int)((value - 0.25) / 0.25 * 255);
                return Color.FromArgb(r, 255, 0);
            }
            else if (value <= 0.75)
            {
                // Przejście od żółtego do pomarańczowego
                int g = (int)(255 - (value - 0.5) / 0.25 * 255);
                return Color.FromArgb(255, g, 0);
            }
            else
            {
                // Przejście od pomarańczowego do czerwonego
                int b = (int)((1.0 - value) / 0.25 * 255);
                return Color.FromArgb(255, 0, b);
            }
        }
        protected internal ushort[] InterpolateData(ushort[] data, int targetSize = 32)
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

        protected internal (ushort[] interpolated, ushort avgValue) InterpolateDataAvgTuple(ushort[] data, int targetSize = 32)
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
