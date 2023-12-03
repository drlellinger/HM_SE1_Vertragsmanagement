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

    [HttpGet]
    public ActionResult<Debitor[]> GetAllDebitoren()
    {
        var allDebitoren = DatabaseContext.Debitoren.ToArray();
        return Ok(allDebitoren);
    }
    
}