using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class AlertSettings
    {
        public bool EmailEnabled { get; set; } = false;
        public bool SmsEnabled { get; set; } = false;
        public bool WebhookEnabled { get; set; } = false;
        public List<string> EmailRecipients { get; set; } = new();
        public List<string> SmsRecipients { get; set; } = new();
        public string WebhookUrl { get; set; } = string.Empty;
        public int CooldownMinutes { get; set; } = 5;
    }

}
