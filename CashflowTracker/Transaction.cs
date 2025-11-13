namespace CashflowTracker;

public class Transaction
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
    public string Recipient { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string AdditionalInfo { get; set; }
    public string Source { get; set; }
    public string Category { get; set; } = "-";


    public static string HazKert = "Ház, kert";
    public static string Bevasarlas = "Bevásárlás";
    public static string OrvosGyogyszer = "Orvos, gyógyszer";
    public static string Etterem = "Étterem";
    public static string KeszpenzFelvetel = "Készpénz felvétel";
    public static string Egyeb = "Egyéb";
}


