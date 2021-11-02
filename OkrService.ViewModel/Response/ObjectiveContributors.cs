using System;

namespace OKRService.ViewModel.Response
{
    public class ObjectiveContributors
    {
        public int GoalType { get; set; }
        public long GoalId { get; set; }
        public long EmployeeId { get; set; }
        public decimal Score { get; set; }
        public DateTime EndDate { get; set; }
    }
}
