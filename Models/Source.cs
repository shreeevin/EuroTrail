namespace EuroTrail.Models
{
    public class Source
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return Name ?? "Unknown Source"; 
        }
    }
}
