
using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
   public  class DirectReportsObjectives
    {
        public int ObjectiveType { get; set; }
        public long GoalObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Score { get; set; }
        public string Cycle { get; set; }
        public int Progress { get; set; }
        public int GoalTypeId { get; set; }
        public int GoalStatusId { get; set; }
        public List<DirectReportsKeyResult> DirectReportsKeyResults { get; set; }
    }
}
