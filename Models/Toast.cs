namespace EuroTrail.Models
{
    public class Toast
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
    }
}