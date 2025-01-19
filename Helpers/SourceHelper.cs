using EuroTrail.Models;

namespace EuroTrail.Helpers
{
    public static class SourceHelper
    {
        public static List<Source> GetSources(string? sourceScope)
        {
            if(sourceScope == "income")
            {
                return new List<Source>
                {
                    new Source { Code = "default", Name = "Select Source" },
                    new Source { Code = "SALARY", Name = "Salary" },
                    new Source { Code = "BONUS", Name = "Bonus" },
                    new Source { Code = "INVESTMENT", Name = "Investment Returns" },
                    new Source { Code = "RENTAL_INCOME", Name = "Rental Income" },
                    new Source { Code = "FREELANCING", Name = "Freelancing" },
                    new Source { Code = "BUSINESS", Name = "Business Income" },
                    new Source { Code = "INTEREST", Name = "Interest Income" },
                    new Source { Code = "DIVIDENDS", Name = "Dividends" },
                    new Source { Code = "ROYALTIES", Name = "Royalties" },
                    new Source { Code = "CASHBACK", Name = "Cashback Offers" },
                    new Source { Code = "GIFTS", Name = "Gifts Received" },
                    new Source { Code = "LOTTERY", Name = "Lottery Winnings" },
                    new Source { Code = "OTHER_SAVINGS", Name = "Other Sources" }
                };
            } 
            else if(sourceScope == "expense") 
            {
                return new List<Source>
                {
                    new Source { Code = "default", Name = "Select Source" },
                    new Source { Code = "RENT", Name = "Rent" },
                    new Source { Code = "FOOD", Name = "Food & Groceries" },
                    new Source { Code = "TRANSPORT", Name = "Transport" },
                    new Source { Code = "ENTERTAINMENT", Name = "Entertainment" },
                    new Source { Code = "HEALTH", Name = "Health & Medical" },
                    new Source { Code = "UTILITIES", Name = "Utilities" },
                    new Source { Code = "INSURANCE", Name = "Insurance Premiums" },
                    new Source { Code = "EMI", Name = "Loan EMI" },
                    new Source { Code = "EDUCATION", Name = "Education" },
                    new Source { Code = "CLOTHING", Name = "Clothing" },
                    new Source { Code = "GIFTS", Name = "Gifts" },
                    new Source { Code = "TRAVEL", Name = "Travel" },
                    new Source { Code = "DONATIONS", Name = "Donations" },
                    new Source { Code = "TAXES", Name = "Taxes" },
                    new Source { Code = "OTHER_EXP", Name = "Other Expenses" }
                };
            }
            else if(sourceScope == "debt") 
            {
                return new List<Source>
                {
                    new Source { Code = "default", Name = "Select Source" },
                    new Source { Code = "LOAN_BANK", Name = "Bank Loan" },
                    new Source { Code = "LOAN_PERSONAL", Name = "Personal Loan" },
                    new Source { Code = "LOAN_HOME", Name = "Home Loan" },
                    new Source { Code = "LOAN_CAR", Name = "Car Loan" },
                    new Source { Code = "LOAN_STUDENT", Name = "Student Loan" },
                    new Source { Code = "CREDIT_CARD", Name = "Credit Card Payment" },
                    new Source { Code = "BORROWED", Name = "Borrowed Amount" },
                    new Source { Code = "FRIENDS_FAMILY", Name = "Friends/Family Loan" },
                    new Source { Code = "PAYDAY", Name = "Payday Loan" },
                    new Source { Code = "OTHER_DEBT", Name = "Other Debt" }
                };
            }
            else {
                return new List<Source>
                {                   
                    new Source { Code = "EUROTRAIL_UNKNOWN", Name = "Unknown Sources" }
                };
            }
        }

        public static string GetSourceNameByCode(string code, string sourceScope)
        {           
            var sources = GetSources(sourceScope);
            var source = sources.FirstOrDefault(s => s.Code == code);
            return source?.Name ?? "Unknown Source";
        }
    }
}
