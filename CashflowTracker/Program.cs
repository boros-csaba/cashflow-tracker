using CashflowTracker;
using CashflowTracker.Services;
using ScottPlot;
using ScottPlot.TickGenerators;

string dataFolder = "../../../../data";
var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);
transactions = transactions.OrderByDescending(t => t.Date).ToList();

Console.WriteLine($"Total transactions loaded: {transactions.Count}");

transactions = TransferFilter.RemoveInternalTransfers(transactions);
Console.WriteLine($"Transactions after removing internal transfers: {transactions.Count}");

Categorizer.Categorize(transactions);

// todo better approach
transactions = transactions.Where(t => t.Amount > 0).ToList();

var rollingAverageData = RollingAverageCalculator.CalculateRollingAverageByCategory(transactions, 3);



var colors = new List<Color>{ Color.FromHex("ff0000"), Color.FromHex("00ff00"), Color.FromHex("0000ff") };
var plot = new Plot();
var categories = rollingAverageData.Select(t => t.Category).Distinct().ToArray();
var months = transactions.Select(t => new DateOnly(t.Date.Year, t.Date.Month, 1)).Distinct().OrderBy(d => d).ToArray();
foreach (var month in months)
{
    var monthTransactions = transactions.Where(t => t.Date.Year == month.Year && t.Date.Month == month.Month).ToList();
    var nextBarBase = 0m;
    foreach (var category in categories)
    {
        var sum = monthTransactions.Where(t => t.Category == category).Sum(t => t.Amount);
        var bar = new Bar
        {
            Value = (double)(nextBarBase + sum),
            ValueBase = (double)nextBarBase,
            Position = Array.IndexOf(months, month),
            FillColor = colors[Array.IndexOf(categories, category) % colors.Count],
        };
        plot.Add.Bar(bar);
        nextBarBase += sum;
    }
}

var tickGen = new NumericManual();
foreach (var month in months)
{
    var position = Array.IndexOf(months, month);
    tickGen.AddMajor(position, month.ToString("yyyy-MM"));
}
plot.Axes.Bottom.TickGenerator = tickGen;
plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
plot.Axes.Bottom.TickLabelStyle.OffsetY = 18;
plot.Axes.Bottom.TickLabelStyle.OffsetX = 10;

plot.Axes.Margins(bottom: 0, top: .3);
plot.XLabel("Horizontal Axis");
plot.YLabel("Vertical Axis");

plot.SavePng("demo.png", 1200, 1000);