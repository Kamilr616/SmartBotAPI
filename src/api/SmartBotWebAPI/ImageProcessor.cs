using SkiaSharp;

namespace SmartBotWebAPI
{
    public class ImageProcessor
    {
        protected internal string GenerateHeatmapBase64Image(ushort[] depthData)
        {
            // Constants for image size
            int width = 8, height = 8, scaledWidth = 32, scaledHeight = 32;

            // Transform 1D array to 2D for the 8x8 image
            var depth2D = new ushort[height, width];
            for (int i = 0; i < depthData.Length; i++)
            {
                depth2D[i / width, i % width] = depthData[i];
            }

            // Depth range assumption
            ushort minDepth = 1;
            ushort maxDepth = 4000;
            double scale = 1.0 / (maxDepth - minDepth);

            // Create an 8x8 bitmap
            using var bitmap = new SKBitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Normalize depth to 0-1 range
                    double normalizedValue = (depth2D[y, x] - minDepth) * scale;
                    normalizedValue = Math.Clamp(normalizedValue, 0, 1);

                    // Generate color using a heatmap function
                    var heatmapColor = GetHeatmapColor(normalizedValue);

                    // Set pixel color
                    bitmap.SetPixel(x, y, heatmapColor);
                }
            }

            // Scale the bitmap to 32x32
            using var scaledBitmap = bitmap.Resize(new SKImageInfo(scaledWidth, scaledHeight), SKFilterQuality.Medium);

            // Encode to PNG and convert to Base64
            using var image = SKImage.FromBitmap(scaledBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return Convert.ToBase64String(data.ToArray());
        }

        private SKColor GetHeatmapColor(double value)
        {
            // Simple heatmap gradient (blue -> red -> yellow -> white)
            if (value < 0.33) return SKColor.FromHsv((float)(240 - value * 240), 1f, 1f); // Blue -> Red
            if (value < 0.66) return SKColor.FromHsv((float)(120 - (value - 0.33) * 120), 1f, 1f); // Red -> Yellow
            return SKColor.FromHsv(0, (float)(1 - (value - 0.66) * 3), 1f); // Yellow -> White
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
