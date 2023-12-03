using System.Numerics;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.DataAccess;

public class TestDataCreator
{
    public static void InitTestData(DatabaseContext context)
    {
        //Währungen initialisieren (ISO-4217)
        context.Currencys.Add(new() { Id = "EUR", Name = "Euro" });
        context.Currencys.Add(new() { Id = "USD", Name = "US-Dollar" });
        context.Currencys.Add(new() { Id = "CHF", Name = "Schweizer Franken" });
        context.Currencys.Add(new() { Id = "GBP", Name = "Pfund Sterling" });
        context.Currencys.Add(new() { Id = "ATS", Name = "Österreichischer Schilling" });
        context.Currencys.Add(new() { Id = "DDM", Name = "Mark der DDR" });
        context.Currencys.Add(new() { Id = "DEM", Name = "Deutsche Mark" });
        context.Currencys.Add(new() { Id = "RUB", Name = "Russischer Rubel" });
        
        context.Bankverbindungen.Add(new()
            { Iban = "DE000000000000000000000000", Bic = "DEGENOSOOS", Kontoinhaber = "Alan Turing" });
        context.Bankverbindungen.Add(new()
            { Iban = "DE000000000000000000020000", Bic = "DEGENOSAAS", Kontoinhaber = "Bill Gates" });

        context.Adressen.Add(new()
            { Id = 1, Adresszeile1 = "Brückenstraße", Hausnummer = "42", Postleitzahl = "80000", Ort = "Manchester" });
        
        
        context.Kreditoren.Add(new() { Id=1, Name = " Turing", Vorname = "Alan", IsCustomer = true, Bankverbindung = "DE000000000000000000000000", Adresse = 1});
        context.Debitoren.Add(new() { Id=1, Name = "Gates" , Vorname = "Bill", IsCustomer = false, Bankverbindung = "DE000000000000000000020000"});

        context.Verträge.Add(new() { Id = 1, Kreditor = 1, Vertragswert = 20000, Währung = "EUR", IsActive = true, ValidFrom = DateTime.Now.AddMonths(-1), ValidTo = DateTime.Now.AddYears(1), NormaleMonatsrate = 750, Anzahlung = 1000, ZeitpunktNächsteAbbuchung = DateTime.Now.AddDays(14)});
        context.Verträge.Add(new() { Id = 2, Debitor = 1, Vertragswert = 200000, Währung = "EUR"});
      
        //Änderungen in Datenbank speichern
        context.SaveChanges();
    }
}