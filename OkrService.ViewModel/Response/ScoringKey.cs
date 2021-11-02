
namespace OKRService.ViewModel.Response
{
    public class ScoringKey
    {
        public long GoalObjectiveId { get; set; }
        public long GoalKeyId { get; set; }
        public decimal DeltaScore { get; set; }
        public decimal CurrentScore { get; set; }
        public int ProgressId { get; set; }
    }
}
