using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet]
    public ActionResult<Vertrag[]> GetAllVerträge()
    {
        var allVerträge = DatabaseContext.Verträge.ToArray();
        return Ok(allVerträge);
    }
    
    [HttpGet("{vertragId}")]
    public ActionResult<Vertrag> GetVertragById([FromRoute] int vertragId)
    {
        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragId);
        if (vertrag == null) return NotFound();
        return Ok(vertrag);
    }
    
    [HttpPost]
    public ActionResult AddVertrag([FromBody] Vertrag vertrag)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        DatabaseContext.Verträge.Add(vertrag);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    [HttpPut("payAnzahlung")]
    public ActionResult PayAnzahlung([FromQuery] int vertragsId, [FromQuery] decimal pay)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return BadRequest("Vertrag not found");
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return BadRequest("Vertrag not found");
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
        
        DatabaseContext.SaveChanges();

        return Ok();
    }
    
    [HttpPut("payAbschlussrate")]
    public ActionResult PayAbschlussrate([FromQuery] int vertragsId, [FromQuery] decimal pay, [FromQuery] String cur)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return BadRequest("Vertrag not found");
        }
        
        if (DatabaseContext.Currencys.Any(c => c.Id == cur) is false)
        {
            return BadRequest("Currency not implemented. Abort.");
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return BadRequest("Vertrag not found");
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
        
        DatabaseContext.SaveChanges();

        return Ok(notice);
    }
    
    [HttpPut("payMonatsrate")]
    public ActionResult PayMonatsrate([FromQuery] int vertragsId, [FromQuery] decimal pay, [FromQuery] Boolean regular)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return BadRequest("Vertrag not found");
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return BadRequest("Vertrag not found");
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
        v.SummeNächsteAbbuchung = v.SummeNächsteAbbuchung - pay;
        v.ZeitpunktLetzteAbbuchung = DateTime.Now;
        if (regular)
        {
            v.ZeitpunktNächsteAbbuchung = DateTime.Now.AddMonths(1);
        }
        
        DatabaseContext.SaveChanges();

        return Ok(pay + " " + v.Währung + " was paid. Next payment due on " + v.ZeitpunktNächsteAbbuchung);
    }
    
    [HttpPut("setNextBalance")]
    public ActionResult SetNextBalance([FromQuery] int vertragsId)
    {
        if (DatabaseContext.Verträge.Any(v => v.Id == vertragsId) is false)
        {
            return BadRequest("Vertrag not found");
        }

        var vertrag = DatabaseContext.Verträge.FirstOrDefault(v => v.Id == vertragsId);
        if (vertrag == null)
        {
            return BadRequest("Vertrag not found");
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