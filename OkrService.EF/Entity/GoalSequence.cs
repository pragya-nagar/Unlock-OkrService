using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class GoalSequence
    {
        public long SequenceId { get; set; }
        public long GoalId { get; set; }
        public long GoalType { get; set; }
        public long EmployeeId { get; set; }
        public long GoalCycleId { get; set; }
        public int Sequence { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}