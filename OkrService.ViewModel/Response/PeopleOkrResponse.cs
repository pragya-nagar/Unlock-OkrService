using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class PeopleOkrResponse
    {
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public int Year { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public long ObjectiveCycleId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DueCycle { get; set; }
        public decimal Score { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public int Progress { get; set; }
        public bool IsAnyFeedback { get; set; }
        public int GoalTypeId { get; set; }
        public int GoalStatusId { get; set; }
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public decimal StartValue { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ContributorValue { get; set; }
        public DateTime GoalProgressTime { get; set; }
        public DateTime KeyProgressTime { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public bool IsContributor { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
        public List<PeopleKeyResponse> MyGoalsDetails { get; set; }
        public ParentTeamDetails ParentTeamDetail { get; set; }
    }
}
