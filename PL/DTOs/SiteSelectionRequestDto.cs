using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class SiteSelectionRequestDto
    {
        public string? EntityType { get; set; }
        public string? EntityName { get; set; }
    }
}
