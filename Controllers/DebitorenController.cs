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
    /// <response code="404">Not found</response>
    /// <response code="418">I'm a teapot. Empty search string.</response>
    /// <returns></returns>
    [HttpGet("search")]
    public ActionResult<Debitor[]> SearchDebitoren([FromQuery] string searchString)
    {
        try
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
        catch (Exception ex)
        {
            return StatusCode(418, "Empty search string.");
        }

    }
    
    /// <summary>
    /// Gibt Debitoren anhand von der Debitoren-Nummer aus
    /// </summary>
    /// <param name="debitorId"></param>
    /// <response code="200">Success. Return Debitor.</response>
    /// <response code="404">Not found</response>
    /// <returns></returns>
    [HttpGet("{debitorId}")]
    public ActionResult<Debitor> GetDebitorById([FromRoute] int debitorId)
    {
        var debitor = DatabaseContext.Debitoren.FirstOrDefault(d => d.Id == debitorId);
        if (debitor == null) return NotFound();
        return Ok(debitor);
    }
    
    /// <summary>
    /// Gibt Adresse anhand einer Debitoren-Nummer aus
    /// </summary>
    /// <param name="debitorId"></param>
    /// <response code="200">Success. Return Adresse.</response>
    /// <response code="404">Not found</response>
    /// <response code="500">Database error</response>
    /// <returns></returns>
    [HttpGet("{debitorId}/adresse")]
    public ActionResult<Adresse> GetAdresseFromDebitor([FromRoute] int debitorId)
    {
        var debitor = DatabaseContext.Debitoren.FirstOrDefault(d => d.Id == debitorId);
        if (debitor == null) return NotFound();

        var adressId = debitor.Adresse;
        var adresse = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adressId);
        if (adresse == null) return StatusCode(500);
        
        return Ok(adresse);
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
    /// <summary>
    /// Ändert vorhandenen Debitor
    /// </summary>
    /// <param name="debitor"></param>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateDebitor([FromBody] Debitor debitor)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (debitor.Id == 0) return BadRequest("Id of Debitor is required when updating a Debitor");

        var dbDebitor = DatabaseContext.Debitoren.FirstOrDefault(d => d.Id == debitor.Id);
        if (dbDebitor == null) return NotFound("Debitor " + debitor.Id + "existiert nicht");

        dbDebitor.Update(debitor); //Methode in Debitor_DomainObject implementiert
        
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
}