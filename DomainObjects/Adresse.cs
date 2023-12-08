using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Adresse
{
    [Key] public int Id { get; set; }

    public string Adresszeile1 { get; set; }
    
    public string Adresszeile2 { get; set; }
    
    public string Hausnummer { get; set; }
    
    public string Postleitzahl { get; set; }
    
    public string Ort { get; set; }
    
    public int InUse { get; set; }

    public void Update(Adresse other)
    {
        this.Adresszeile1 = Adresszeile1;
        this.Adresszeile2 = Adresszeile2;
        this.Hausnummer = Hausnummer;
        this.Postleitzahl = Postleitzahl;
        this.Ort = Ort;
    }

}