
using System.Globalization;

namespace CashflowTracker.Services
{
    public static class Categorizer
    {
        const string Ignore = "Ignore";

        public static List<Transaction> Categorize(List<Transaction> transactions)
        {
            foreach (var transaction in transactions!)
            {
                transaction.Category = GetTransactionCategory(transaction);
                


            }
            return transactions.Where(c => c.Category != Ignore).ToList();
        }

        private static string GetTransactionCategory(Transaction transaction)
        {
            foreach (var category in Keywords)
            {
                foreach (var keyword in category.Value)
                {
                    if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(transaction.Recipient, keyword, CompareOptions.IgnoreCase) >= 0)
                    {
                        return category.Key;
                    }
                }
            }

            if (transaction.Recipient == "NAV" ||
                transaction.AdditionalInfo == "Converted USD to HUF" ||
                transaction.AdditionalInfo == "Received money from Boros Csaba E.V. with reference")
            {
                return Ignore;
            }

            if (transaction.Type.Contains("Készpénzfelvét"))
            {
                return Transaction.KeszpenzFelvetel;
            }

            return transaction.Category;
        }

        private static Dictionary<string, List<string>> Keywords = new Dictionary<string, List<string>>
        {
            { Transaction.Bevasarlas, ["auchan", "tesco", "aldi", "spar"] },
            { Transaction.OrvosGyogyszer, ["gyogyszertar", "GYOGYSZE"] },
            { Transaction.Etterem, ["etterem", "foodora", "Bumerang Bufe"] },
            { Transaction.HazKert, ["Vedanta Home"] }
        };
    }
}
