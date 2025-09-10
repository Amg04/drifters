using DAL.Models;
using System.Drawing;

namespace PL.DTOs
{
    public class CameraDetectionDto
    {
        public int CameraId { get; set; }
        public string Status { get; set; }
        public float Crowd_density { get; set; }
        public string Activity_type { get; set; }
        public float Threshold { get; set; }
        public string Heatmap { get; set; }

        #region Mapping

        public static explicit operator CameraDetection(CameraDetectionDto ViewModel)
        {
            return new CameraDetection
            {
                CameraId = ViewModel.CameraId,
                Status = ViewModel.Status,
                Crowd_density = ViewModel.Crowd_density,
                Activity_type = ViewModel.Activity_type,
                Threshold = ViewModel.Threshold,
                Heatmap = ViewModel.Heatmap,
            };
        }

        #endregion

    }
}
