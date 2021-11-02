using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class ContributorsResponse
    {
        public int GoalType { get; set; }
        public long? GoalId { get; set; }
        public long? EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public int AssignmentTypeId { get; set; }
        public string KeyResult { get; set; }
        public string ObjectiveName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public int KrStatusId { get; set; }
        public string KrAssigneeMessage { get; set; }
        public string KrAssignerMessage { get; set; }
        public int GoalStatusId { get; set; }
        public decimal StartValue { get; set; }
        public decimal ContributorsContribution { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsSource { get; set; }
        public DateTime? CreatedOnAssignee { get; set; }
        public DateTime? CreatedOnAssigner { get; set; }
        public decimal LastLoginScore { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public bool IsSelfCreation { get; set; }
        public bool IsExternal { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public int MetricId { get; set; }
        public int CurrencyId { get; set; }

    }

    public class ReportContributorsWithScore
    {
        public long? EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public decimal Score { get; set; }
        public DateTime EndDate { get; set; }
        public List<ReportContributorsWithScore> SecondLevelContributors { get; set; }
    }
}
