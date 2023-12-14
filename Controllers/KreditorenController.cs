using Microsoft.AspNetCore.Mvc;
using Vertragsmanagement.DataAccess;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.Controllers;

[ApiController]
[Route("~/kreditoren")]
public class KreditorenController : ControllerBase
{
    private DatabaseContext DatabaseContext { get; }
    public KreditorenController(DatabaseContext dbc)
    {
        this.DatabaseContext = dbc;
    }

    
    /// <summary>
    /// Gibt alle Kreditoren aus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<Kreditor[]> GetAllKreditoren()
    {
        var allKreditoren = DatabaseContext.Kreditoren.ToArray();
        return Ok(allKreditoren);
    }
    
    
    /// <summary>
    /// Suche nach Kreditoren anhand von Schlagwörtern
    /// </summary>
    /// <param name="searchString">Sucheingabe</param>
    /// <returns></returns>
    [HttpGet("search")]
    public ActionResult<Kreditor[]> SearchKreditoren([FromQuery] string searchString)
    {
        var kreditoren = DatabaseContext.Kreditoren
            .Where(k => 
                // k.Id.ToString().Equals(searchString)
                // ||
                k.Name.ToUpper().Contains(searchString.ToUpper())
                ||
                k.Vorname.ToUpper().Contains(searchString.ToUpper())
            )
            .ToArray();

        string kreditorenResult = kreditoren.ToString();

        if (kreditorenResult != null && !(kreditorenResult.Contains("[]")))
        {
            return NotFound("\"" + searchString + "\" is not found");
        }
        else
        {
            return Ok(kreditoren);
        }

    }
    
    /// <summary>
    /// Gibt Kreditoren anhand von der Kreditoren-Nummer aus
    /// </summary>
    /// <param name="kreditorId"></param>
    /// <returns></returns>
    [HttpGet("{kreditorId}")]
    public ActionResult<Kreditor> GetKreditorById([FromRoute] int kreditorId)
    {
        var kreditor = DatabaseContext.Kreditoren.FirstOrDefault(k => k.Id == kreditorId);
        if (kreditor == null) return NotFound();
        return Ok(kreditor);
    }
    
    /// <summary>
    /// Fügt einen Kreditor hinzu
    /// </summary>
    /// <param name="kreditor"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddKreditor([FromBody] Kreditor kreditor)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        int adresseId = kreditor.Adresse;
        var adresse = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adresseId);
        if (adresse == null)
        {
            return BadRequest("Adresse is not found");
        }
        else
        {
            adresse.InUse++; //Hier wird der Zähler erhöht
        }
        
        string iban = kreditor.Bankverbindung;
        var bv = DatabaseContext.Bankverbindungen.FirstOrDefault(b => b.Iban.Equals(iban) );
        if (bv == null)
        {
            return BadRequest("Bankverbindung is not found");
        }
        else
        {
            bv.InUse++; //Hier wird der Zähler erhöht
        }
        
        DatabaseContext.Kreditoren.Add(kreditor);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    /// <summary>
    /// Ändert vorhandenen Kreditor
    /// </summary>
    /// <param name="kreditor"></param>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateKreditor([FromBody] Kreditor kreditor)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (kreditor.Id == 0) return BadRequest("Id of Kreditor is required when updating a Kreditor");

        var dbKreditor = DatabaseContext.Kreditoren.FirstOrDefault(k => k.Id == kreditor.Id);
        if (dbKreditor == null) return NotFound("Kreditor " + kreditor.Id + "existiert nicht");

        dbKreditor.Update(kreditor); //Methode in Kreditor_DomainObject implementiert
        
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
}