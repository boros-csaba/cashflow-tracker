using System.Text;

namespace CashflowTracker.Services;

public static class TransactionsCsvProcessor
{
    public static async Task<List<Transaction>> ProcessFiles(string dataFolder)
    {
        string[] csvFiles = Directory.GetFiles(dataFolder, "*.csv");
        var allTransactions = new List<Transaction>();

        foreach (var file in csvFiles)
        {
            Console.WriteLine($"Processing file: {Path.GetFileName(file)}");

            using var reader = new StreamReader(file, Encoding.UTF8);
            var headerLine = (await reader.ReadLineAsync()) ?? "";

            var csvOptions = GetOptions(headerLine);

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

        return allTransactions;
    }

    private static CsvOptions GetOptions(string headerLine)
    {
        if (headerLine.StartsWith("könyvelés dátuma"))
        {
            return Options.KH;
        }
        else if (headerLine.StartsWith(@"""Felhasználónév""") || headerLine.Contains("Felhasználónév"))
        {
            return Options.Erste;
        }
        else if (headerLine.StartsWith(@"""TransferWise ID""") || headerLine.Contains("TransferWise ID"))
        {
            return Options.Wise;
        }
        throw new InvalidOperationException("Unknown CSV format");
    }
}
