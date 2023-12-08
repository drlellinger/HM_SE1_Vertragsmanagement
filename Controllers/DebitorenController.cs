using Microsoft.AspNetCore.Mvc;
using Vertragsmanagement.DataAccess;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.Controllers;

[ApiController]
[Route("~/debitoren")]
public class DebitorenController : ControllerBase
{
    private DatabaseContext DatabaseContext { get; }
    public DebitorenController(DatabaseContext dbc)
    {
        this.DatabaseContext = dbc;
    }

    
    /// <summary>
    /// Gibt alle Debitoren aus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<Debitor[]> GetAllDebitoren()
    {
        var allDebitoren = DatabaseContext.Debitoren.ToArray();
        return Ok(allDebitoren);
    }
    
    /// <summary>
    /// Suche nach Debitoren anhand von Schlagwörtern
    /// </summary>
    /// <param name="searchString">Sucheingabe</param>
    /// <returns></returns>
    [HttpGet("search")]
    public ActionResult<Debitor[]> SearchDebitoren([FromQuery] string searchString)
    {
        var debitoren = DatabaseContext.Debitoren
            .Where(d => 
                // k.Id.ToString().Equals(searchString)
                // ||
                d.Name.ToUpper().Contains(searchString.ToUpper())
                ||
                d.Vorname.ToUpper().Contains(searchString.ToUpper())
            )
            .ToArray();

        string debitorenResult = debitoren.ToString();

        if (debitorenResult != null && !(debitorenResult.Contains("[]")))
        {
            return NotFound("\"" + searchString + "\" is not found");
        }
        else
        {
            return Ok(debitoren);
        }

    }
    
    /// <summary>
    /// Gibt Debitoren anhand von der Debitoren-Nummer aus
    /// </summary>
    /// <param name="debitorId"></param>
    /// <returns></returns>
    [HttpGet("{debitorId}")]
    public ActionResult<Debitor> GetDebitorById([FromRoute] int debitorId)
    {
        var debitor = DatabaseContext.Debitoren.FirstOrDefault(d => d.Id == debitorId);
        if (debitor == null) return NotFound();
        return Ok(debitor);
    }
    
    /// <summary>
    /// Fügt einen Debitor hinzu
    /// </summary>
    /// <param name="debitor"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddDebitor([FromBody] Debitor debitor)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        int adresseId = debitor.Adresse;
        var adresse = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adresseId);
        if (adresse == null)
        {
            return BadRequest("Adresse is not found");
        }
        else
        {
            adresse.InUse++; //Hier wird der Zähler erhöht
        }
        
        string iban = debitor.Bankverbindung;
        var bv = DatabaseContext.Bankverbindungen.FirstOrDefault(b => b.Iban.Equals(iban) );
        if (bv == null)
        {
            return BadRequest("Bankverbindung is not found");
        }
        else
        {
            bv.InUse++; //Hier wird der Zähler erhöht
        }
        
        DatabaseContext.Debitoren.Add(debitor);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
}