using CashflowTracker;
using System.Text;

string dataFolder = "../../../../data";
string[] csvFiles = Directory.GetFiles(dataFolder, "*.csv");
var allTransactions = new List<Transaction>();

foreach (var file in csvFiles)
{
    Console.WriteLine($"Processing file: {Path.GetFileName(file)}");

    using var reader = new StreamReader(file, Encoding.UTF8);
    var headerLine = (await reader.ReadLineAsync()) ?? "";
    CsvOptions csvOptions;

    if (headerLine.StartsWith("könyvelés dátuma"))
    {
        csvOptions = Options.KH;
    }
    else if (headerLine.StartsWith(@"""Felhasználónév""") || headerLine.Contains("Felhasználónév"))
    {
        csvOptions = Options.Erste;
    }
    else if (headerLine.StartsWith(@"""TransferWise ID""") || headerLine.Contains("TransferWise ID"))
    {
        csvOptions = Options.Wise;
    }
    else
    {
        Console.WriteLine($"Unknown CSV format in file: {Path.GetFileName(file)}");
        Console.WriteLine($"Header: {headerLine}");
        continue;
    }

    while (!reader.EndOfStream)
    {
        var line = (await reader.ReadLineAsync()) ?? "";
        if (string.IsNullOrWhiteSpace(line)) continue;

        var values = line.Split(csvOptions.Delimiter, StringSplitOptions.None);

        if (values.Length <= Math.Max(csvOptions.DateIndex, csvOptions.AmountIndex)) continue;

        var amountRaw = values[csvOptions.AmountIndex];
        var amountValue = amountRaw.Trim('"')
            .Replace(" ", "")
            .Replace(" ", "")
            .Replace(",", ".");

        var dateRaw = values[csvOptions.DateIndex];
        var dateValue = dateRaw.Trim('"');

        DateTime parsedDate;
        if (!string.IsNullOrEmpty(csvOptions.DateFormat))
        {
            parsedDate = DateTime.ParseExact(dateValue, csvOptions.DateFormat, null);
        }
        else
        {
            dateValue = dateValue.Replace(".", "-");
            parsedDate = DateTime.Parse(dateValue);
        }

        var id = values[csvOptions.IdIndex].Trim('"');
        if (string.IsNullOrEmpty(id))
        {
            id = $"{parsedDate:yyyyMMdd}_{Math.Abs(amountValue.GetHashCode())}_{values[csvOptions.RecipientIndex < values.Length ? csvOptions.RecipientIndex : 0].Trim('"').GetHashCode()}";
        }

        var transaction = new Transaction
        {
            Id = id,
            Date = parsedDate,
            Type = csvOptions.TypeIndex >= 0 && csvOptions.TypeIndex < values.Length ? values[csvOptions.TypeIndex].Trim('"') : "",
            Recipient = csvOptions.RecipientIndex < values.Length ? values[csvOptions.RecipientIndex].Trim('"') : "",
            Amount = decimal.Parse(amountValue),
            Currency = csvOptions.CurrencyIndex < values.Length ? values[csvOptions.CurrencyIndex].Trim('"') : "",
            AdditionalInfo = csvOptions.AdditionalInfoIndex < values.Length ? values[csvOptions.AdditionalInfoIndex].Trim('"') : "",
            Source = csvOptions == Options.KH ? "kh" : csvOptions == Options.Erste ? "erste" : "wise"
        };

        allTransactions.Add(transaction);
        Console.WriteLine($"ID: {transaction.Id}, Date: {transaction.Date:yyyy-MM-dd}, Type: {transaction.Type}, Recipient: {transaction.Recipient}, Amount: {transaction.Amount} {transaction.Currency}, Source: {transaction.Source}");
    }

    Console.WriteLine();
}

// Sort transactions by date descending
var sortedTransactions = allTransactions.OrderByDescending(t => t.Date).ToList();

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