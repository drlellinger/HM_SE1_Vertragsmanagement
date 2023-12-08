using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddAdresse([FromBody] Adresse adresse)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);
        
        DatabaseContext.Adressen.Add(adresse);
        DatabaseContext.SaveChanges();
        System.Console.WriteLine(adresse + "was added.");
        return Ok();
    }
    
    /// <summary>
    /// Ändert vorhandene Adresse
    /// </summary>
    /// <param name="adresse"></param>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateAdress([FromBody] Adresse adresse)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (adresse.Id == 0) return BadRequest("Id of Adress is required when updating a Adress");

        var dbAdress = DatabaseContext.Adressen.FirstOrDefault(v => v.Id == adresse.Id);
        if (dbAdress == null) return NotFound("Adresse " + adresse.Id + "existiert nicht");

        dbAdress.Update(adresse); //Methode in Vertrag_DomainObject implementiert
        
        DatabaseContext.SaveChanges();
        return Ok();
    }

    
    /// <summary>
    /// Löscht die angegebene Adresse anhand der Adress-ID
    /// </summary>
    /// <param name="adressId">Adress-ID</param>
    /// <returns></returns>
    [HttpDelete("{adressId}")]
    public ActionResult DeleteAdress([FromRoute] int adressId)
    {
        var adressse = DatabaseContext.Adressen.FirstOrDefault(a => a.Id == adressId);
        if (adressse == null) return NotFound();

        if (adressse.InUse != 0) return BadRequest("Adresse still in use.");
        
        DatabaseContext.SaveChanges();
        return Ok("Adresse " + adressId + " was deleted.");
    }
    
}