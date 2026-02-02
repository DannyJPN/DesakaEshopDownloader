using Desaka.Contracts.Config;

namespace Desaka.Config.Application;

public interface IConfigService
{
    Task<IReadOnlyList<EshopConfigDto>> GetEshopsAsync(CancellationToken cancellationToken = default);
    Task<EshopConfigDto> UpsertEshopAsync(EshopConfigDto eshop, CancellationToken cancellationToken = default);
    Task<bool> DeleteEshopAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutopollRuleConfigDto>> GetAutopollRulesAsync(CancellationToken cancellationToken = default);
    Task<AutopollRuleConfigDto> UpsertAutopollRuleAsync(AutopollRuleConfigDto rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteAutopollRuleAsync(int id, CancellationToken cancellationToken = default);

    Task<PathsConfigDto> GetPathsAsync(CancellationToken cancellationToken = default);
    Task<PathsConfigDto> UpdatePathsAsync(PathsConfigDto paths, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LanguageConfigDto>> GetLanguagesAsync(CancellationToken cancellationToken = default);
    Task<LanguageConfigDto> UpsertLanguageAsync(LanguageConfigDto language, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiConfigDto>> GetAiConfigsAsync(CancellationToken cancellationToken = default);
    Task<AiConfigDto> UpsertAiConfigAsync(AiConfigDto config, CancellationToken cancellationToken = default);
}
