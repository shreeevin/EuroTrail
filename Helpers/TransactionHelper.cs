using EuroTrail.Models;

namespace EuroTrail.Helpers
{
    public static class TransactionHelper
    {
        public static List<Filter> GetTransactionTypes()
        {
            return new List<Filter>
            {
                new Filter { Code = "default", Name = "Default" },                
                new Filter { Code = "debit", Name = "Debit" },
                new Filter { Code = "credit", Name = "Credit" }                
            };
        }
        
        public static List<Filter> GetTransactionScopes()
        {
            return new List<Filter>
            {
                new Filter { Code = "default", Name = "Default" },                
                new Filter { Code = "income", Name = "Income" },
                new Filter { Code = "expense", Name = "Expense" },
                new Filter { Code = "debt", Name = "Debt" }                
            };
        }

        public static List<Filter> GetTransactionStatus()
        {
            return new List<Filter>
            {
                new Filter { Code = "default", Name = "Default" },                
                new Filter { Code = "completed", Name = "Completed" },
                new Filter { Code = "pending", Name = "Pending" }                
            };
        }

        public static List<Filter> GetTransactionPriceSort()
        {
            return new List<Filter>
            {
                new Filter { Code = "default", Name = "Default" },                
                new Filter { Code = "h2l", Name = "High to Low" },
                new Filter { Code = "l2h", Name = "Low to High" }                
            };
        }
    }
}