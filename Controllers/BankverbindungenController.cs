using Microsoft.AspNetCore.Mvc;
using Vertragsmanagement.DataAccess;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.Controllers;

[ApiController]
[Route("~/bankverbindungen")]
public class BankverbindungenController : ControllerBase
{
    private DatabaseContext DatabaseContext { get; }

    public BankverbindungenController(DatabaseContext dbc)
    {
        this.DatabaseContext = dbc;
    }
    
    /// <summary>
    /// Gibt alle Bankverbindungen aus
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<Bankverbindung[]> GetAllBVs()
    {
        var allBVs = DatabaseContext.Bankverbindungen.ToArray();
        return Ok(allBVs);
    }
    
    
    /// <summary>
    /// Suche nach Bankverbindungen anhand von Schlagwörtern
    /// </summary>
    /// <param name="searchString">Sucheingabe</param>
    /// <returns></returns>
    [HttpGet("search")]
    public ActionResult<Bankverbindung[]> SearchBVs([FromQuery] string searchString)
    {
        var bvSuche = DatabaseContext.Bankverbindungen
            .Where(b => 
                b.Iban.ToUpper().Contains(searchString.ToUpper())
                ||
                b.Bic.ToUpper().Contains(searchString.ToUpper())
                ||
                b.Kontoinhaber.ToUpper().Contains(searchString.ToUpper())
            )
            .ToArray();

        string bvResult = bvSuche.ToString();

        if (bvResult != null && !(bvResult.Contains("[]")))
        {
            return NotFound("\"" + searchString + "\" is not found");
        }
        else
        {
            return Ok(bvSuche);
        }

    }
    
    
    /// <summary>
    /// Gibt die Zahlungsinformationen anhand einer IBAN aus
    /// </summary>
    /// <param name="iban">IBAN der Bankverbindung</param>
    /// <returns></returns>
    [HttpGet("{iban}")]
    public ActionResult<Bankverbindung> GetBVByIban([FromRoute] string iban)
    {
        var bv = DatabaseContext.Bankverbindungen.FirstOrDefault(b => b.Iban.Equals(iban));
        if (bv == null) return NotFound();
        return Ok(bv);
    }
    
    /// <summary>
    /// Fügt Bankverbindung hinzu
    /// </summary>
    /// <param name="bv"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult AddBv([FromBody] Bankverbindung bv)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);
        
        DatabaseContext.Bankverbindungen.Add(bv);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    /// <summary>
    /// Ändert vorhandene Bankverbindung
    /// </summary>
    /// <param name="bv"></param>
    /// <returns></returns>
    [HttpPut]
    public ActionResult UpdateBv([FromBody] Bankverbindung bv)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        if (bv.Iban is null) return BadRequest("IBAN is required when updating a Adress");

        var dbBv = DatabaseContext.Bankverbindungen.FirstOrDefault(b => b.Iban.Equals(bv.Iban));
        if (dbBv == null) return NotFound("Adresse " + bv.Iban + "existiert nicht");

        DatabaseContext.Bankverbindungen.Update(bv);        
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
    
    /// <summary>
    /// Löscht die Bankverbindung
    /// </summary>
    /// <param name="iban">IBAN der Bankverbindung</param>
    /// <returns></returns>
    [HttpDelete("{iban}")]
    public ActionResult DeleteAdress([FromRoute] string iban)
    {
        var bv = DatabaseContext.Bankverbindungen.FirstOrDefault(b => b.Iban.Equals(iban));
        if (bv == null) return NotFound();

        if (bv.InUse != 0) return BadRequest("Bankverbindung still in use.");
        
        DatabaseContext.SaveChanges();
        return Ok("Bankverbindung mit der IBAN " + iban + " was deleted.");
    }
}