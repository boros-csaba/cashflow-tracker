
namespace CashflowTracker.Services
{
    public static class Categorizer
    {
        public static void Categorize(List<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                if (transaction.Recipient.Contains("E.ON", StringComparison.OrdinalIgnoreCase) ||
                    transaction.Recipient.Contains("DÉMÁSZ", StringComparison.OrdinalIgnoreCase) ||
                    transaction.Recipient.Contains("NKM", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Category = Transaction.HazAram;
                }
                else
                {
                    transaction.Category = Transaction.Egyeb;
                }
            }
        }
    }
}
