
namespace OKRService.Common
{
    public enum MessageType
    {
        /// <summary>
        /// The information
        /// </summary>
        Info,

        /// <summary>
        /// The success
        /// </summary>
        Success,

        /// <summary>
        /// The alert
        /// </summary>
        Alert,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The error
        /// </summary>
        Error,
    }

    public enum NotificationType
    {
        lockMyGoals = 16,
        KrUpdate = 15,
        ObjContributors = 25,
        KeyContributors = 26,
        DeleteOkr = 21,
        ImportPreviousOkr = 17,
        AlignObjectives = 18,
        AlignKey = 19,
        DeleteOkrAtReset = 22,
        ResetOkr = 25,
        AmberMessage = 23,
        RedMessage = 24,
        DeleteKr = 25,
        DraftOkr = 26
    }

    public enum MessageTypeForNotifications
    {
        NotificationsMessages = 1,
        AlertMessages = 2
    }

    public enum ProgressMaster
    {
        NotStarted = 1,
        AtRisk = 2,
        Lagging = 3,
        OnTrack = 4
    }

    public enum TemplateCodes
    {
        RA = 10,
        ASO = 11,
        KA = 12,
        KD = 13,
        AK = 14,
        DO = 15,
        AKR = 16,
        KP = 17,
        KP2 = 18
    }

    public enum Metrics
    {
        Percentage = 1,
        Currency = 2,
        Numbers = 3,
        Boolean = 4,
        NoUnits = 5
    }

    public enum GoalRequest
    {
        Sequence = 0,
        ImportedType = 1,
        KeyImportedType = 2,
        GoalStatusId = 2,
        GoalTypeId = 2
    }

    public enum GoalStatus
    {
        Draft = 1,
        Public = 2,
        Archived = 3
    }

    public enum AssignmentType
    {
        StandAlone = 1,
        WithParentObjective = 2,
    }

    public enum KrStatus
    {
        Pending = 1,
        Accepted,
        Declined
    }

    public enum GoalType
    {
        GoalObjective = 1,
        GoalKey = 2
    }

    public enum MetricType
    {
        Percentage = 1,
        Currency,
        Numbers,
        Boolean,
        NoUnits

    }

    public enum UserType
    {
        Parent = 1,
        Owner
    }

    public enum GoalTypeId
    {
        Team = 1,
        Individual = 2
    }

    public enum Filters
    {
        Assigned = 1,
        Aligned,
        Standalone,
        Individual
    }
    public enum CurrencyValues
    {
        Dollar = 1,
        Euro,
        Rupee,
        Yen,
        Pound
    }
}
