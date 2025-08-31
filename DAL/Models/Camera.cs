using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Camera:BaseClass
    {
        public string Name { get; set; }
        public string StreamUrl { get; set; }
        public bool IsActive { get; set; }
        public CameraSettings Settings { get; set; }
        public virtual ICollection<Detection> Detections { get; set; } = new List<Detection>();

    }
}
