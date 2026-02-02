using Desaka.Contracts.Common;
using Desaka.Contracts.Unifier;
using Desaka.Unifier.Application;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Unifier.Api.Controllers;

[ApiController]
[Route("api/v1/unifier")]
public sealed class UnifierController : ControllerBase
{
    private readonly IUnifierService _service;

    public UnifierController(IUnifierService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<ActionResult<UnifierStartResponseDTO>> Start([FromBody] UnifierStartRequestDTO request)
        => Ok(await _service.StartAsync(request, HttpContext.RequestAborted));

    [HttpGet("status")]
    public async Task<ActionResult<UnifierStatusResponseDTO>> Status([FromQuery] long run_id)
    {
        var status = await _service.GetStatusAsync(run_id, HttpContext.RequestAborted);
        return status == null ? NotFound() : Ok(status);
    }

    [HttpGet("pending-approvals")]
    public async Task<ActionResult<IReadOnlyList<ApprovalItemDTO>>> PendingApprovals()
        => Ok(await _service.GetPendingApprovalsAsync(HttpContext.RequestAborted));

    [HttpPost("approve")]
    public async Task<ActionResult<ApprovalActionResponseDTO>> Approve([FromBody] ApprovalActionRequestDTO request)
        => Ok(await _service.ApproveAsync(request, HttpContext.RequestAborted));

    [HttpGet("logs")]
    public ActionResult<IReadOnlyList<string>> Logs([FromQuery] long run_id, [FromQuery] DateTime? from)
        => Ok(Array.Empty<string>());
}

