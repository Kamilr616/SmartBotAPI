namespace SmartBotBlazorApp.Data;

public sealed class RobotMeasurementThrottle(TimeProvider timeProvider)
{
    private const int MaximumTrackedRobots = 256;
    private readonly Dictionary<string, DateTimeOffset> _lastSaveByRobot = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    public bool TryAcquire(string robotId, TimeSpan interval, out DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(robotId);

        timestamp = timeProvider.GetUtcNow();

        lock (_sync)
        {
            if (_lastSaveByRobot.TryGetValue(robotId, out var previousTimestamp) &&
                timestamp - previousTimestamp < interval)
            {
                return false;
            }

            if (!_lastSaveByRobot.ContainsKey(robotId) && _lastSaveByRobot.Count >= MaximumTrackedRobots)
            {
                var oldestRobot = _lastSaveByRobot.MinBy(entry => entry.Value).Key;
                _lastSaveByRobot.Remove(oldestRobot);
            }

            _lastSaveByRobot[robotId] = timestamp;
            return true;
        }
    }
}
