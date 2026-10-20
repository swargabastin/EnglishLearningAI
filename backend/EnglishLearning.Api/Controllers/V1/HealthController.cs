using EnglishLearning.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var response = ApiResponse<object>.Ok(new
        {
            status = "healthy",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        }, "System is operational");

        return Ok(response);
    }
}
