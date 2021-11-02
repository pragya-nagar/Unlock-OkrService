using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Request
{
    public class MyGoalsRequest
    {
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public int Year { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public int ObjectiveCycleId { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public long Source { get; set; }
        public int Sequence { get; set; }
        public int GoalStatusId { get; set; }
        public int GoalTypeId { get; set; }
        public decimal Score { get; set; }
        public long Owner { get; set; }
        public long LinkedObjectiveId { get; set; }
        public List<MyGoalsDetails> MyGoalsDetails { get; set; }
        public List<TeamOkrRequest> TeamOkrRequests { get; set; }
        public bool IsSavedAsDraft { get; set; }
        public bool IsCoach { get; set; }
    }

    public class MyGoalsDetails
    {
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public decimal Score { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public int Progress { get; set; }
        public long Source { get; set; }
        public List<ContributorDetails> Contributors { get; set; }
        public int MetricId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int AssignmentTypeId { get; set; }
        public int KrStatusId { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal StartValue { get; set; }
        public string KeyNotes { get; set; }
        public long Owner { get; set; }
    }
}
