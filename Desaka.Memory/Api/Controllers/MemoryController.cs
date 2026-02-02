using Desaka.Contracts.Memory;
using Desaka.Memory.Application;
using Microsoft.AspNetCore.Mvc;

namespace Desaka.Memory.Api.Controllers;

[ApiController]
[Route("api/v1/memory")]
public sealed class MemoryController : ControllerBase
{
    private readonly IMemoryService _memoryService;

    public MemoryController(IMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<MemoryEntryDTO>>> Search([FromQuery] string memory_type, [FromQuery] string query, [FromQuery] string? language_code, [FromQuery] string mode)
        => Ok(await _memoryService.SearchAsync(new MemorySearchRequestDTO(memory_type, query, language_code, mode), HttpContext.RequestAborted));

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<MemoryEntryDTO>>> SearchPost([FromBody] MemorySearchRequestDTO request)
        => Ok(await _memoryService.SearchAsync(request, HttpContext.RequestAborted));

    [HttpPost("import")]
    public async Task<ActionResult<MemoryImportResponseDTO>> Import([FromForm] string memory_type, [FromForm] IFormFile file, [FromForm] string? language_code)
    {
        await using var stream = file.OpenReadStream();
        var result = await _memoryService.ImportAsync(memory_type, stream, language_code, HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string memory_type, [FromQuery] string? language_code)
    {
        var entries = await _memoryService.ExportAsync(memory_type, language_code, HttpContext.RequestAborted);
        var csv = BuildCsv(entries);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"{memory_type}_export.csv");
    }

    [HttpGet("entries")]
    public async Task<ActionResult<IReadOnlyList<MemoryEntryDTO>>> ListEntries([FromQuery] string memory_type)
        => Ok(await _memoryService.ExportAsync(memory_type, null, HttpContext.RequestAborted));

    [HttpPost("entries")]
    public async Task<ActionResult<MemoryEntryDTO>> Create([FromQuery] string memory_type, [FromBody] MemoryEntryDTO entry)
        => Ok(await _memoryService.CreateAsync(memory_type, entry, HttpContext.RequestAborted));

    [HttpPut("entries")]
    public async Task<ActionResult<MemoryEntryDTO>> Update([FromQuery] string memory_type, [FromBody] MemoryEntryDTO entry)
    {
        var result = await _memoryService.UpdateAsync(memory_type, entry, HttpContext.RequestAborted);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("entries")]
    public async Task<IActionResult> Delete([FromQuery] string memory_type, [FromQuery] int id)
    {
        var ok = await _memoryService.DeleteAsync(memory_type, id, HttpContext.RequestAborted);
        return ok ? NoContent() : NotFound();
    }

    private static string BuildCsv(IEnumerable<MemoryEntryDTO> entries)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("id;key;value;source;created_at;updated_at;language_code");
        foreach (var entry in entries)
        {
            builder.Append(entry.Id).Append(';');
            builder.Append(Escape(entry.Key)).Append(';');
            builder.Append(Escape(entry.Value)).Append(';');
            builder.Append(Escape(entry.Source)).Append(';');
            builder.Append(entry.CreatedAt.ToString("yyyy-MM-dd")).Append(';');
            builder.Append(entry.UpdatedAt.ToString("yyyy-MM-dd")).Append(';');
            builder.Append(Escape(entry.LanguageCode ?? string.Empty)).AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('\"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

