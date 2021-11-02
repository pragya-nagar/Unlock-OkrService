using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class UnLockLog
    {
        public long UnLockLogId { get; set; }
        public int? Year { get; set; }
        public int? Cycle { get; set; }
        public long? EmployeeId { get; set; }
        public DateTime? LockedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LockedTill { get; set; } = DateTime.UtcNow.AddHours(48);
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? Status { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
