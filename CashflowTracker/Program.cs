using CashflowTracker.Services;
using System.Text;

string dataFolder = "../../../../data";
var transactions = await TransactionsCsvProcessor.ProcessFiles(dataFolder);

// Sort transactions by date descending
var sortedTransactions = transactions.OrderByDescending(t => t.Date).ToList();

Console.WriteLine($"Total transactions loaded: {sortedTransactions.Count}");

// Export to CSV
string outputPath = "C:/Users/boros/My Drive/transactions.csv";
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
{
    // Write header
    writer.WriteLine("ID,Date,Type,Recipient,Amount,Currency,Description,Source");

    // Write transactions
    foreach (var transaction in sortedTransactions)
    {
        var csvLine = $"\"{transaction.Id}\",\"{transaction.Date:yyyy-MM-dd}\",\"{transaction.Type}\",\"{transaction.Recipient}\",{transaction.Amount},\"{transaction.Currency}\",\"{transaction.AdditionalInfo}\",\"{transaction.Source}\"";
        writer.WriteLine(csvLine);
    }
}

Console.WriteLine($"Exported {sortedTransactions.Count} transactions to {outputPath}");