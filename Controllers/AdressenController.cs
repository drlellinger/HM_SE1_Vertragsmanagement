using Microsoft.AspNetCore.Mvc;
using Vertragsmanagement.DataAccess;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.Controllers;

[ApiController]
[Route("~/adressen")]
public class AdressenController : ControllerBase
{
    private DatabaseContext DatabaseContext { get; }

    public AdressenController(DatabaseContext dbc)
    {
        this.DatabaseContext = dbc;
    }

    
    /// <summary>
    /// Gibt alle gespeicherten Adressen aus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<Adresse[]> GetAllAdresses()
    {
        var allAdressen = DatabaseContext.Adressen.ToArray();
        return Ok(allAdressen);
    }
    
    /// <summary>
    /// Fügt Adresse hinzu
    /// </summary>
    /// <param name="adresse"></param>
    /// <response code="400">Bad request. Returns response body with error.</response>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddAdresse([FromBody] Adresse adresse)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);
        
        DatabaseContext.Adressen.Add(adresse);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    /// <summary>
    /// Ändert vorhandene Adresse
    /// </summary>
    /// <param name="adresse"></param>
    /// <response code="400">Bad request. Returns response body with error.</response>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateAdress([FromBody] Adresse adresse)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (adresse.Id == 0) return BadRequest("Id of Adress is required when updating a Adress");

        var dbAdress = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adresse.Id);
        if (dbAdress == null) return NotFound("Adresse " + adresse.Id + "existiert nicht");

        dbAdress.Update(adresse); //Methode in Adresse_DomainObject implementiert
        
        DatabaseContext.SaveChanges();
        return Ok();
    }

    
    /// <summary>
    /// Löscht die angegebene Adresse anhand der Adress-ID
    /// </summary>
    /// <param name="adressId">Adress-ID</param>
    /// <param name="force">Force-Parameter</param>
    /// <response code="400">Bad request. Returns error message</response>
    /// <response code="404">Not found.</response>
    /// <returns></returns>
    [HttpDelete("{adressId}")]
    public ActionResult DeleteAdress([FromRoute] int adressId, [FromQuery] bool force)
    {
        var adressse = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adressId);
        if (adressse == null) return NotFound();

        if (adressse.InUse != 0 && force==false) return BadRequest("Adresse still in use.");

        var msg = "";
        
        if (force)
        {
            msg = "Adresse " + adressId + " was deleted forcefully.";
        }
        else
        {
            msg = "Adresse " + adressId + " was deleted.";
        }

        DatabaseContext.Adressen.Remove(adressse);
            
        DatabaseContext.SaveChanges();
        return Ok(msg);
    }
    
}