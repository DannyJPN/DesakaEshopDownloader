namespace Desaka.Contracts.WebScrape;

public sealed record WebScrapeLogEntryDTO(DateTime Timestamp, string Level, string Message, string? DataJson = null);
