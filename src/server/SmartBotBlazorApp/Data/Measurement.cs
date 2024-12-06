namespace SmartBotBlazorApp.Data
{
    public class Measurement
    {
        public int Id { get; set; } 
        public string RobotId { get; set; }
        public double TemperatureC { get; set; }
        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double AccelerationZ { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public DateTime Timestamp { get; set; }
        public ushort AvgDistance { get; set; }

    }
}
