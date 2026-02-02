using Desaka.Autopolling.Application;
using Desaka.Autopolling.Infrastructure;
using Desaka.Contracts.Autopoll;
using Desaka.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Autopolling.Api.Controllers;

[ApiController]
[Route("api/v1/autopoll")]
public sealed class AutopollController : ControllerBase
{
    private readonly IAutopollService _service;

    public AutopollController(IAutopollService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<ActionResult<AutopollStartResponseDTO>> Start([FromBody] AutopollStartRequestDTO request)
        => Ok(await _service.StartAsync(request, HttpContext.RequestAborted));

    [HttpGet("status")]
    public async Task<ActionResult<AutopollStatusResponseDTO>> Status([FromQuery] long? run_id, [FromQuery] long? rule_id)
    {
        var result = await _service.GetStatusAsync(run_id, rule_id.HasValue ? (int?)rule_id.Value : null, HttpContext.RequestAborted);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("rules")]
    public async Task<ActionResult<IReadOnlyList<AutopollRuleDTO>>> ListRules()
        => Ok(await _service.GetRulesAsync(HttpContext.RequestAborted));

    [HttpPost("rules")]
    public async Task<ActionResult<AutopollRuleDTO>> CreateRule([FromBody] AutopollRuleDTO rule)
        => Ok(await _service.UpsertRuleAsync(rule, HttpContext.RequestAborted));

    [HttpPut("rules")]
    public async Task<ActionResult<AutopollRuleDTO>> UpdateRule([FromBody] AutopollRuleDTO rule)
        => Ok(await _service.UpsertRuleAsync(rule, HttpContext.RequestAborted));

    [HttpDelete("rules")]
    public async Task<IActionResult> DeleteRule([FromQuery] int id)
        => await _service.DeleteRuleAsync(id, HttpContext.RequestAborted) ? NoContent() : NotFound();

    [HttpGet("logs")]
    public ActionResult<IReadOnlyList<string>> Logs([FromQuery] long run_id, [FromQuery] DateTime? from)
        => Ok(Array.Empty<string>());

    [HttpPost("batch-commit")]
    public async Task<ActionResult<AutopollBatchCommitResponseDTO>> BatchCommit(
        [FromServices] AutopollBatchCommitService batchCommitService,
        [FromBody] AutopollBatchCommitRequestDTO request)
        => Ok(await batchCommitService.CommitAsync(request, HttpContext.RequestAborted));

    [HttpGet("reports")]
    public IActionResult Reports([FromQuery] DateTime? date) => NoContent();
}

