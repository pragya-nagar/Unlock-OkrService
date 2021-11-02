using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class WeeklyReportResponse
    {
        public List<KrGrowthCycleResponse> Week2 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Week4 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Week6 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Week8 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Week10 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Week12 { get; set; } = new List<KrGrowthCycleResponse>();

        public List<KrGrowthCycleResponse> Month1 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Month2 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Month3 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Month4 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Month5 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Month6 { get; set; } = new List<KrGrowthCycleResponse>();

        public List<KrGrowthCycleResponse> Year1 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Year2 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Year3 { get; set; } = new List<KrGrowthCycleResponse>();

        public List<KrGrowthCycleResponse> Months1 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months2 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months3 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months4 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months5 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months6 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months7 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months8 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months9 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months10 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months11 { get; set; } = new List<KrGrowthCycleResponse>();
        public List<KrGrowthCycleResponse> Months12 { get; set; } = new List<KrGrowthCycleResponse>();
    }

    public class KrGrowthCycleResponse
    {
        public long GoalKeyId { get; set; }
        public long? EmployeeId { get; set; } = null;
        public string ObjectiveDescription { get; set; }
        public string ObjectiveName { get; set; }
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }
        public string Score { get; set; }
        public decimal GoalScore { get; set; }
        public int Progress { get; set; }
        public int KrUpdate { get; set; }
        public int CycleStatus { get; set; }
        public List<KeyContributorsResponse> KeyContributorsResponses { get; set; }
    }
}
