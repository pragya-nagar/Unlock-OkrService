using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AllOkrViewResponse
    {
        public List<OkrViewResponse> OkrViewResponses { get; set; }
    }
    public class OkrViewResponse
    {
        public long EmployeeId { get; set; }
        public long Owner { get; set; }
        public string OwnerDesignation { get; set; }
        public string OwnerEmailId { get; set; }
        public string OwnerEmployeeCode { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerImagePath { get; set; }
        public string OwnerLastName { get; set; }
        public bool IsMyOkr { get; set; }
        public int ObjectiveType { get; set; }
        public long ObjectiveId { get; set; }
        public string Name { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Score { get; set; }
        public string Cycle { get; set; }
        public int KrStatusId { get; set; }
        public int Progress { get; set; }
        public bool IsContributorExist { get; set; }
        public bool IsSourceExist { get; set; }
        public bool IsPrivate { get; set; }
        public int ObjectiveTypeId { get; set; }
        public int ObjectiveStatusId { get; set; }
        public decimal KrCurrentValue { get; set; }
        public decimal KrStartValue { get; set; }
        public decimal KrTargetValue { get; set; }
        public bool IsSourceLinked { get; set; }
        public int ContributorCount { get; set; }
        public long ParentId { get; set; }
        public string ObjectiveUniqueId { get; set; }
        public List<string> Parent { get; set; }
        public int Index { get; set; }
        public bool IsAnyFeedback { get; set; }
        public int CurrencyId { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public bool IsAligned { get; set; }
        public string ObjectiveDescription { get; set; }
        public string KeyNotes { get; set; }
        public string KrUniqueId { get; set; }
        public string KrParentId { get; set; }
        public long ImportedId { get; set; }
        public int ActionLevel { get; set; }
        public decimal InValue { get; set; }
        public decimal OutValue { get; set; }
        public bool IsContributor { get; set; }
        public string OkrOwner { get; set; }
        public bool SourceResponse { get; set; }
        public string CurrencyInValue { get; set; }
        public string CurrencyOutValue { get; set; }
        public bool IsUnreadFeedback { get; set; }
        public long LinkedObjectiveId { get; set; }
        public bool IsVirtualLink { get; set; } = false;
        public bool IsParentVirtualIcon { get; set; } = false;
        public bool IsParentVirtualLink { get; set; } = false;
        public List<OkrViewKeyResults> OkrViewKeyResults { get; set; }
        public List<OkrViewContributors> OkrViewContributors { get; set; }
        public List<ContributorsResponse> OkrViewStandAloneContributors { get; set; }
        public ParentTeamDetails ParentTeamDetails { get; set; }
        public DateTime? ParentStartDate { get; set; } = null;
        public DateTime? ParentDueDate { get; set; } = null;

    }
    public class OkrViewContributors
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
        public string UserType { get; set; }
    }

    public class OkrViewKeyResults
    {
        public long EmployeeId { get; set; }
        public long KrId { get; set; }
        public string KrName { get; set; }
        public decimal KrScore { get; set; }
        public decimal KrCurrentValue { get; set; }
        public decimal KrContributorValue { get; set; }
        public int KrProgress { get; set; }
        public decimal KrStartValue { get; set; }
        public decimal KrTargetValue { get; set; }
        public bool IsSourceLinked { get; set; }
        public int ContributorCount { get; set; }
        public string ParentId { get; set; }
        public DateTime KrDueDate { get; set; }
        public int KrStatusId { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public bool IsAnyFeedback { get; set; }
        public int CurrencyId { get; set; }
        public int MetricId { get; set; }
        public int AssignmentTypeId { get; set; }
        public string KeyNotes { get; set; }
        public string KrUniqueId { get; set; }
        public int ActionLevel { get; set; }
        public long ImportedId { get; set; }
        public decimal InValue { get; set; }
        public decimal OutValue { get; set; }
        public long Owner { get; set; }
        public string OwnerDesignation { get; set; }
        public string OwnerEmailId { get; set; }
        public string OwnerEmployeeCode { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerImagePath { get; set; }
        public string OwnerLastName { get; set; }
        public bool IsContributor { get; set; }
        public string OkrOwner { get; set; }
        public string CurrencyInValue { get; set; } = "0";
        public string CurrencyOutValue { get; set; } = "0";
        public bool IsUnreadFeedback { get; set; }
        public bool IsVirtualLink { get; set; } = false;
        public bool IsParentVirtualLink { get; set; } = false;
        public bool IsCollaborator { get; set; }
        public List<ContributorsResponse> OkrViewKeyContributors { get; set; }

    }

}
