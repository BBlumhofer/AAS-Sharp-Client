namespace AasSharpClient.Models;

public enum ActionStatusEnum
{
    OPEN,
    PLANNED,
    EXECUTING,
    SUSPENDED,
    DONE,
    ABORTED,
    ERROR
}

public enum StepStatusEnum
{
    OPEN,
    PLANNED,
    EXECUTING,
    SUSPENDED,
    DONE,
    ABORTED,
    ERROR
}

internal static class StatusExtensions
{
    public static string ToAasValue(this ActionStatusEnum status) => status switch
    {
        ActionStatusEnum.OPEN => "open",
        ActionStatusEnum.PLANNED => "planned",
        ActionStatusEnum.EXECUTING => "executing",
        ActionStatusEnum.SUSPENDED => "suspended",
        ActionStatusEnum.DONE => "done",
        ActionStatusEnum.ABORTED => "aborted",
        ActionStatusEnum.ERROR => "error",
        _ => "open"
    };

    public static string ToAasValue(this StepStatusEnum status) => status switch
    {
        StepStatusEnum.OPEN => "open",
        StepStatusEnum.PLANNED => "planned",
        StepStatusEnum.EXECUTING => "executing",
        StepStatusEnum.SUSPENDED => "suspended",
        StepStatusEnum.DONE => "done",
        StepStatusEnum.ABORTED => "aborted",
        StepStatusEnum.ERROR => "error",
        _ => "open"
    };

    public static StepStatusEnum FromAasValue(string? status) => status?.ToLowerInvariant() switch
    {
        "planned" => StepStatusEnum.PLANNED,
        "executing" => StepStatusEnum.EXECUTING,
        "suspended" => StepStatusEnum.SUSPENDED,
        "done" => StepStatusEnum.DONE,
        "aborted" => StepStatusEnum.ABORTED,
        "error" => StepStatusEnum.ERROR,
        _ => StepStatusEnum.OPEN
    };

    public static ActionStatusEnum FromActionValue(string? status) => status?.ToLowerInvariant() switch
    {
        "planned" => ActionStatusEnum.PLANNED,
        "executing" => ActionStatusEnum.EXECUTING,
        "suspended" => ActionStatusEnum.SUSPENDED,
        "done" => ActionStatusEnum.DONE,
        "aborted" => ActionStatusEnum.ABORTED,
        "error" => ActionStatusEnum.ERROR,
        _ => ActionStatusEnum.OPEN
    };
}
