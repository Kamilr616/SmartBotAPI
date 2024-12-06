namespace SmartBotBlazorApp.Data
{
    public class MeasurementService
    {
        private readonly ApplicationDbContext _context;

        public MeasurementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMeasurementAsync(Measurement measurement)
        {
            _context.Measurements.Add(measurement);
            await _context.SaveChangesAsync();
        }

        public async Task SaveMeasurementsToDatabase(string robotID, double[] measurements, ushort distance)
        {
            var newMeasurement = new Measurement
            {
                RobotId = robotID,
                AccelerationX = measurements[0],
                AccelerationY = measurements[1],
                AccelerationZ = measurements[2],
                RotationX = measurements[3],
                RotationY = measurements[4],
                RotationZ = measurements[5],
                TemperatureC = measurements[6],
                AvgDistance = distance
            };

            await AddMeasurementAsync(newMeasurement);
        }

        public double[] RoundMeasurements(double[] measurementsArray, int precision = 2)
        {
            double[] newMeasurementsArray = new double[7];

            for (int i = 0; i < measurementsArray.Length; i++)
            {
                newMeasurementsArray[i] = Math.Round(measurementsArray[i], precision, MidpointRounding.ToEven);
            }

            return newMeasurementsArray;
        }
    }

}
