using System.Security.Cryptography;
using System.Text;

namespace CashflowTracker.Services;

public static class TransactionsCsvProcessor
{
    public static async Task<List<Transaction>> ProcessFiles(string dataFolder)
    {
        string[] csvFiles = Directory.GetFiles(dataFolder, "*.csv");
        var allTransactions = new List<Transaction>();
        var usedIds = new HashSet<string>();

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
                var source = csvOptions == Options.KH ? "kh" : csvOptions == Options.Erste ? "erste" : "wise";
                var type = csvOptions.TypeIndex >= 0 && csvOptions.TypeIndex < values.Length ? values[csvOptions.TypeIndex].Trim('"') : "";
                var recipient = csvOptions.RecipientIndex < values.Length ? values[csvOptions.RecipientIndex].Trim('"') : "";
                var amount = decimal.Parse(amountValue);
                var currency = csvOptions.CurrencyIndex < values.Length ? values[csvOptions.CurrencyIndex].Trim('"') : "";
                var additionalInfo = csvOptions.AdditionalInfoIndex < values.Length ? values[csvOptions.AdditionalInfoIndex].Trim('"') : "";

                if (string.IsNullOrEmpty(id))
                {
                    id = GenerateUniqueId(parsedDate, amount, recipient, type, additionalInfo, source, usedIds);
                }
                else
                {
                    id = EnsureUniqueId(id, usedIds);
                }

                var transaction = new Transaction
                {
                    Id = id,
                    Date = parsedDate,
                    Type = type,
                    Recipient = recipient,
                    Amount = amount,
                    Currency = currency,
                    AdditionalInfo = additionalInfo,
                    Source = source
                };
                if (csvOptions.IsNegativeAmount)
                {
                    transaction.Amount = -amount;
                }

                allTransactions.Add(transaction);
                Console.WriteLine($"ID: {transaction.Id}, Date: {transaction.Date:yyyy-MM-dd}, Type: {transaction.Type}, Recipient: {transaction.Recipient}, Amount: {transaction.Amount} {transaction.Currency}, Source: {transaction.Source}");
            }

            Console.WriteLine();
        }

        AssertNoDuplicateIds(allTransactions);

        return allTransactions;
    }

    private static string GenerateUniqueId(DateTime date, decimal amount, string recipient, string type, string additionalInfo, string source, HashSet<string> usedIds)
    {
        var data = $"{date:yyyyMMddHHmmss}|{amount}|{recipient}|{type}|{additionalInfo}|{source}";
        var hash = ComputeSha256Hash(data);
        var baseId = hash[..16];

        return EnsureUniqueId(baseId, usedIds);
    }

    private static string EnsureUniqueId(string baseId, HashSet<string> usedIds)
    {
        var id = baseId;
        var counter = 1;

        while (usedIds.Contains(id))
        {
            id = $"{baseId}_{counter}";
            counter++;
        }

        usedIds.Add(id);
        return id;
    }

    private static string ComputeSha256Hash(string data)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static void AssertNoDuplicateIds(List<Transaction> transactions)
    {
        var ids = transactions.Select(t => t.Id).ToList();
        var duplicates = ids.GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException($"Duplicate transaction IDs found: {string.Join(", ", duplicates)}");
        }
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
