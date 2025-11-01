using Microsoft.AspNetCore.Mvc;

namespace SpendWise.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthCheckController : ControllerBase
{
    /// <summary>
    /// Health check simples na rota /health
    /// </summary>
    [HttpGet]
    public ActionResult<object> Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            service = "SpendWise API"
        });
    }
}
