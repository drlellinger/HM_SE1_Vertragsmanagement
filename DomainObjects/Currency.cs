using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Currency
{
    [Key]public String Id { get; set; }

    [Required]public string Name { get; set; }
    
}