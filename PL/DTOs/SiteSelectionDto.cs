namespace PL.DTOs
{
    public class SiteSelectionDto
    {
        public IEnumerable<string> EntityTypes { get; set; }
        public string  EntityName { get; set; }
        public IEnumerable<YourSitesDto>? YourSites { get; set; }
    }
}
