using System;

namespace OKRService.ViewModel.Request
{
    public class ContributorDetails
    {
        public long EmployeeId { get; set; }
        public int AssignmentTypeId { get; set; }
        public string ObjectiveName { get; set; }
        public string KeyResult { get; set; }
        public decimal Score { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public string KrAssignerMessage { get; set; }
        public int KrStatusId { get; set; }
        public int GoalStatusId { get; set; }
        public int CycleId { get; set; }
        public int Year { get; set; }
        public decimal StartValue { get; set; }
        public long TeamId { get; set; }
        public bool IsTeamSelected { get; set; }

    }
}
