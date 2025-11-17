using ScottPlot;
using ScottPlot.TickGenerators;
using System.Text;

namespace CashflowTracker.Services
{
    public static class Reports
    {
        private static int Width = 1200;
        private static int Height = 800;

        public static void GenerateHtmlReport(string outputPath, List<Transaction> transactions, List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var chartBase64 = GenerateChartAsBase64(rollingAverageData, startDate, endDate);

            var categories = rollingAverageData.Select(t => t.Category).Distinct().ToList();
            var categoryCharts = new Dictionary<string, string>();
            foreach (var category in categories)
            {
                var categoryData = rollingAverageData.Where(t => t.Category == category).ToList();
                categoryCharts[category] = GenerateCategoryChartAsBase64(category, categoryData, startDate, endDate);
            }

            var html = BuildHtmlContent(chartBase64, categoryCharts, transactions, rollingAverageData, startDate, endDate);

            File.WriteAllText(outputPath, html);
        }

        private static string GenerateChartAsBase64(List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var categories = rollingAverageData.Select(t => t.Category).Distinct().ToArray();
            var plot = new Plot();

            var days = new List<DateOnly>();
            var currentDate = startDate;
            while (currentDate < endDate)
            {
                days.Add(new DateOnly(currentDate.Year, currentDate.Month, currentDate.Day));
                currentDate = currentDate.AddDays(1);
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
                tickGen.AddMajor(position++, day.ToString("yyyy-MM"));
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

            var imageBytes = plot.GetImageBytes(Width, Height, ImageFormat.Png);
            return Convert.ToBase64String(imageBytes);
        }

        private static string GenerateCategoryChartAsBase64(string category, List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var plot = new Plot();

            var days = new List<DateOnly>();
            var currentDate = startDate;
            while (currentDate < endDate)
            {
                days.Add(new DateOnly(currentDate.Year, currentDate.Month, currentDate.Day));
                currentDate = currentDate.AddDays(1);
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
                tickGen.AddMajor(position++, day.ToString("yyyy-MM"));
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

            var imageBytes = plot.GetImageBytes(Width, Height, ImageFormat.Png);
            return Convert.ToBase64String(imageBytes);
        }

        private static string BuildHtmlContent(string chartBase64, Dictionary<string, string> categoryCharts, List<Transaction> transactions, List<RollingAverageDto> rollingAverageData, DateTime startDate, DateTime endDate)
        {
            var categories = rollingAverageData.Select(t => t.Category).Distinct().ToList();
            var totalAmount = rollingAverageData.Sum(t => t.Amount);
            var monthlyAverage = totalAmount / ((endDate - startDate).Days / 30.0m);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine("    <title>Cashflow Report</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }");
            sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; background-color: white; padding: 30px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("        h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }");
            sb.AppendLine("        .overview { background-color: #f9f9f9; padding: 20px; border-radius: 8px; margin: 20px 0; }");
            sb.AppendLine("        .overview h2 { margin-top: 0; color: #4CAF50; }");
            sb.AppendLine("        .stat-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }");
            sb.AppendLine("        .stat-card { background-color: white; padding: 15px; border-left: 4px solid #4CAF50; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .stat-label { font-size: 14px; color: #666; margin-bottom: 5px; }");
            sb.AppendLine("        .stat-value { font-size: 24px; font-weight: bold; color: #333; }");
            sb.AppendLine("        .chart-container { text-align: center; margin: 30px 0; }");
            sb.AppendLine("        .chart-container img { max-width: 100%; height: auto; border: 1px solid #ddd; border-radius: 4px; }");
            sb.AppendLine("        .category-section { margin: 20px 0; }");
            sb.AppendLine("        details { background-color: #f9f9f9; border: 1px solid #ddd; border-radius: 8px; margin: 10px 0; }");
            sb.AppendLine("        details summary { padding: 15px 20px; cursor: pointer; font-size: 18px; font-weight: bold; color: #333; user-select: none; }");
            sb.AppendLine("        details summary:hover { background-color: #e9e9e9; }");
            sb.AppendLine("        details[open] summary { border-bottom: 1px solid #ddd; background-color: #e9e9e9; }");
            sb.AppendLine("        .category-content { padding: 20px; }");
            sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; font-size: 14px; }");
            sb.AppendLine("        table thead { background-color: #4CAF50; color: white; }");
            sb.AppendLine("        table th { padding: 12px; text-align: left; font-weight: bold; }");
            sb.AppendLine("        table td { padding: 10px; border-bottom: 1px solid #ddd; }");
            sb.AppendLine("        table tbody tr:hover { background-color: #f5f5f5; }");
            sb.AppendLine("        table tbody tr:nth-child(even) { background-color: #fafafa; }");
            sb.AppendLine("        .amount-cell { text-align: right; font-weight: bold; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <h1>Cashflow Tracker Report</h1>");
            sb.AppendLine("        <div class=\"overview\">");
            sb.AppendLine("            <h2>Overview</h2>");
            sb.AppendLine("            <div class=\"stat-grid\">");
            sb.AppendLine($"                <div class=\"stat-card\">");
            sb.AppendLine($"                    <div class=\"stat-label\">Period</div>");
            sb.AppendLine($"                    <div class=\"stat-value\">{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}</div>");
            sb.AppendLine($"                </div>");
            sb.AppendLine($"                <div class=\"stat-card\">");
            sb.AppendLine($"                    <div class=\"stat-label\">Total Amount</div>");
            sb.AppendLine($"                    <div class=\"stat-value\">{totalAmount:N0}</div>");
            sb.AppendLine($"                </div>");
            sb.AppendLine($"                <div class=\"stat-card\">");
            sb.AppendLine($"                    <div class=\"stat-label\">Monthly Average</div>");
            sb.AppendLine($"                    <div class=\"stat-value\">{monthlyAverage:N0}</div>");
            sb.AppendLine($"                </div>");
            sb.AppendLine($"                <div class=\"stat-card\">");
            sb.AppendLine($"                    <div class=\"stat-label\">Categories</div>");
            sb.AppendLine($"                    <div class=\"stat-value\">{categories.Count}</div>");
            sb.AppendLine($"                </div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"chart-container\">");
            sb.AppendLine("            <h2>Monthly Rolling Average by Category</h2>");
            sb.AppendLine($"            <img src=\"data:image/png;base64,{chartBase64}\" alt=\"Cashflow Chart\" />");
            sb.AppendLine("        </div>");

            sb.AppendLine("        <div class=\"category-section\">");
            sb.AppendLine("            <h2>Category Details</h2>");

            foreach (var category in categories)
            {
                var categoryData = rollingAverageData.Where(t => t.Category == category).ToList();
                var categoryTotal = categoryData.Sum(t => t.Amount);
                var categoryMonthlyAverage = categoryTotal / ((endDate - startDate).Days / 30.0m);

                sb.AppendLine("            <details>");
                sb.AppendLine($"                <summary>{category} - Total: {categoryTotal:N0}</summary>");
                sb.AppendLine("                <div class=\"category-content\">");
                sb.AppendLine("                    <div class=\"stat-grid\">");
                sb.AppendLine($"                        <div class=\"stat-card\">");
                sb.AppendLine($"                            <div class=\"stat-label\">Total Amount</div>");
                sb.AppendLine($"                            <div class=\"stat-value\">{categoryTotal:N0}</div>");
                sb.AppendLine($"                        </div>");
                sb.AppendLine($"                        <div class=\"stat-card\">");
                sb.AppendLine($"                            <div class=\"stat-label\">Monthly Average</div>");
                sb.AppendLine($"                            <div class=\"stat-value\">{categoryMonthlyAverage:N0}</div>");
                sb.AppendLine($"                        </div>");
                sb.AppendLine($"                        <div class=\"stat-card\">");
                sb.AppendLine($"                            <div class=\"stat-label\">Percentage of Total</div>");
                sb.AppendLine($"                            <div class=\"stat-value\">{(categoryTotal / totalAmount * 100):N1}%</div>");
                sb.AppendLine($"                        </div>");
                sb.AppendLine("                    </div>");
                sb.AppendLine("                    <div class=\"chart-container\">");
                sb.AppendLine($"                        <img src=\"data:image/png;base64,{categoryCharts[category]}\" alt=\"{category} Chart\" />");
                sb.AppendLine("                    </div>");

                var categoryTransactions = transactions
                    .Where(t => t.Category == category)
                    .OrderByDescending(t => t.Amount)
                    .ToList();

                if (categoryTransactions.Any())
                {
                    sb.AppendLine("                    <h3>Transactions</h3>");
                    sb.AppendLine("                    <table>");
                    sb.AppendLine("                        <thead>");
                    sb.AppendLine("                            <tr>");
                    sb.AppendLine("                                <th style=\"min-width: 90px;\">Date</th>");
                    sb.AppendLine("                                <th>Recipient</th>");
                    sb.AppendLine("                                <th>Additional Info</th>");
                    sb.AppendLine("                                <th>Amount</th>");
                    sb.AppendLine("                            </tr>");
                    sb.AppendLine("                        </thead>");
                    sb.AppendLine("                        <tbody>");

                    foreach (var transaction in categoryTransactions)
                    {
                        sb.AppendLine("                            <tr>");
                        sb.AppendLine($"                                <td>{transaction.Date:yyyy-MM-dd}</td>");
                        sb.AppendLine($"                                <td>{transaction.Recipient}</td>");
                        sb.AppendLine($"                                <td>{transaction.AdditionalInfo}</td>");
                        sb.AppendLine($"                                <td class=\"amount-cell\">{transaction.Amount:N0}</td>");
                        sb.AppendLine("                            </tr>");
                    }

                    sb.AppendLine("                        </tbody>");
                    sb.AppendLine("                    </table>");
                }

                sb.AppendLine("                </div>");
                sb.AppendLine("            </details>");
            }

            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
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
