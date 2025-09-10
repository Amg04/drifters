namespace PL.DTOs
{
    public class DashboardDto
    {
        public int TotalAlerts { get; set; }
        public int ActiveCameras { get; set; }
        public int CriticalEvents { get; set; }
        public IEnumerable<CameraStatusDto> CameraStatus { get; set; }
    }
}
