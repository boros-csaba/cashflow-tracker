namespace CashflowTracker;

public static class Options
{
    public static readonly CsvOptions KH = new()
    { 
        Delimiter = "\t" 
    };

    public static readonly CsvOptions Erste = new()
    { 
        Delimiter = ",",
        IdIndex = 10,
        DateIndex = 2,
        AmountIndex = 3,
       

        /*
         * public int IdIndex { get; set; }
    public int DateIndex { get; set; }
    public int TypeIndex { get; set; }
    public int RecipientIndex { get; set; }
    public int AmountIndex { get; set; }
    public int CurrencyIndex { get; set; }
    public int AdditionalInfoIndex { get; set; }
    public int SourceIndex { get; set; }
         */
    };
}
