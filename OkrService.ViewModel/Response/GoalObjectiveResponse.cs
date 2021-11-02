using System;

namespace OKRService.ViewModel.Response
{
    public class GoalObjectiveResponse
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
    }
}
