using Desaka.AI.Application;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.AI.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public sealed class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("pricing")]
    public async Task<ActionResult<IReadOnlyList<AiPricingDto>>> Pricing()
        => Ok(await _aiService.GetPricingAsync(HttpContext.RequestAborted));

    [HttpPost("pricing/update")]
    public async Task<IActionResult> UpdatePricing()
    {
        await _aiService.UpdatePricingAsync(HttpContext.RequestAborted);
        return Ok();
    }
}
