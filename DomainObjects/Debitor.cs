using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Debitor
{
    [Key] public int Id{ get; set; }
    
    [Required]public string Name { get; set; }

    public string Vorname { get; set; }
    
    public int Adresse { get; set; }
    
    [Required] public string Bankverbindung { get; set; }

    [Required] public bool IsCustomer { get; set; } = true;
    
    public void Update(Debitor other)
    {
        this.Name = Name;
        this.Vorname = Vorname;
        this.Adresse = Adresse;
        this.Bankverbindung = Bankverbindung;
        this.IsCustomer = IsCustomer;
    }
}