
using System.Globalization;

namespace CashflowTracker.Services;

public static class Categorizer
{
    const string Ignore = "Ignore";

    public static List<Transaction> Categorize(List<Transaction> transactions)
    {
        foreach (var transaction in transactions!)
        {
            transaction.Category = GetTransactionCategory(transaction);
            


        }
        return transactions.Where(c => c.Category != Ignore).ToList();
    }

    private static string GetTransactionCategory(Transaction transaction)
    {
        foreach (var category in Keywords)
        {
            foreach (var keyword in category.Value)
            {
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(transaction.Recipient, keyword, CompareOptions.IgnoreCase) >= 0)
                {
                    return category.Key;
                }
                if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(transaction.AdditionalInfo, keyword, CompareOptions.IgnoreCase) >= 0)
                {
                    return category.Key;
                }
            }
        }

        if (transaction.Recipient == "NAV" ||
            transaction.Recipient == "OTPMOBL nav" ||
            transaction.Recipient == "Fővárosi Önkormányzat" ||
            transaction.Recipient == "SIMPLEP nav" ||
            transaction.Recipient == "OTPMOBL webkincstar" ||
            transaction.Recipient == "NAV Személyi jövedelemadó" ||
            transaction.AdditionalInfo == "Converted USD to HUF" ||
            transaction.AdditionalInfo.Contains("Received money from Boros Csaba E.V. with reference"))
        {
            return Ignore;
        }

        if (transaction.Type.Contains("Készpénzfelvét"))
        {
            return Transaction.KeszpenzFelvetel;
        }

        return transaction.Category;
    }

    private static Dictionary<string, List<string>> Keywords = new Dictionary<string, List<string>>
    {
        { Transaction.Bevasarlas, ["auchan", "tesco", "aldi", "spar", "LIDL", "COOP", "ECOFAMILY", "CBA SVA", "Penny "] },
        { Transaction.OrvosGyogyszer, ["gyogyszertar", "GYOGYSZE", "PATIKA", "Tritonlife", "AMALGAM STUDIO BT", "PIRINYO-SZEMESZET", "AMBULANCIA", "DENTAL", "DENTALKOPE", "OPTIKA", "Vitamin Porta", "BENU APOLLO", "futunatura", "Viola Optimum", "EGE SZSE GKO ZP", "FOGASZAT"] },
        { Transaction.Etterem, ["etterem", "foodora", "Bumerang Bufe", "PIZZA", "Étterem", "Kávéház", "VENDEGLO", "RESTAURANT", "CITYFOOD", "BISZTRO", "Klarissza", "Fresh Corner", "MCD ", "KEBAB"] },
        { Transaction.HazKert, ["Vedanta Home", "Sun-Set Shading", "Gönczi Dániel"] },
        { Transaction.HitelTorleszto, ["Hitel kamat törl. ütem szerint"] },
        { Transaction.HotelRepulo, ["RYANAIR", "BOOKING.COM", "HOTEL", "Centrum Swiatla", "SAN SIMON", "Portobello Wellness", "PANZIO"] },
        { Transaction.Ruha, ["H&M", "Decathlon", "HM Hennes", "C&A", "RESERVED", "TornaDora", "sin-say", "Deichmann", "ecipo.hu", "ABOUT YOU"] },
        { Transaction.Elektronika, ["Alza", "Media Markt", "Euronics", "Extreme Digital", "edigital.hu", "MEDIAMARKT", "Philips", "DIGIPRIME.HU"]  },
        { Transaction.ButorHaztartas, ["IKEA", "JYSK", "KIKA", "Butor", "Haztartas", "Lakberendezes", "XXXLUTZ", "Lakberendezés", "PEPCO"] },
    };
}
