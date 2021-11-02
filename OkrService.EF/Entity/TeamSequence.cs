using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class TeamSequence
    {
        public long TeamSequenceId { get; set; }
        public long TeamId { get; set; }
        public long EmployeeId { get; set; }
        public long CycleId { get; set; }
        public int Sequence { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
