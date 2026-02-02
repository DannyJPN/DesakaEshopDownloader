namespace Desaka.Contracts.Common;

public static class ServiceNames
{
    public const string WebScrape = "WebScrape";
    public const string Autopolling = "Autopolling";
    public const string Unifier = "Unifier";
    public const string Memory = "Memory";
    public const string Config = "Config";
    public const string AI = "AI";
    public const string Validation = "Validation";
    public const string Notification = "Notification";
}

public static class EventTypes
{
    public const string NotificationRequested = "NotificationRequested";
    public const string WebScrapeStarted = "WebScrapeStarted";
    public const string WebScrapeFinished = "WebScrapeFinished";
    public const string WebScrapeFailed = "WebScrapeFailed";
    public const string AutopollRunStarted = "AutopollRunStarted";
    public const string AutopollRunFinished = "AutopollRunFinished";
    public const string AutopollRunFailed = "AutopollRunFailed";
    public const string AutopollBatchReady = "AutopollBatchReady";
    public const string AutopollBatchCommitted = "AutopollBatchCommitted";
    public const string UnifierRunStarted = "UnifierRunStarted";
    public const string UnifierRunFinished = "UnifierRunFinished";
    public const string UnifierRunFailed = "UnifierRunFailed";
    public const string ApprovalNeeded = "ApprovalNeeded";
    public const string ExportCompleted = "ExportCompleted";
}
