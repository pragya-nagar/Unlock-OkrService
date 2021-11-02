using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class GoalObjective
    {
        public GoalObjective()
        {
            GoalKey = new HashSet<GoalKey>();
        }

        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; } = "";
        public int ObjectiveCycleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public decimal Score { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public int Year { get; set; }
        public int Progress { get; set; }
        public long Source { get; set; }
        public int Sequence { get; set; }
        public int GoalStatusId { get; set; }
        public int GoalTypeId { get; set; }
        public long Owner { get; set; }
        public long TeamId { get; set; }
        public long LinkedObjectiveId { get; set; }
        public bool IsCoachCreation { get; set; } = false;
        public virtual ICollection<GoalKey> GoalKey { get; set; }
    }
}
