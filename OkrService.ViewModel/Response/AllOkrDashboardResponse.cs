using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AllOkrDashboardResponse
    {
        public string WelcomeMessage { get; set; }
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsUnLockRequested { get; set; }
        public bool IsScoreLocked { get; set; }
        public decimal AvgScore { get; set; }
        public string ProgressCode { get; set; }
        public List<AllOkrDashboardProgressDetail> NotStarted { get; set; }
        public List<AllOkrDashboardProgressDetail> AtRisk { get; set; }
        public List<AllOkrDashboardProgressDetail> Lagging { get; set; }
        public List<AllOkrDashboardProgressDetail> OnTrack { get; set; }
        public List<AllOkrDashboardOkrKRResponse> MyGoalOkrResponses { get; set; }
        public int ContributorsCount { get; set; }
        public bool IsDeltaVisible { get; set; } = false;
        public bool IsFirstTimeUser { get; set; } = false;
        public DateTime GoalSubmitDate { get; set; }
        public bool ToShowGuidedTour { get; set; } = true;
    }
}
