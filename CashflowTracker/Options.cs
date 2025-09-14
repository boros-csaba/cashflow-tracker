namespace CashflowTracker;

public static class Options
{
    public static readonly CsvOptions KH = new()
    {
        Delimiter = "\t",
        IdIndex = 1,
        DateIndex = 0,
        TypeIndex = 2,
        RecipientIndex = 6,
        AmountIndex = 7,
        CurrencyIndex = 8,
        AdditionalInfoIndex = 9
    };

    public static readonly CsvOptions Erste = new()
    {
        Delimiter = ",",
        IdIndex = 10,
        DateIndex = 2,
        TypeIndex = -1, // No direct type column
        RecipientIndex = 5,
        AmountIndex = 3,
        CurrencyIndex = 4,
        AdditionalInfoIndex = 9
    };

    public static readonly CsvOptions Wise = new()
    {
        Delimiter = ",",
        IdIndex = 0,
        DateIndex = 1,
        TypeIndex = 21, // Transaction Type
        RecipientIndex = 12, // Payee Name
        AmountIndex = 3,
        CurrencyIndex = 4,
        AdditionalInfoIndex = 5, // Description
        DateFormat = "dd-MM-yyyy"
    };
}
