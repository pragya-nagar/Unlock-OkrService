using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Request
{
    public class ContributorKeyResultRequest
    {
        public long GoalKeyId { get; set; }
        public long GoalObjectiveId { get; set; }
        public string KeyDescription { get; set; }
        public string ObjectiveName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int KrStatusId { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public int Progress { get; set; }
        public long Source { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal StartValue { get; set; }
        public decimal TargetValue { get; set; }
        public int AssignmentTypeId { get; set; }
        public string KrAssignerMessage { get; set; }
        public string KrAssigneeMessage { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public string KeyNotes { get; set; }
        public string ObjectiveDescription { get; set; }
        public long Owner { get; set; }
        public long NotificationDetailsId { get; set; }
        public long TeamId { get; set; }
        public bool IsSelf { get; set; }
        public List<ContributorDetails> Contributors { get; set; }
        public int GoalTypeId { get; set; }

    }
}
