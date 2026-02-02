using Desaka.Contracts.Common;
using Desaka.Contracts.WebScrape;
using Desaka.WebScrape.Application;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.WebScrape.Api.Controllers;

[ApiController]
[Route("api/v1/webscrape")]
public sealed class WebScrapeController : ControllerBase
{
    private readonly IWebScrapeService _service;

    public WebScrapeController(IWebScrapeService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<ActionResult<WebScrapeStartResponseDTO>> Start([FromBody] WebScrapeStartRequestDTO request)
        => Ok(await _service.StartAsync(request, HttpContext.RequestAborted));

    [HttpGet("status")]
    public async Task<ActionResult<WebScrapeStatusResponseDTO>> Status([FromQuery] long? run_id, [FromQuery] int? eshop_id)
    {
        var result = await _service.GetStatusAsync(run_id, eshop_id, HttpContext.RequestAborted);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("logs")]
    public ActionResult<IReadOnlyList<WebScrapeLogEntryDTO>> Logs([FromQuery] long run_id, [FromQuery] DateTime? from)
        => Ok(Array.Empty<WebScrapeLogEntryDTO>());

    [HttpGet("list")]
    public async Task<ActionResult<IReadOnlyList<WebScrapeStatusResponseDTO>>> List([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _service.ListAsync(status, from, to, HttpContext.RequestAborted));

    [HttpGet("progress/stream")]
    public IActionResult ProgressStream() => NoContent();
}

