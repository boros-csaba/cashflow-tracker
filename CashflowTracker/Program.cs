using CashflowTracker;
using CashflowTracker.Services;
using ScottPlot;
using ScottPlot.TickGenerators;
using System.Collections.Generic;

var endDate = new DateTime(2025, 9, 15);
var startDate = endDate.AddMonths(-24);

string dataFolder = "../../../../data";
var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);
transactions = transactions.OrderByDescending(t => t.Date).ToList();
Console.WriteLine($"Total transactions loaded: {transactions.Count}");

transactions = transactions.Where(t => t.Date >= startDate).ToList();
transactions = transactions.Where(t => t.Amount > 0).ToList();
transactions = transactions.Where(t => t.Date <= endDate).ToList();

transactions = TransferFilter.RemoveInternalTransfers(transactions);
Console.WriteLine($"Transactions after removing internal transfers: {transactions.Count}");

Categorizer.Categorize(transactions);


var rollingAverageData = RollingAverageCalculator.CalculateRollingAverageByCategory(transactions, 90);
rollingAverageData = rollingAverageData
    .GroupBy(t => new DateOnly(t.Day.Year, t.Day.Month, 1))
    .Select(g => new RollingAverageDto
    {
        Day = g.Key,
        Category = g.First().Category,
        Amount = g.Sum(t => t.Amount)
    }).ToList();



var colors = new List<Color>{ Color.FromHex("ff0000"), Color.FromHex("00ff00"), Color.FromHex("0000ff") };
var plot = new Plot();
var categories = rollingAverageData.Select(t => t.Category).Distinct().ToArray();
var days = new List<DateOnly>();
while (startDate < endDate)
{
    days.Add(new DateOnly(startDate.Year, startDate.Month, startDate.Day));
    startDate = startDate.AddDays(1);
}
var position = 0;
foreach (var day in days)
{
    if (day.Day != 1)
        continue;
    var dayTransactions = rollingAverageData.Where(t => t.Day.Year == day.Year && t.Day.Month == day.Month && t.Day.Day == day.Day).ToList();
    var nextBarBase = 0m;
    foreach (var category in categories)
    {
        var sum = dayTransactions.Where(t => t.Category == category).Sum(t => t.Amount);
        var bar = new Bar
        {
            Value = (double)(nextBarBase + sum),
            ValueBase = (double)nextBarBase,
            Position = position++,
            FillColor = colors[Array.IndexOf(categories, category) % colors.Count],
        };
        plot.Add.Bar(bar);
        nextBarBase += sum;
    }
}

var tickGen = new NumericManual();
position = 0;
foreach (var day in days)
{
    if (day.Day != 1)
        continue;
    tickGen.AddMajor(position++, day.ToString("yyyy-MM-dd"));
}
plot.Axes.Bottom.TickGenerator = tickGen;
plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
plot.Axes.Bottom.TickLabelStyle.OffsetY = 18;
plot.Axes.Bottom.TickLabelStyle.OffsetX = 10;

plot.Axes.Margins(bottom: 0, top: .3);
plot.XLabel(" ");

plot.SavePng("demo.png", 1200, 1000);