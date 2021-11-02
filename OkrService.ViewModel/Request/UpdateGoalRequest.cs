using System;

namespace OKRService.ViewModel.Request
{
    public class UpdateGoalRequest
    {
        public long GoalObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public long LinkedObjectiveId { get; set; }
        public long Owner { get; set; }
        public int GoalTypeId { get; set; }
        public long TeamId { get; set; }
        public bool IsCoach { get; set; }
    }
}
