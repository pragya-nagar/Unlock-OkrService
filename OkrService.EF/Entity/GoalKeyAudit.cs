using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class GoalKeyAudit
    {
        public long GoalKeyAuditId { get; set; }
        public long? UpdatedGoalKeyId { get; set; }
        public string UpdatedColumn { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; } = null;
    }
}
