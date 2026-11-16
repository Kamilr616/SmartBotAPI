using SkiaSharp;

namespace SmartBotBlazorApp.Tests;

public class ImageProcessorTests
{
    [Theory]
    [InlineData(0, 255, 0, 0)]
    [InlineData(10, 255, 0, 0)]
    [InlineData(1755, 0, 255, 0)]
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

        using var image = SKBitmap.Decode(png);
        Assert.NotNull(image);
        Assert.Equal(64, image.Width);
        Assert.Equal(64, image.Height);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                Assert.Equal((byte)red, pixel.Red);
                Assert.Equal((byte)green, pixel.Green);
                Assert.Equal((byte)blue, pixel.Blue);
                Assert.Equal(byte.MaxValue, pixel.Alpha);
            }
        }
    }

    [Fact]
    public void GenerateHeatmapPreservesXMajorOrientationAndInterpolatesColors()
    {
        var input = new ushort[64];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                input[x * 8 + y] = x < 4 ? (ushort)10 : (ushort)3500;
            }
        }

        var result = new ImageProcessor().GenerateHeatmapBase64Image(input);

        using var image = SKBitmap.Decode(Convert.FromBase64String(result));
        Assert.NotNull(image);
        Assert.Equal(64, image.Width);
        Assert.Equal(64, image.Height);
        var red = new SKColor(255, 0, 0, 255);
        var violet = new SKColor(255, 0, 255, 255);
        Assert.Equal(red, image.GetPixel(8, 8));
        Assert.Equal(red, image.GetPixel(8, 56));
        Assert.Equal(violet, image.GetPixel(56, 8));
        Assert.Equal(violet, image.GetPixel(56, 56));

        var leftTransition = image.GetPixel(31, 24);
        var rightTransition = image.GetPixel(32, 24);
        Assert.Equal((byte)255, leftTransition.Red);
        Assert.Equal((byte)0, leftTransition.Green);
        Assert.Equal((byte)255, leftTransition.Alpha);
        Assert.Equal((byte)255, rightTransition.Red);
        Assert.Equal((byte)0, rightTransition.Green);
        Assert.Equal((byte)255, rightTransition.Alpha);
        Assert.InRange(leftTransition.Blue, (byte)1, (byte)254);
        Assert.InRange(rightTransition.Blue, (byte)1, (byte)254);
        Assert.True(leftTransition.Blue < rightTransition.Blue);
        Assert.Equal(leftTransition, image.GetPixel(31, 40));
        Assert.Equal(rightTransition, image.GetPixel(32, 40));
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
