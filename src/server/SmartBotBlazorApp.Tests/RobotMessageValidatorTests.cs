using Microsoft.AspNetCore.SignalR;
using SmartBotBlazorApp.Hubs;

namespace SmartBotBlazorApp.Tests;

public class RobotMessageValidatorTests
{
    [Theory]
    [InlineData(-256, 0)]
    [InlineData(256, 0)]
    [InlineData(0, -256)]
    [InlineData(0, 256)]
    public void MovementOutsidePwmRangeIsRejected(int motorA, int motorB)
    {
        Assert.Throws<HubException>(() => RobotMessageValidator.ValidateMovementCommand(motorA, motorB));
    }

    [Theory]
    [InlineData(-255, 255)]
    [InlineData(0, 0)]
    [InlineData(255, -255)]
    public void MovementInsidePwmRangeIsAccepted(int motorA, int motorB)
    {
        RobotMessageValidator.ValidateMovementCommand(motorA, motorB);
    }

    [Fact]
    public void CorrectTelemetryShapeIsAccepted()
    {
        RobotMessageValidator.ValidateTelemetry("Robot_KI", new double[7], new ushort[64]);
    }

    [Fact]
    public void InvalidTelemetryShapeOrValuesAreRejected()
    {
        Assert.Throws<HubException>(() => RobotMessageValidator.ValidateTelemetry("Robot_KI", new double[6], new ushort[64]));
        Assert.Throws<HubException>(() => RobotMessageValidator.ValidateTelemetry("Robot_KI", new double[7], new ushort[63]));

        var measurements = new double[7];
        measurements[0] = double.NaN;
        Assert.Throws<HubException>(() => RobotMessageValidator.ValidateTelemetry("Robot_KI", measurements, new ushort[64]));
    }
}
