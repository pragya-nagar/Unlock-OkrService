
namespace OKRService.ViewModel.Request
{
    public class KeyScoreUpdate
    {
        public long GoalObjectiveId { get; set; }
        public decimal GoalObjectiveScore { get; set; }
        public int GoalObjectiveProgress { get; set; }
        public long GoalKeyId { get; set; }
        public decimal GoalKeyScore { get; set; }
        public int GoalKeyProgress { get; set; }
    }
}

