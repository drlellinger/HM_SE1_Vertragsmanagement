using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Vertragsmanagement.DataAccess;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.Controllers;

[ApiController]
[Route("~/vertraege")]
public class VerträgeController : ControllerBase
{
    private DatabaseContext DatabaseContext { get; }
    public VerträgeController(DatabaseContext dbc)
    {
        this.DatabaseContext = dbc;
    }

    
    /// <summary>
    /// Gibt alle Verträge aus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<Vertrag[]> GetAllVerträge()
    {
        var allVerträge = DatabaseContext.Verträge.ToArray();
        return Ok(allVerträge);
    }
    
    
    /// <summary>
    /// Gibt einen Vertrag anhand der Vertrags-Nummer aus
    /// </summary>
    /// <param name="vertragId">Vertragsnummer</param>
    /// <returns></returns>
    [HttpGet("{vertragId}")]
    public ActionResult<Vertrag> GetVertragById([FromRoute] int vertragId)
    {
        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragId);
        if (vertrag == null) return NotFound();
        return Ok(vertrag);
    }
    
    /// <summary>
    /// Gibt zurück, ob eine Kaufoption besteht
    /// </summary>
    /// <param name="vertragId">Vertragsnummer</param>
    /// <returns></returns>
    [HttpGet("{vertragId}/kaufoption")]
    public ActionResult<bool> CheckKaufoption([FromRoute] int vertragId)
    {
        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragId);
        if (vertrag == null) return NotFound();

        if (vertrag.Vertragswert <= vertrag.Abschlussrate)
        {
            return Ok(true);
        }
        else
        {
            return BadRequest(false);
        }
    }
    
    
    /// <summary>
    /// Fügt Vertrag hinzu
    /// </summary>
    /// <param name="vertrag">Vertrag</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddVertrag([FromBody] Vertrag vertrag)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        vertrag.Revision = 1;
        DatabaseContext.Verträge.Add(vertrag);
        DatabaseContext.SaveChanges();
        System.Console.WriteLine(vertrag + "was added.");
        return Ok();
    }
    
    
    /// <summary>
    /// Ändert den Vertrag mithilfe der selbst angegebenen Parameter
    /// </summary>
    /// <param name="vertrag">Vertrag</param>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateVertrag([FromBody] Vertrag vertrag)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (vertrag.Id == 0) return BadRequest("Id of Vertrag is required when updating a Vertrag");

        var dbVertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertrag.Id);
        if (dbVertrag == null) return NotFound("Vertrag " + vertrag.Id + "existiert nicht");

        dbVertrag.Update(vertrag); //Methode in Vertrag_DomainObject implementiert
        
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    /// <summary>
    /// Bezahlt die Anzahlung
    /// </summary>
    /// <param name="vertragsId">Vertragsnummer</param>
    /// <param name="pay">Summe eingehende Zahlung</param>
    /// <param name="cur">Währung der eingehenden Zahlung</param>
    /// <returns></returns>
    [HttpPut("payAnzahlung")]
    public ActionResult PayAnzahlung([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return BadRequest("Vertrag not found");
        }
        
        if (DatabaseContext.Currencys.Any(c => c.Id == cur) is false)
        {
            return StatusCode(501);
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }

        var vertragsänderung = DatabaseContext.Verträge.Where(v =>
                v.Id == vertragsId  && v.Währung == cur
            )
            .OrderBy(v => v.ZeitpunktNächsteAbbuchung)
            .ToArray();

        if (vertragsänderung.Length == 0)
        {
            return BadRequest("Vertrag is not available");
        }

        var v = vertragsänderung[0];
        if (pay == v.Anzahlung)
        {
            v.Anzahlung = 0;
            v.ZeitpunktAnzahlung = DateTime.Now;
        }
        else if (pay < v.Anzahlung)
        {
            v.Anzahlung = v.Anzahlung - pay;
        }
        else if (pay > v.Anzahlung)
        {
            return BadRequest("Too much Anzahlung. Abort.");
        }

        v.Vertragswert = v.Vertragswert - pay;
        
        DatabaseContext.SaveChanges();

        return Ok();
    }
    
    /// <summary>
    /// Bezahlt die Abschlussrate.
    /// </summary>
    /// <param name="vertragsId">Vertragsnummer</param>
    /// <param name="pay">Summe eingehende Zahlung</param>
    /// <param name="cur">Währung der eingehenden Zahlung</param>
    /// <returns></returns>
    [HttpPut("payAbschlussrate")]
    public ActionResult PayAbschlussrate([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }
        
        if (DatabaseContext.Currencys.Any(c => c.Id == cur) is false)
        {
            return StatusCode(501);
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }

        var vertragsänderung = DatabaseContext.Verträge.Where(v =>
                v.Id == vertragsId && v.Währung == cur
            )
            .OrderBy(v => v.ZeitpunktNächsteAbbuchung)
            .ToArray();

        if (vertragsänderung.Length == 0)
        {
            return BadRequest("Vertrag is not available");
        }

        var v = vertragsänderung[0];

        if (DateTime.Now.AddMonths(1) < v.ValidTo)
        {
            return BadRequest("Abschlussrate is not yet available");
        }

        string notice = null;
        
        if (pay == v.Abschlussrate)
        {
            v.Abschlussrate = 0;
            v.ZeitpunktAbschlussrate = DateTime.Now;
            notice = "Abschlussrate vollständig bezahlt.";
        }
        else if (pay < v.Abschlussrate)
        {
            v.Abschlussrate = v.Abschlussrate - pay;
            notice = "Abschlussrate nicht bezahlt. Es fehlen " + v.Abschlussrate + v.Währung;
        }
        else if (pay > v.Abschlussrate)
        {
            return BadRequest("Too much Anzahlung. Abort.");
        }
        
        v.Vertragswert = v.Vertragswert - pay;
        
        DatabaseContext.SaveChanges();

        return Ok(notice);
    }
    
    
    /// <summary>
    /// Bezahlt die Monatsrate
    /// </summary>
    /// <param name="vertragsId">Vertragsnummer</param>
    /// <param name="pay">Summe eingehende Zahlung</param>
    /// <param name="cur">Währung der eingehenden Zahlung</param>
    /// <param name="regular">Gibt an, ob die Monatsrate regulär (also über Lastschrifteinzug und pünktlich) überwiesen wurde und somit der Zeitpunkt für die nächste reguläre Abbuchung einen Monat später angesetzt wird</param>
    /// <returns></returns>
    [HttpPut("payMonatsrate")]
    public ActionResult PayMonatsrate([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur, [FromQuery] Boolean regular)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }
        
        if (DatabaseContext.Currencys.Any(c => c.Id == cur) is false)
        {
            return StatusCode(501);
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }

        var vertragsänderung = DatabaseContext.Verträge.Where(v =>
                v.Id == vertragsId  && v.Währung == cur
            )
            .OrderBy(v => v.ZeitpunktNächsteAbbuchung)
            .ToArray();

        if (vertragsänderung.Length == 0)
        {
            return BadRequest("Vertrag is not available");
        }

        var v = vertragsänderung[0];
        v.SummeNächsteAbbuchung = v.SummeNächsteAbbuchung - pay;
        v.ZeitpunktLetzteAbbuchung = DateTime.Now;
        if (regular)
        {
            v.ZeitpunktNächsteAbbuchung = DateTime.Now.AddMonths(1);
        }
        
        v.Vertragswert = v.Vertragswert - pay;
        
        DatabaseContext.SaveChanges();

        return Ok(pay + " " + v.Währung + " was paid. Restwert: " + v.Vertragswert + cur + " Next payment due on " + v.ZeitpunktNächsteAbbuchung);
    }
    
    
    /// <summary>
    /// Setzt die abzubuchende Summe im Vertrag
    /// </summary>
    /// <param name="vertragsId">Vertragsnummer</param>
    /// <returns></returns>
    [HttpPut("setNextBalance")]
    public ActionResult SetNextBalance([FromQuery][Required] int vertragsId)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return NotFound("Vertrag " + vertragsId + " not found");
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return NotFound("Vertrag not found");
        }

        var vertragsänderung = DatabaseContext.Verträge.Where(v =>
                v.Id == vertragsId
            )
            .OrderBy(v => v.ZeitpunktNächsteAbbuchung)
            .ToArray();

        if (vertragsänderung.Length == 0)
        {
            return BadRequest("Vertrag is not available");
        }

        var v = vertragsänderung[0];
        v.SummeNächsteAbbuchung = v.SummeNächsteAbbuchung + v.NormaleMonatsrate;
        
        DatabaseContext.SaveChanges();

        return Ok();
    }
    
}