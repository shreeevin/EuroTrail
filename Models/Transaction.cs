namespace EuroTrail.Models
{
    public class Transaction
    {
        public int Id { get; set; }        
        public int UserId { get; set; }
        public string? Tnx { get; set; } 
        public string? Type { get; set; }        
        public string? Scope { get; set; }
        public string? Source { get; set; }        
        public List<string> Tags { get; set; } = new List<string>();
        public string? Note { get; set; }        
        public decimal Fee { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public string? Status { get; set; } = "pending";        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
