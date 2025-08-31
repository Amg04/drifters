using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class CameraSettings
    {
        public string Location { get; set; } = string.Empty;
        public string Resolution { get; set; } = "1920x1080";
        public int Fps { get; set; } = 30;
        public float DetectionThreshold { get; set; } = 0.5f;
        public bool SaveImages { get; set; } = true;
        public bool EnableRecording { get; set; } = false;
        public string RecordingPath { get; set; } = string.Empty;
        public int MaxStorageDays { get; set; } = 30;
        public List<DetectionZone> DetectionZones { get; set; } = new();
        public AlertSettings AlertSettings { get; set; } = new();
        public ScheduleSettings ScheduleSettings { get; set; } = new();
    }
}
