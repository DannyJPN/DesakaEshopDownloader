namespace Desaka.Contracts.Common;

public enum JobStatus
{
    Queued = 0,
    Running = 1,
    Success = 2,
    Error = 3,
    Cancelled = 4,
    WaitingApproval = 5
}

public enum ApprovalAction
{
    Approve = 0,
    Override = 1,
    Reject = 2
}

public enum NotificationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

public enum IntervalUnit
{
    Minute = 0,
    Hour = 1,
    Day = 2
}
