using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AllOkrDashboardKeyResponse
    {
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }
        public int Progress { get; set; }
        public decimal Score { get; set; }
        public DateTime KeyProgressTime { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public bool IsAnyFeedback { get; set; }
        public DateTime StartDate { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public int CurrencyId { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
        public string CurrencyCode { get; set; }
        public int GoalStatusId { get; set; }
        public decimal ContributorValue { get; set; }
        public decimal StartValue { get; set; }
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
        public bool IsCollaborator { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsAligned { get; set; }
        public long TeamMembersCount { get; set; }
        public string TeamLogo { get; set; }
        public ParentTeamDetails ParentTeamDetail { get; set; }
    }
}
