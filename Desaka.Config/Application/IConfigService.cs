using Desaka.Contracts.Config;

namespace Desaka.Config.Application;

public interface IConfigService
{
    Task<IReadOnlyList<EshopConfigDTO>> GetEshopsAsync(CancellationToken cancellationToken = default);
    Task<EshopConfigDTO> UpsertEshopAsync(EshopConfigDTO eshop, CancellationToken cancellationToken = default);
    Task<bool> DeleteEshopAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutopollRuleConfigDTO>> GetAutopollRulesAsync(CancellationToken cancellationToken = default);
    Task<AutopollRuleConfigDTO> UpsertAutopollRuleAsync(AutopollRuleConfigDTO rule, CancellationToken cancellationToken = default);
    Task<bool> DeleteAutopollRuleAsync(int id, CancellationToken cancellationToken = default);

    Task<PathsConfigDTO> GetPathsAsync(CancellationToken cancellationToken = default);
    Task<PathsConfigDTO> UpdatePathsAsync(PathsConfigDTO paths, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LanguageConfigDTO>> GetLanguagesAsync(CancellationToken cancellationToken = default);
    Task<LanguageConfigDTO> UpsertLanguageAsync(LanguageConfigDTO language, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiConfigDTO>> GetAiConfigsAsync(CancellationToken cancellationToken = default);
    Task<AiConfigDTO> UpsertAiConfigAsync(AiConfigDTO config, CancellationToken cancellationToken = default);
}
