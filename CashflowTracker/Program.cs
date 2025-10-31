using CashflowTracker.Services;

string dataFolder = "../../../../data";
var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);
transactions = transactions.OrderByDescending(t => t.Date).ToList();

Console.WriteLine($"Total transactions loaded: {transactions.Count}");

transactions = TransferFilter.RemoveInternalTransfers(transactions);
Console.WriteLine($"Transactions after removing internal transfers: {transactions.Count}");

