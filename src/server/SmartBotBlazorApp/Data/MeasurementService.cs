using Microsoft.EntityFrameworkCore;

namespace SmartBotBlazorApp.Data
{
    public class MeasurementService
    {
        private readonly ApplicationDbContext _context;
        private static DateTime _lastSaveTime = DateTime.MinValue;
        private static readonly TimeSpan SaveThrottleDuration = TimeSpan.FromSeconds(1);


        public MeasurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMeasurementAsync(Measurement measurement)
        {
            _context.Measurements.Add(measurement);
            await _context.SaveChangesAsync();
        }

        public async Task SaveMeasurementsToDatabase(string robotId, double[] measurements, ushort distance)
        {
            var currentTime = DateTime.Now;

            if ((currentTime - _lastSaveTime) >= SaveThrottleDuration)
            {
                var newMeasurement = new Measurement
                {
                    RobotId = robotId,
                    AccelerationX = measurements[0],
                    AccelerationY = measurements[1],
                    AccelerationZ = measurements[2],
                    RotationX = measurements[3],
                    RotationY = measurements[4],
                    RotationZ = measurements[5],
                    TemperatureC = measurements[6],
                    AvgDistance = distance,
                    Timestamp = currentTime
                };

                _lastSaveTime = currentTime;
                await AddMeasurementAsync(newMeasurement);
            }
            else
            {
                await Task.CompletedTask;
            }
        }
        
        public double[] RoundMeasurements(double[] measurementsArray, int precision = 2)
        {
            var newMeasurementsArray = new double[7];

            for (var i = 0; i < measurementsArray.Length; i++)
            {
                newMeasurementsArray[i] = Math.Round(measurementsArray[i], precision, MidpointRounding.ToEven);
            }

            return newMeasurementsArray;
        }
        public async Task<List<Measurement>> GetAllMeasurementsFromDatabaseAsync()
        {
            return await _context.Measurements.OrderBy(m => m.Timestamp).ToListAsync();
        }

        public async Task<List<Measurement>> GetMeasurementsFromDatabaseAsync(DateTime startTimestamp, DateTime endTimestamp)
        {
            return await _context.Measurements
                .Where(m => m.Timestamp >= startTimestamp && m.Timestamp <= endTimestamp)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }

}
