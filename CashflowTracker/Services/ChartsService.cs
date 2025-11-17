
using ScottPlot;
using ScottPlot.TickGenerators;

namespace CashflowTracker.Services
{
    public static class ChartsService
    {
        private static int Width = 1200;
        private static int Height = 1000;

        public static void GenerateCombinedChart(string outputPath, List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var categories = rollingAverageData.Select(t => t.Category).Distinct().ToArray();

            var plot = new Plot();

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
                        Position = position,
                        FillColor = GetColorForCategory(category),
                        LineWidth = 0
                    };
                    plot.Add.Bar(bar);
                    nextBarBase += sum;
                }
                position++;
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

            foreach (var category in categories)
            {
                plot.Legend.ManualItems.Add(new() { LabelText = category, FillColor = GetColorForCategory(category) });
            }
            plot.Legend.Orientation = Orientation.Horizontal;
            plot.Legend.Alignment = Alignment.UpperCenter;

            plot.SavePng(outputPath, Width, Height);
        }

        public static void GenerateCategoryChart(string outputPath, string category, List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var plot = new Plot();

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
                var sum = dayTransactions.Where(t => t.Category == category).Sum(t => t.Amount);
                var bar = new Bar
                {
                    Value = (double)(sum),
                    ValueBase = 0,
                    Position = position,
                    FillColor = GetColorForCategory(category),
                    LineWidth = 0
                };
                plot.Add.Bar(bar);
                position++;
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

            plot.Legend.ManualItems.Add(new() { LabelText = category, FillColor = GetColorForCategory(category) });
            plot.Legend.Orientation = Orientation.Horizontal;
            plot.Legend.Alignment = Alignment.UpperCenter;

            plot.SavePng(outputPath, Width, Height);
        }

        private static Color GetColorForCategory(string category)
        {
            if (category == "-") return Color.FromHex("ff0000");
            if (category == Transaction.Bevasarlas) return Color.FromHex("fecf6a");
            if (category == Transaction.OrvosGyogyszer) return Color.FromHex("6abf69");
            if (category == Transaction.Etterem) return Color.FromHex("39a275");
            if (category == Transaction.HazKert) return Color.FromHex("26734d");
            if (category == Transaction.KeszpenzFelvetel) return Color.FromHex("6a9bef");
            throw new InvalidOperationException($"No color defined for category '{category}'");
        }
    }
}
