using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class ReportMostLeastObjective
    {
        public long GoalObjectiveId { get; set; }        
        public int Year { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public long ObjectiveCycleId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string DueCycle { get; set; }
        public decimal Score { get; set; }      
        public long ImportedId { get; set; }      
        public List<ReportContributorsWithScore> Contributors { get; set; }       
    }

    public class ReportMostLeastObjectiveKeyResult
    {
        public long GoalKeyId { get; set; }
        public int Year { get; set; }       
        public string KeyDescription { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string DueCycle { get; set; }
        public decimal Score { get; set; }
        public long ImportedId { get; set; }
        public List<ReportContributorsWithScore> Contributors { get; set; }
    }
}
