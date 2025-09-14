
using CashflowTracker;

string dataFolder = "../../../../data";
string[] csvFiles = Directory.GetFiles(dataFolder, "*.csv");



foreach (var file in csvFiles)
{
    using var reader = new StreamReader(file);
    var headerLine = (await reader.ReadLineAsync()) ?? "";
    CsvOptions csvOptions;
    if (headerLine.StartsWith("könyvelés dátuma"))
    {
        csvOptions = Options.KH;
    }
    else if (headerLine.StartsWith(@"""Felhasználónév"""))
    {
        csvOptions = Options.Erste;
    }
    else throw new ArgumentException("Unknown CSV format");

    while (!reader.EndOfStream)
    {
        var line = (await reader.ReadLineAsync()) ?? "";
        var values = line.Split(csvOptions.Delimiter, StringSplitOptions.TrimEntries);

        var amountRaw = values[csvOptions.AmountIndex];
        var amountValue = amountRaw.Trim('"')
            .Replace(" ", "")
            .Replace(",", ".");

        var dateRaw = values[csvOptions.DateIndex];
        var dateValue = dateRaw.Trim('"')
            .Replace(".", "-");

        var transaction = new Transaction
        {
            Id = values[csvOptions.IdIndex],
            Date = DateTime.Parse(dateValue),
            /*Type = values[2],
            Recipient = values[3],*/
            Amount = decimal.Parse(amountValue),
            //Currency = values[5],
            //AdditionalInfo = values[6],
            //Source = Path.GetFileName(file)
        };
        Console.WriteLine($"Transaction ID: {transaction.Id}, Date: {transaction.Date}, Type: {transaction.Type}, Recipient: {transaction.Recipient}, Amount: {transaction.Amount} {transaction.Currency}, Source: {transaction.Source}, {transaction.AdditionalInfo}");
    }
}