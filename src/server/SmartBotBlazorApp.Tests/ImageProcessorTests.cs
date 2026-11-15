using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SmartBotBlazorApp.Tests;

public class ImageProcessorTests
{
    [Theory]
    [InlineData(0, 255, 0, 0)]
    [InlineData(10, 255, 0, 0)]
    [InlineData(3500, 255, 0, 255)]
    [InlineData(ushort.MaxValue, 255, 0, 255)]
    public void GenerateHeatmapProducesValidPng(int depth, int red, int green, int blue)
    {
        var input = Enumerable.Repeat((ushort)depth, 64).ToArray();

        var result = new ImageProcessor().GenerateHeatmapBase64Image(input);

        var png = Convert.FromBase64String(result);
        byte[] signature = [137, 80, 78, 71, 13, 10, 26, 10];
        Assert.True(png.Length >= signature.Length);
        Assert.Equal(signature, png[..signature.Length]);

        using var image = Image.Load<Rgba32>(png);
        Assert.Equal(64, image.Width);
        Assert.Equal(64, image.Height);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                Assert.Equal((byte)red, pixel.R);
                Assert.Equal((byte)green, pixel.G);
                Assert.Equal((byte)blue, pixel.B);
                Assert.Equal(byte.MaxValue, pixel.A);
            }
        }
    }

    [Fact]
    public void InterpolationPreservesAllInputCorners()
    {
        var input = new ushort[64];
        input[0] = 10;
        input[7] = 20;
        input[56] = 30;
        input[63] = 40;

        var result = new ImageProcessor().InterpolateData(input, 32);

        Assert.Equal((ushort)10, result[0]);
        Assert.Equal((ushort)30, result[31]);
        Assert.Equal((ushort)20, result[31 * 32]);
        Assert.Equal((ushort)40, result[^1]);
    }

    [Fact]
    public void InterpolationRejectsFramesOtherThanEightByEight()
    {
        Assert.Throws<ArgumentException>(() => new ImageProcessor().InterpolateData(new ushort[63], 32));
    }

    [Fact]
    public void InterpolationRejectsInvalidTargetSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImageProcessor().InterpolateData(new ushort[64], 1));
    }
}
