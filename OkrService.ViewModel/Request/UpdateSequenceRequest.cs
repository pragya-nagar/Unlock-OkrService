namespace OKRService.ViewModel.Request
{
    public class UpdateSequenceRequest
    {
        public long GoalId { get; set; }
        public int Sequence { get; set; }
        public int GoalType { get; set; }
        public int GoalCycleId { get; set; }
    }
}
