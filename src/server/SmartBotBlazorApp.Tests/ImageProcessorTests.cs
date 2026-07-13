namespace SmartBotBlazorApp.Tests;

public class ImageProcessorTests
{
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
