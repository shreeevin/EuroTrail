namespace EuroTrail.Models
{
    public class Currency
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return Name ?? "Unknown Currency"; 
        }
    }
}
