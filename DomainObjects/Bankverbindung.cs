using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Bankverbindung
{
    [Key] public string Iban { get; set; }
    public string Bic { get; set; }
    [Required] public string Kontoinhaber { get; set; }
    
    public int InUse { get; set; }
}