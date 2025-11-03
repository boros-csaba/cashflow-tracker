namespace CashflowTracker;

public class RollingAverageCalculator
{
    public static List<RollingAverageDto> CalculateRollingAverageByCategory(
        List<Transaction> transactions,
        int daysWindow)
    {
        var result = new List<RollingAverageDto>();
        var categories = transactions.Select(t => t.Category).Distinct().ToList();
        var days = transactions
            .Select(t => new DateOnly(t.Date.Year, t.Date.Month, t.Date.Day))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        foreach (var category in categories)
        {
            var categoryTransactions = transactions.Where(t => t.Category == category).ToList();

            for (int i = 0; i < days.Count; i++)
            {
                var currentDay = days[i];
                var startMonth = i >= daysWindow - 1 ? days[i - daysWindow + 1] : days[0];

                var relevantTransactions = categoryTransactions
                    .Where(t =>
                    {
                        var transactionDay = new DateOnly(t.Date.Year, t.Date.Month, t.Date.Day);
                        return transactionDay >= startMonth && transactionDay <= currentDay;
                    })
                    .ToList();

                var groupedByDays = relevantTransactions
                    .GroupBy(t => t.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                var monthsInWindow = i >= daysWindow - 1 ? daysWindow : i + 1;
                var totalAmount = groupedByDays.Values.Sum();
                var average = totalAmount / monthsInWindow;

                result.Add(new RollingAverageDto
                {
                    Day = currentDay,
                    Category = category,
                    Amount = average
                });
            }
        }

        return result;
    }
}
