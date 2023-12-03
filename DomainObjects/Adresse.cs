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

    

}