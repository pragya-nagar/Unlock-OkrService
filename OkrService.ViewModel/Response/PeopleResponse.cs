
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class PeopleResponse
    {
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsScoreLocked { get; set; }
        public decimal AvgScore { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Team { get; set; }
        public int AtRisk { get; set; }
        public int OnTrack { get; set; }
        public int Lagging { get; set; }
        public int NotStarted { get; set; }
        public List<PeopleOkrResponse> MyGoalOkrResponses { get; set; }
    }
}
