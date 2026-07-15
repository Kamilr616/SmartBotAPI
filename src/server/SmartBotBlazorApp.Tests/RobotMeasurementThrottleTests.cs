using SmartBotBlazorApp.Data;

namespace SmartBotBlazorApp.Tests;

public class RobotMeasurementThrottleTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    [Fact]
    public void ThrottleIsIndependentForEachRobot()
    {
        var timeProvider = new ManualTimeProvider();
        var throttle = new RobotMeasurementThrottle(timeProvider);

        Assert.True(throttle.TryAcquire("robot-a", Interval, out _));
        Assert.False(throttle.TryAcquire("robot-a", Interval, out _));
        Assert.True(throttle.TryAcquire("robot-b", Interval, out _));

        timeProvider.Advance(Interval);

        Assert.True(throttle.TryAcquire("robot-a", Interval, out var timestamp));
        Assert.Equal(timeProvider.GetUtcNow(), timestamp);
    }

    [Fact]
    public void ConcurrentAttemptsAllowOnlyOneWrite()
    {
        var throttle = new RobotMeasurementThrottle(new ManualTimeProvider());
        var successes = 0;

        Parallel.For(0, 100, iteration =>
        {
            if (throttle.TryAcquire("robot-a", Interval, out _))
            {
                Interlocked.Increment(ref successes);
            }
        });

        Assert.Equal(1, successes);
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow = new(2026, 7, 15, 10, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow += duration;
    }
}
