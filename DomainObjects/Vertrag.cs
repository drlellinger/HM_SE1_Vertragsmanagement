using System.ComponentModel.DataAnnotations;

namespace Vertragsmanagement.DomainObjects;

public class Vertrag
{
   public void Update(Vertrag other)
    {
        this.Revision = Revision++;
        this.IsActive = IsActive;
        this.Abschlussrate = Abschlussrate;
        this.Anzahlung = Anzahlung;
        this.NormaleMonatsrate = NormaleMonatsrate;
        this.Author = Author;
        this.Title = Title;
        this.Debitor = Debitor;
        this.Kreditor = Kreditor;
        this.Währung = Währung;
        this.ValidFrom = ValidFrom;
        this.ValidTo = ValidTo;
        this.Vertragsdauer = TimeSpan.FromTicks(ValidTo.Ticks) - TimeSpan.FromTicks(ValidFrom.Ticks);
        
    }

    [Key] public int Id { get; set; }

    public string Title { get; set; }

    public string Author { get; set; }

    [Required] public DateTime ValidFrom { get; set; } = DateTime.Now;
    
    public int Revision { get; set; } 

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
    
    public TimeSpan Vertragsdauer { get; set; } 
    public decimal Vertragswert { get; set; } //Jeder Vertrag hat einen Wert

    [Required] public string Währung { get; set; } //Jeder Vertrag kann nur mit der Währung bezahlt werden, mit der er in der Datenbank hinterlegt ist 
    
    
}
