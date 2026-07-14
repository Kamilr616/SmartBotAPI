using Microsoft.AspNetCore.SignalR;

namespace SmartBotBlazorApp.Hubs;

public static class RobotMessageValidator
{
    public const int MeasurementCount = 7;
    public const int DepthPixelCount = 64;
    public const int MaximumMotorPower = 255;
    public const int MaximumRobotIdLength = 50;

    public static void ValidateMovementCommand(int motorA, int motorB)
    {
        if (motorA is < -MaximumMotorPower or > MaximumMotorPower ||
            motorB is < -MaximumMotorPower or > MaximumMotorPower)
        {
            throw new HubException($"Motor power must be between {-MaximumMotorPower} and {MaximumMotorPower}.");
        }
    }

    public static void ValidateTelemetry(string user, double[] measurements, ushort[] rawMatrix)
    {
        if (string.IsNullOrWhiteSpace(user) || user.Length > MaximumRobotIdLength)
        {
            throw new HubException($"Robot identifier must contain between 1 and {MaximumRobotIdLength} characters.");
        }
        if (measurements is null || measurements.Length != MeasurementCount || measurements.Any(value => !double.IsFinite(value)))
        {
            throw new HubException($"Telemetry must contain exactly {MeasurementCount} finite measurements.");
        }
        if (rawMatrix is null || rawMatrix.Length != DepthPixelCount)
        {
            throw new HubException($"Depth frame must contain exactly {DepthPixelCount} values.");
        }
    }
}
