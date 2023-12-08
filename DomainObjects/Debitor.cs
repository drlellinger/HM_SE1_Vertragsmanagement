using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Debitor
{
    [Key] public int Id{ get; set; }
    
    [Required]public string Name { get; set; }

    public string Vorname { get; set; }
    
    public int Adresse { get; set; }
    
    [Required] public string Bankverbindung { get; set; }

    [Required] public Boolean IsCustomer { get; set; } = false;
}