namespace CashflowTracker;

public class RollingAverageCalculator
{
    public static List<RollingAverageDto> CalculateRollingAverageByCategory(
        List<Transaction> transactions,
        int monthsWindow)
    {
        var result = new List<RollingAverageDto>();
        var categories = transactions.Select(t => t.Category).Distinct().ToList();
        var months = transactions
            .Select(t => new DateOnly(t.Date.Year, t.Date.Month, 1))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var category in categories)
        {
            var categoryTransactions = transactions.Where(t => t.Category == category).ToList();

            for (int i = 0; i < months.Count; i++)
            {
                var currentMonth = months[i];
                var startMonth = i >= monthsWindow - 1 ? months[i - monthsWindow + 1] : months[0];

                var relevantTransactions = categoryTransactions
                    .Where(t =>
                    {
                        var transactionMonth = new DateOnly(t.Date.Year, t.Date.Month, 1);
                        return transactionMonth >= startMonth && transactionMonth <= currentMonth;
                    })
                    .ToList();

                var groupedByMonth = relevantTransactions
                    .GroupBy(t => new DateOnly(t.Date.Year, t.Date.Month, 1))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var monthsInWindow = i >= monthsWindow - 1 ? monthsWindow : i + 1;
                var totalAmount = groupedByMonth.Values.Sum();
                var average = totalAmount / monthsInWindow;

                result.Add(new RollingAverageDto
                {
                    Month = currentMonth,
                    Category = category,
                    Amount = average
                });
            }
        }

        return result;
    }
}
