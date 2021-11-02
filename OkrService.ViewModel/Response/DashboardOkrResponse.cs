using System;
using System.Collections.Generic;
using OKRService.ViewModel.Request;

namespace OKRService.ViewModel.Response
{
    public class DashboardOkrResponse
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
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DueCycle { get; set; }
        public int Progress { get; set; }
        public DateTime GoalProgressTime { get; set; }
        public decimal Score { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public bool IsAnyFeedback { get; set; }
        public int GoalTypeId { get; set; }
        public int GoalStatusId { get; set; }
        public int Sequence { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
        public List<DashboardKeyResponse> MyGoalsDetails { get; set; }
    }

    public class DashboardOkrKRResponse
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
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DueCycle { get; set; }
        public int Progress { get; set; }
        public DateTime GoalProgressTime { get; set; }
        public decimal Score { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public bool IsAnyFeedback { get; set; }
        public int GoalTypeId { get; set; }
        public int GoalStatusId { get; set; }
        public int Sequence { get; set; }
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public DateTime KeyProgressTime { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public decimal StartValue { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ContributorValue { get; set; }
        public string KeyNotes { get; set; }
        public bool IsLastStatusDraft { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public long Owner { get; set; }
        public string OwnerDesignation { get; set; }
        public string OwnerEmailId { get; set; }
        public string OwnerEmployeeCode { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerImagePath { get; set; }
        public string OwnerLastName { get; set; }
        public long CreatedBy { get; set; }
        public List<DashboardKeyResponse> MyGoalsDetails { get; set; }
        public List<TeamOkrRequest> TeamOkrRequests { get; set; }
        public LinkedObjectiveResponse LinkedObjective { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsAligned { get; set; }
        public ParentTeamDetails ParentTeamDetail { get; set; }
        public long TeamMembersCount { get; set; }
        public string TeamLogo { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsCollaborator { get; set; }
    }
}
