namespace CashflowTracker.Services;

public static class TransferFilter
{
    public static List<Transaction> RemoveInternalTransfers(List<Transaction> transactions)
    {
        var result = new List<Transaction>(transactions);
        var toRemove = new HashSet<string>();

        for (int i = 0; i < transactions.Count; i++)
        {
            if (toRemove.Contains(transactions[i].Id)) continue;

            for (int j = i + 1; j < transactions.Count; j++)
            {
                if (toRemove.Contains(transactions[j].Id)) continue;

                if (IsInternalTransfer(transactions[i], transactions[j]))
                {
                    toRemove.Add(transactions[i].Id);
                    toRemove.Add(transactions[j].Id);
                    break;
                }
            }
        }

        return result.Where(t => !toRemove.Contains(t.Id)).ToList();
    }

    private static bool IsInternalTransfer(Transaction t1, Transaction t2)
    {
        if (t1.Source == t2.Source) return false;

        if (t1.Currency != t2.Currency) return false;

        var dateDiff = Math.Abs((t1.Date - t2.Date).TotalDays);
        if (dateDiff > 2) return false;

        var amountMatch = t1.Amount == -t2.Amount ||
                         Math.Abs(t1.Amount + t2.Amount) < 0.01m;

        return amountMatch;
    }
}
