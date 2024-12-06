using System.ComponentModel.DataAnnotations;


namespace SmartBotBlazorApp.Data
{
    public class Measurement
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RobotId { get; set; } = "N/A";

        [Required]
        public double TemperatureC { get; set; }

        [Required] 
        public double AccelerationX { get; set; }

        [Required]
        public double AccelerationY { get; set; }

        [Required] 
        public double AccelerationZ { get; set; }

        [Required] 
        public double RotationX { get; set; }

        [Required]
        public double RotationY { get; set; }

        [Required] 
        public double RotationZ { get; set; }

        [Required] 
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required] 
        public ushort AvgDistance { get; set; } = 0;
    }

}
