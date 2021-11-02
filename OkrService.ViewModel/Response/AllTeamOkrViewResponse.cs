using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AllTeamOkrViewResponse
    {
        public long EmployeeId { get; set; }
        public long Owner { get; set; }
        public int ObjectiveType { get; set; }
        public long ObjectiveId { get; set; }
        public string Name { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public decimal TeamScore { get; set; }
        public string ProgressCode { get; set; }
        public int KeyResultCount { get; set; }
        public int ObjectiveCount { get; set; }
        public int TeamProgress { get; set; }
        public long MemberCount { get; set; }
        public int Sequence { get; set; }
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
        public bool IsCollaborator { get; set; }
        public decimal LastSevenDaysProgress { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public List<OkrViewKeyResults> OkrViewKeyResults { get; set; }
        public List<OkrViewContributors> OkrViewContributors { get; set; }
        public LeaderDetailsAlignmentResponse LeaderDetails { get; set; }

    }
}
