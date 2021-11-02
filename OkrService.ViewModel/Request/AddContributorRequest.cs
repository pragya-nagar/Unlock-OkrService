using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Request
{
    public class AddContributorRequest
    {
        public long GoalKeyId { get; set; }
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public int GoalStatusId { get; set; }
        public int GoalTypeId { get; set; }
        public decimal Score { get; set; }
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }
        public int KrStatusId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int AssignmentTypeId { get; set; }
        public string KrAssignerMessage { get; set; }
        public decimal StartValue { get; set; }
        public int MetricId { get; set; }
        public int CurrencyId { get; set; }
        public int ObjectiveCycleId { get; set; }
        public string ObjectiveName { get; set; }
        public long TeamId { get; set; }
        public bool IsSelf { get; set; }
        public List<ContributorDetails> Contributors { get; set; }

    }
}
