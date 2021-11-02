
namespace OKRService.ViewModel.Request
{
    public class NudgeTeamRequest
    {
        public string TeamName { get; set; }
        public long TeamId { get; set; }
        public int Cycle { get; set; } 
        public int Year { get; set; }
    }
}
