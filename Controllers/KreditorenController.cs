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

    [HttpGet]
    public ActionResult<Kreditor[]> GetAllKreditoren()
    {
        var allKreditoren = DatabaseContext.Kreditoren.ToArray();
        return Ok(allKreditoren);
    }
    
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
    
    [HttpGet("{kreditorId}")]
    public ActionResult<Kreditor> GetKreditorById([FromRoute] int kreditorId)
    {
        var kreditor = DatabaseContext.Kreditoren.FirstOrDefault(k => k.Id == kreditorId);
        if (kreditor == null) return NotFound();
        return Ok(kreditor);
    }
    
    
    [HttpPost]
    public ActionResult AddKreditor([FromBody] Kreditor kreditor)
    {
        if (ModelState.IsValid is false) return BadRequest(ModelState);

        DatabaseContext.Kreditoren.Add(kreditor);
        DatabaseContext.SaveChanges();
        return Ok();
    }
    
}