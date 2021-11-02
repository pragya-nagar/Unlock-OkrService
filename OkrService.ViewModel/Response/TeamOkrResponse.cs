
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class TeamOkrResponse
    {
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public long? LeaderEmployeeId { get; set; }
        public string LeaderName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public decimal TeamAvgScore { get; set; }
        public int TeamProgress { get; set; }
        public string ProgressCode { get; set; }
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public long TeamEmployeeCount { get; set; }
        public List<ProgressDetail> NotStarted { get; set; }
        public List<ProgressDetail> AtRisk { get; set; }
        public List<ProgressDetail> Lagging { get; set; }
        public List<ProgressDetail> OnTrack { get; set; }
        public List<DashboardOkrKRResponse> MyGoalOkrResponses { get; set; }
        public bool AlertMessage { get; set; }
        public decimal LastSevenDaysProgress { get; set; }
        public List<ContributorsLastSevenDaysProgressResponse> ContributorsLastSevenDaysProgress { get; set; }
        public bool IsDeltaVisible { get; set; } = false;
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
    }
}
