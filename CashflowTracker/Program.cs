using CashflowTracker;
using CashflowTracker.Services;
using System.Text;

string dataFolder = "../../../../data";
string outputFolder = "C:/Users/boros/My Drive/penz";
var endDate = new DateTime(2025, 9, 15, 0, 0, 0, DateTimeKind.Local);
var startDate = endDate.AddMonths(-24);




var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);
transactions = transactions.OrderByDescending(t => t.Date).ToList();
Console.WriteLine($"Total transactions loaded: {transactions.Count}");

transactions = TransferFilter.RemoveInternalTransfers(transactions);

transactions = transactions.Where(t => t.Date >= startDate).ToList();
transactions = transactions.Where(t => t.Amount > 0).ToList();
transactions = transactions.Where(t => t.Date <= endDate).ToList();


Console.WriteLine($"Transactions after removing internal transfers: {transactions.Count}");

transactions = Categorizer.Categorize(transactions);


var rollingAverageData = RollingAverageCalculator.CalculateRollingAverageByCategory(transactions, 90);
rollingAverageData = rollingAverageData
    .GroupBy(t => new { Date = new DateOnly(t.Day.Year, t.Day.Month, 1), t.Category })
    .Select(g => new RollingAverageDto
    {
        Day = g.Key.Date,
        Category = g.Key.Category,
        Amount = g.Sum(t => t.Amount)
    }).ToList();


ChartsService.GenerateCombinedChart(Path.Combine(outputFolder, "rolling_average_combined_chart.png"), rollingAverageData, startDate, endDate);

var categories = rollingAverageData.Select(t => t.Category).Distinct().ToArray();
foreach (var category in categories)
{
    var categoryData = rollingAverageData.Where(t => t.Category == category).ToList();
    ChartsService.GenerateCategoryChart(Path.Combine(outputFolder, $"rolling_average_{category}_chart.png"), category, categoryData, startDate, endDate);
}

var sb = new StringBuilder();
sb.AppendLine("Id\tDate\tType\tRecipient\tAmount\tCurrency\tAdditionalInfo\tSource\tCategory");
var unknows = transactions.Where(t => t.Category == "-")
    .OrderByDescending(t => t.Amount)
    .ToList();
foreach (var item in unknows)
{
    sb.AppendLine($"{item.Id}\t{item.Date:yyyy-MM-dd}\t{item.Type}\t{item.Recipient}\t{item.Amount}\t{item.Currency}\t{item.AdditionalInfo}\t{item.Source}\t{item.Category}");
}
var s = sb.ToString();
var ss = 0;

Reports.GenerateHtmlReport(Path.Combine(outputFolder, "cashflow_report.html"), transactions, rollingAverageData, startDate, endDate);