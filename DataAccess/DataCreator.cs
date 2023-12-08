using System.IO.Enumeration;
using System.Numerics;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.DataAccess;

public class DataCreator
{
    /// <summary>
    /// Fügt Test-Datensätze zur InMemory-Datenbank hinzu
    /// </summary>
    /// <param name="context"></param>
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
            { Iban = "DE24500105173764984632", Bic = "GENODE51AGR", Kontoinhaber = "Alan Turing", InUse = 1});
        context.Bankverbindungen.Add(new()
            { Iban = "DE63500105172644736168", Bic = "ANHODE77", Kontoinhaber = "Bill Gates", InUse = 1});
        context.Bankverbindungen.Add(new()
            { Iban = "DE46500105178843196233", Bic = "GENODEF1HH2", Kontoinhaber = "Olaf Scholz"});

        context.Adressen.Add(new()
            { Id = 1, Adresszeile1 = "Brückenstraße", Hausnummer = "42", Postleitzahl = "80000", Ort = "München", InUse = 1});
        context.Adressen.Add(new()
            { Id = 2, Adresszeile1 = "Treppenweg", Hausnummer = "69", Postleitzahl = "30000", Ort = "Hamburg"});
        context.Adressen.Add(new()
            { Id = 3, Adresszeile1 = "Holzweg", Hausnummer = "3", Postleitzahl = "70000", Ort = "Stuttgart"});

        
        
        context.Kreditoren.Add(new() { Id=1, Name = " Turing", Vorname = "Alan", IsCustomer = false, Bankverbindung = "DE24500105173764984632", Adresse = 1});
        context.Debitoren.Add(new() { Id=1, Name = "Gates" , Vorname = "Bill", IsCustomer = true, Bankverbindung = "DE63500105172644736168"});

        context.Verträge.Add(new() { Id = 1, Kreditor = 1, Debitor = 1,  Währung = "EUR", IsActive = true, Vertragswert = 15000, NormaleMonatsrate = 1000, Anzahlung = 1000, Abschlussrate = 2000, ZeitpunktNächsteAbbuchung = DateTime.Now.AddDays(14), ValidFrom = DateTime.Now.AddMonths(-1), ValidTo = DateTime.Now.AddYears(1), });
        context.Verträge.Add(new() { Id = 2, Kreditor = 1, Vertragswert = 200000, Währung = "EUR", IsActive = false});
        
        for (int i = 0; i < context.Kreditoren.Count(); i++)
        {
            var adresseInK = context.Kreditoren.Where(k => k.Adresse == i).ToString();
            System.Console.WriteLine("Test");
        }
        
        //Änderungen in Datenbank speichern
        context.SaveChanges();
    }
    
}