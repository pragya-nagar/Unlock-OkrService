
namespace OKRService.ViewModel.Response
{
    public class TeamOkrCardResponse
    {
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public string TeamLogo { get; set; }
        public decimal TeamScore { get; set; }
        public int Progress { get; set; }
        public string ProgressCode { get; set; }
        public int KeyCount { get; set; }
        public int Sequence { get; set; }
        public LeaderDetailsResponse LeaderDetails { get; set; }
        public long MembersCount { get; set; }
    }
}
