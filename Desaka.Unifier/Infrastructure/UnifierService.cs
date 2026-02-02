using Desaka.Contracts.Common;
using Desaka.Contracts.EventBus;
using Desaka.Contracts.Unifier;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Desaka.EventBus;
using Desaka.Unifier.Application;
using Microsoft.EntityFrameworkCore;

namespace Desaka.Unifier.Infrastructure;

public sealed class UnifierService : IUnifierService
{
    private readonly DesakaDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly Desaka.Export.ExportService _exportService;
    private readonly UnifierProcessor _processor;
    private readonly MemoryLookupService _memoryLookup;

    public UnifierService(DesakaDbContext db, IEventBus eventBus, Desaka.Export.ExportService exportService, UnifierProcessor processor, MemoryLookupService memoryLookup)
    {
        _db = db;
        _eventBus = eventBus;
        _exportService = exportService;
        _processor = processor;
        _memoryLookup = memoryLookup;
    }

    public async Task<UnifierStartResponseDTO> StartAsync(UnifierStartRequestDTO request, CancellationToken cancellationToken = default)
    {
        var run = new UnifierRun
        {
            StartAt = DateTime.UtcNow,
            Status = "running"
        };

        _db.UnifierRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        await ProcessAsync(run, cancellationToken);

        await _eventBus.PublishAsync(new EventEnvelope(
            "UnifierRunStarted",
            "Unifier",
            DateTime.UtcNow,
            request.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new { run.Id })),
            cancellationToken);

        return new UnifierStartResponseDTO(run.Id, JobStatus.Running);
    }

    public Task<UnifierStatusResponseDTO?> GetStatusAsync(long runId, CancellationToken cancellationToken = default)
    {
        return GetStatusInternalAsync(runId, cancellationToken);
    }

    public Task<IReadOnlyList<ApprovalItemDTO>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return GetApprovalsInternalAsync(cancellationToken);
    }

    public async Task<ApprovalActionResponseDTO> ApproveAsync(ApprovalActionRequestDTO request, CancellationToken cancellationToken = default)
    {
        var approval = await _db.UnifierApprovals.FirstOrDefaultAsync(x => x.Id == request.ApprovalId, cancellationToken);
        if (approval == null)
        {
            return new ApprovalActionResponseDTO(JobStatus.Error);
        }

        var resolvedValue = request.Action == ApprovalAction.Override
            ? request.Value
            : approval.SuggestedValue ?? approval.CurrentValue ?? request.Value;

        if (!string.IsNullOrWhiteSpace(resolvedValue))
        {
            var memoryType = MapApprovalToMemoryType(approval.PropertyName);
            if (memoryType != null)
            {
                var key = !string.IsNullOrWhiteSpace(approval.CurrentValue) ? approval.CurrentValue : approval.ProductCode;
                await _memoryLookup.AddOrUpdateAsync(memoryType, key, resolvedValue, approval.Language, "user", cancellationToken);
            }
        }

        approval.Status = request.Action.ToString().ToLowerInvariant();
        approval.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return new ApprovalActionResponseDTO(JobStatus.Success);
    }

    private static string? MapApprovalToMemoryType(string propertyName)
    {
        return propertyName switch
        {
            "brand" => "product_brand_memory",
            "type" => "product_type_memory",
            "model" => "product_model_memory",
            "category" => "category_memory",
            "category_ids" => "category_id_list",
            "name" => "name_memory",
            "desc" => "desc_memory",
            "short_desc" => "short_desc_memory",
            "keywords_google" => "keywords_google",
            "keywords_zbozi" => "keywords_zbozi",
            "category_mapping_google" => "category_mapping_google",
            "category_mapping_heureka" => "category_mapping_heureka",
            "category_mapping_zbozi" => "category_mapping_zbozi",
            "category_mapping_glami" => "category_mapping_glami",
            _ => null
        };
    }

    private async Task<UnifierStatusResponseDTO?> GetStatusInternalAsync(long runId, CancellationToken cancellationToken)
    {
        var run = await _db.UnifierRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == runId, cancellationToken);
        if (run == null)
        {
            return null;
        }

        return new UnifierStatusResponseDTO(run.Id, ParseStatus(run.Status), run.ItemsProcessed, run.StartAt, run.EndAt, run.ErrorMessage);
    }

    private async Task<IReadOnlyList<ApprovalItemDTO>> GetApprovalsInternalAsync(CancellationToken cancellationToken)
    {
        var approvals = await _db.UnifierApprovals.AsNoTracking()
            .Where(x => x.Status == "pending")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return approvals.Select(x => new ApprovalItemDTO(x.Id, x.ProductCode, x.Url, x.ImageUrl, x.PropertyName, x.CurrentValue, x.SuggestedValue, x.Language)).ToList();
    }

    private static JobStatus ParseStatus(string status)
        => status switch
        {
            "running" => JobStatus.Running,
            "waiting" => JobStatus.WaitingApproval,
            "success" => JobStatus.Success,
            "error" => JobStatus.Error,
            "cancelled" => JobStatus.Cancelled,
            _ => JobStatus.Queued
        };

    private async Task ProcessAsync(UnifierRun run, CancellationToken cancellationToken)
    {
        var pending = await _db.UnifierApprovals.AsNoTracking()
            .AnyAsync(x => x.Status == "pending", cancellationToken);
        if (pending)
        {
            run.Status = "waiting";
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var result = await _processor.ProcessAsync(run, cancellationToken);
        if (result.Outcome == UnifierProcessOutcome.WaitingApproval)
        {
            run.Status = "waiting";
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var outputRoot = await ResolvePathAsync("paths.output_root", "data/output", cancellationToken);
        var products = await _db.ProductsCurrent.AsNoTracking().ToListAsync(cancellationToken);

        var export = await _exportService.ExportProductsAsync(products, new Desaka.Export.ExportOptions
        {
            OutputDirectory = outputRoot,
            Format = Desaka.Export.ExportFormat.Csv,
            FileNamePrefix = "Report_All"
        }, cancellationToken);

        run.ItemsTotal = products.Count;
        run.ItemsProcessed = products.Count;
        run.Status = "success";
        run.EndAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new EventEnvelope(
            "ExportCompleted",
            "Unifier",
            DateTime.UtcNow,
            null,
            System.Text.Json.JsonSerializer.Serialize(new { run.Id, export.FullPath })), cancellationToken);
    }

    private async Task<string> ResolvePathAsync(string key, string fallback, CancellationToken cancellationToken)
    {
        var setting = await _db.ConfigSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.Value) ? fallback : setting.Value;
    }
}

