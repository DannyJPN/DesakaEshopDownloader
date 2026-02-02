using Desaka.Config.Application;
using Desaka.Contracts.Config;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Config.Api.Controllers;

[ApiController]
[Route("api/v1/config")]
public sealed class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet("eshops")]
    public async Task<ActionResult<IReadOnlyList<EshopConfigDTO>>> GetEshops()
        => Ok(await _configService.GetEshopsAsync(HttpContext.RequestAborted));

    [HttpPost("eshops")]
    public async Task<ActionResult<EshopConfigDTO>> CreateEshop([FromBody] EshopConfigDTO dto)
        => Ok(await _configService.UpsertEshopAsync(dto, HttpContext.RequestAborted));

    [HttpPut("eshops")]
    public async Task<ActionResult<EshopConfigDTO>> UpdateEshop([FromBody] EshopConfigDTO dto)
        => Ok(await _configService.UpsertEshopAsync(dto, HttpContext.RequestAborted));

    [HttpDelete("eshops")]
    public async Task<IActionResult> DeleteEshop([FromQuery] int id)
        => await _configService.DeleteEshopAsync(id, HttpContext.RequestAborted) ? NoContent() : NotFound();

    [HttpGet("autopoll-rules")]
    public async Task<ActionResult<IReadOnlyList<AutopollRuleConfigDTO>>> GetRules()
        => Ok(await _configService.GetAutopollRulesAsync(HttpContext.RequestAborted));

    [HttpPost("autopoll-rules")]
    public async Task<ActionResult<AutopollRuleConfigDTO>> CreateRule([FromBody] AutopollRuleConfigDTO dto)
        => Ok(await _configService.UpsertAutopollRuleAsync(dto, HttpContext.RequestAborted));

    [HttpPut("autopoll-rules")]
    public async Task<ActionResult<AutopollRuleConfigDTO>> UpdateRule([FromBody] AutopollRuleConfigDTO dto)
        => Ok(await _configService.UpsertAutopollRuleAsync(dto, HttpContext.RequestAborted));

    [HttpDelete("autopoll-rules")]
    public async Task<IActionResult> DeleteRule([FromQuery] int id)
        => await _configService.DeleteAutopollRuleAsync(id, HttpContext.RequestAborted) ? NoContent() : NotFound();

    [HttpGet("paths")]
    public async Task<ActionResult<PathsConfigDTO>> GetPaths()
        => Ok(await _configService.GetPathsAsync(HttpContext.RequestAborted));

    [HttpPost("paths")]
    public async Task<ActionResult<PathsConfigDTO>> SetPaths([FromBody] PathsConfigDTO dto)
        => Ok(await _configService.UpdatePathsAsync(dto, HttpContext.RequestAborted));

    [HttpGet("languages")]
    public async Task<ActionResult<IReadOnlyList<LanguageConfigDTO>>> GetLanguages()
        => Ok(await _configService.GetLanguagesAsync(HttpContext.RequestAborted));

    [HttpPost("languages")]
    public async Task<ActionResult<LanguageConfigDTO>> CreateLanguage([FromBody] LanguageConfigDTO dto)
        => Ok(await _configService.UpsertLanguageAsync(dto, HttpContext.RequestAborted));

    [HttpPut("languages")]
    public async Task<ActionResult<LanguageConfigDTO>> UpdateLanguage([FromBody] LanguageConfigDTO dto)
        => Ok(await _configService.UpsertLanguageAsync(dto, HttpContext.RequestAborted));

    [HttpGet("ai")]
    public async Task<ActionResult<IReadOnlyList<AiConfigDTO>>> GetAi()
        => Ok(await _configService.GetAiConfigsAsync(HttpContext.RequestAborted));

    [HttpPost("ai")]
    public async Task<ActionResult<AiConfigDTO>> CreateAi([FromBody] AiConfigDTO dto)
        => Ok(await _configService.UpsertAiConfigAsync(dto, HttpContext.RequestAborted));

    [HttpPut("ai")]
    public async Task<ActionResult<AiConfigDTO>> UpdateAi([FromBody] AiConfigDTO dto)
        => Ok(await _configService.UpsertAiConfigAsync(dto, HttpContext.RequestAborted));

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ConfigValidationRequestDTO request) => Ok();
}

