using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class GoalKeyHistory
    {
        public long HistoryId { get; set; }
        public long GoalKeyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal Score { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public int Progress { get; set; }
    }
}
