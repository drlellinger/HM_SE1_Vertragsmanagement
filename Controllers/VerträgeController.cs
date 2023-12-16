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
    /// <response code="200">Success. Return Vertrag.</response>
    /// <response code="404">Not found.</response>
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
    /// <response code="200">Success. Return true.</response>
    /// <response code="400">Bad request. Return false.</response>
    /// <response code="404">Not found</response>
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
    /// Gibt den Kreditor eines Vertrags aus
    /// </summary>
    /// <param name="vertragId"></param>
    /// <response code="200">Success. Return Kreditor.</response>
    /// <response code="404">Not found.</response>
    /// <returns></returns>
    [HttpGet("{vertragId}/kreditor")]
    public ActionResult<Kreditor> GetKreditorFromVertrag([FromRoute] int vertragId)
    {
        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragId);
        if (vertrag == null) return NotFound();

        var kreditorId = vertrag.Kreditor;
        var kreditor = DatabaseContext.Kreditoren.FirstOrDefault(k => k.Id == kreditorId);
        if (kreditor == null) return StatusCode(500);
        
        return Ok(kreditor);
    }
    
    /// <summary>
    /// Gibt den Debitor eines Vertrags aus
    /// </summary>
    /// <param name="vertragId"></param>
    /// <response code="200">Success. Return Debitor.</response>
    /// <response code="404">Not found.</response>
    /// <returns></returns>
    [HttpGet("{vertragId}/debitor")]
    public ActionResult<Debitor> GetDebitorFromVertrag([FromRoute] int vertragId)
    {
        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragId);
        if (vertrag == null) return NotFound();

        var debitorId = vertrag.Debitor;
        var debitor = DatabaseContext.Debitoren.FirstOrDefault(d => d.Id == debitorId);
        if (debitor == null) return StatusCode(500);
        
        return Ok(debitor);
    }
    
    
    /// <summary>
    /// Fügt Vertrag hinzu
    /// </summary>
    /// <param name="vertrag">Vertrag</param>
    /// <response code="400">Bad request. Returns response body with error.</response>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddVertrag([FromBody] Vertrag vertrag)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        vertrag.Revision = 1;
        DatabaseContext.Verträge.Add(vertrag);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    
    /// <summary>
    /// Ändert den Vertrag mithilfe der selbst angegebenen Parameter
    /// </summary>
    /// <param name="vertrag">Vertrag</param>
    /// <response code="400">Bad request. Returns response body with error.</response>
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
    /// <param name="force">Force-Parameter</param>
    /// <response code="400">Bad request.</response>
    /// <response code="403">Forbidden. Negative payment is forbidden.</response>
    /// <response code="404">Not found.</response>
    /// <response code="501">Not implemented.</response>
    /// <returns></returns>
    [HttpPut("payAnzahlung")]
    public ActionResult PayAnzahlung([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur, [FromQuery]bool force)
    {
        if (pay < 0 && force == false) return StatusCode(403, "Negative payment is forbidden.");
        
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return NotFound("Vertrag not found");
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
            if (force)
            {
                v.Anzahlung = 0;
                v.ZeitpunktAnzahlung = DateTime.Now;
            }
            else
            {
                return BadRequest("Too much Anzahlung. Abort.");
            }
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
    /// <param name="force">Force-Parameter</param>
    /// <response code="403">Forbidden. Negative payment is forbidden.</response>
    /// <response code="404">Not found.</response>
    /// <response code="501">Not implemented.</response>
    /// <returns></returns>
    [HttpPut("payAbschlussrate")]
    public ActionResult PayAbschlussrate([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur, [FromQuery]bool force)
    {
        if (pay < 0 && force == false) return StatusCode(403, "Negative payment is forbidden");
        
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
    /// <param name="force">Force-Parameter</param>
    /// <response code="403">Forbidden. Negative payment is forbidden.</response>
    /// <response code="404">Not found.</response>
    /// <response code="501">Not implemented.</response>
    /// <returns></returns>
    [HttpPut("payMonatsrate")]
    public ActionResult PayMonatsrate([FromQuery][Required] int vertragsId, [FromQuery][Required] decimal pay, [FromQuery][Required] string cur, [FromQuery] Boolean regular, [FromQuery]bool force)
    {
        if (pay < 0 && force == false) return StatusCode(403, "Negative payment is forbidden");
        
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
    /// <response code="404">Not found.</response>
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