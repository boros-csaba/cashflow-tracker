public class CsvOptions
{
    public string Delimiter { get; set; }
    public int IdIndex { get; set; }
    public int DateIndex { get; set; }
    public int TypeIndex { get; set; }
    public int RecipientIndex { get; set; }
    public int AmountIndex { get; set; }
    public int CurrencyIndex { get; set; }
    public int AdditionalInfoIndex { get; set; }
    public int SourceIndex { get; set; }
    public string DateFormat { get; set; } = "";
}