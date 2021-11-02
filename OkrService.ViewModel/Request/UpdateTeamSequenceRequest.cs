
namespace OKRService.ViewModel.Request
{
    public class UpdateTeamSequenceRequest
    {
        public long TeamId { get; set; }
        public int Sequence { get; set; }
        public int CycleId { get; set; }
    }
}
