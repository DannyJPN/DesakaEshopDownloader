using Desaka.Validation.Application;
using Desaka.Validation.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Validation.Api.Controllers;

[ApiController]
[Route("api/v1/validation")]
public sealed class ValidationController : ControllerBase
{
    private readonly IValidationService _validationService;

    public ValidationController(IValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpGet("status")]
    public IActionResult Status() => Ok();

    [HttpPost("url")]
    public async Task<ActionResult<ValidationResult>> ValidateUrl([FromBody] string url)
        => Ok(await _validationService.ValidateUrlAsync(url, HttpContext.RequestAborted));

    [HttpPost("api-key")]
    public async Task<ActionResult<ValidationResult>> ValidateApiKey([FromBody] ApiKeyValidationRequest request)
        => Ok(await _validationService.ValidateApiKeyAsync(request.BaseUrl, request.ApiKey, HttpContext.RequestAborted));

    [HttpGet("memory-check")]
    public async Task<ActionResult<IReadOnlyList<MemoryValidationIssue>>> MemoryCheck(
        [FromServices] MemoryValidationService memoryValidationService)
        => Ok(await memoryValidationService.ValidateAsync(HttpContext.RequestAborted));
}

public sealed record ApiKeyValidationRequest(string BaseUrl, string ApiKey);
