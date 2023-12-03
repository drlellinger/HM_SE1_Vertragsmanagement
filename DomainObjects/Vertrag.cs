using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Vertrag
{
    public Vertrag()
    {
        Vertragsdauer = TimeSpan.FromTicks(ValidTo.Ticks) - TimeSpan.FromTicks(ValidFrom.Ticks);
    }

    [Key] public int Id { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    [Required] public DateTime ValidFrom { get; set; } = DateTime.Now;
    
    [Required] public int Revision { get; set; } = 1;

    public DateTime ValidTo { get; set; } = DateTime.MaxValue;

    public int Debitor { get; set; }

    public int Kreditor { get; set; }

    public Boolean IsActive { get; set; } = false;

    public decimal NormaleMonatsrate { get; set; }

    public decimal Anzahlung { get; set; } = 0;

    public DateTime ZeitpunktAnzahlung { get; set; } = DateTime.MinValue;

    public decimal Abschlussrate { get; set; } = 0;
    
    public DateTime ZeitpunktAbschlussrate { get; set; }
    
    public DateTime ZeitpunktLetzteAbbuchung { get; set; } = DateTime.MinValue;

    public decimal SummeNächsteAbbuchung { get; set; } = 0;

    public DateTime ZeitpunktNächsteAbbuchung { get; set; }
    
    public TimeSpan Vertragsdauer { get; } 
    public decimal Vertragswert { get; set; } //Jeder Vertrag hat einen Wert

    [Required] public string Währung { get; set; } //Jeder Vertrag kann nur mit der Währung bezahlt werden, mit der er in der Datenbank hinterlegt ist 
    
    
}
