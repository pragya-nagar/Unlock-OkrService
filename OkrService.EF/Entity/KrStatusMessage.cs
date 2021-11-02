using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class KrStatusMessage
    {
        public int KrStatusMessageId { get; set; }
        public long AssignerGoalKeyId { get; set; }
        public long AssigneeGoalKeyId { get; set; }
        public string KrAssignerMessage { get; set; }
        public string KrAssigneeMessage { get; set; }
        public DateTime CreatedOnAssigner { get; set; }
        public DateTime CreatedOnAssignee { get; set; }
        public bool IsActive { get; set; }
    }
}
