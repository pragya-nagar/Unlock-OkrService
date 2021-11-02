using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class MyGoalDetailResponse
    {
        public long GoalObjectiveId { get; set; }
        public string ObjectiveDescription { get; set; }
        public string ObjectiveName { get; set; }
        public long EmployeeId { get; set; }
        public List<GoalKeyDetails> KeyDetails { get; set; } = new List<GoalKeyDetails>(); 
        public long TeamId { get; set; }
        public string TeamName { get; set; }
    }

    public class GoalKeyDetails
    {
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public long EmployeeId { get; set; }
    }
}
