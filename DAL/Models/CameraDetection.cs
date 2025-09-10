namespace DAL.Models
{
    public class CameraDetection : BaseClass
    {
        public int CameraId { get; set; }
        public Camera Camera { get; set; } = null!;
        public string Status { get; set; }
        public float Crowd_density { get; set; }
         public string Activity_type { get; set; }
         public float Threshold { get; set; }
         public string Heatmap { get; set; }
    }
}
