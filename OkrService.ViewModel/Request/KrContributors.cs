
using System;

namespace OKRService.ViewModel.Request
{
    public class KrContributors
    {
        public long EmployeeId { get; set; }
        public long GoalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public long Owner { get; set; }
        public long GoalObjectiveId { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }

        public string KeyDescription { get; set; }
        public decimal Score { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } 
        public bool IsActive { get; set; } = true;
        public int Progress { get; set; }
        public long Source { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public int CycleId { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal StartValue { get; set; }
        public long TeamId { get; set; }
        public string KeyNotes { get; set; } = "";
      

    }
}
