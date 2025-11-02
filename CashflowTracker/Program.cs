using CashflowTracker.Services;
using System.Text;

string dataFolder = "../../../../data";
var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);
transactions = transactions.OrderByDescending(t => t.Date).ToList();

Console.WriteLine($"Total transactions loaded: {transactions.Count}");

transactions = TransferFilter.RemoveInternalTransfers(transactions);
Console.WriteLine($"Transactions after removing internal transfers: {transactions.Count}");

Categorizer.Categorize(transactions);

var output = new StringBuilder();
output.AppendLine("Month\tDate\tId\tAmount\tCategory");
foreach (var transaction in transactions)
{
    var date = new DateOnly(transaction.Date.Year, transaction.Date.Month, 1);
    output.AppendLine($"{date:yyyy-MM}\t{transaction.Date:yyyy-MM-dd}\t{transaction.Id}\t{transaction.Amount}\t{transaction.Category}");
}
var x = output.ToString();
var xx = 0;