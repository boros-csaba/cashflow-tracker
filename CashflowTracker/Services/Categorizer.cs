
using System.Globalization;

namespace CashflowTracker.Services
{
    public static class Categorizer
    {
        public static List<Transaction> Categorize(List<Transaction> transactions)
        {
            foreach (var transaction in transactions!)
            {
                foreach (var category in Keywords)
                {
                    foreach (var keyword in category.Value)
                    {
                        if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(transaction.Recipient, keyword, CompareOptions.IgnoreCase) >= 0)
                        {
                            transaction.Category = category.Key;
                        }
                    }
                }
            }
            return transactions;
        }

        private static Dictionary<string, List<string>> Keywords = new Dictionary<string, List<string>>
        {
            { Transaction.Bevasarlas, ["auchan", "tesco", "aldi", "spar"] }
        };
    }
}
