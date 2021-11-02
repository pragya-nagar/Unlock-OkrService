using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class MetricMasterResponse
    {
        public int MetricId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<MetricDataMasterResponse> MetricDataMaster { get; set; }
        public bool IsActive { get; set; }
    }

    public class AllOkrMasterData
    {
        public List<GoalTypeResponse> GoalTypes { get; set; }
        public List<GoalStatusResponse> GoalStatus { get; set; }
        public List<KrStatusResponse> KrStatus { get; set; }
        public List<MetricMasterResponse> MetricMasters { get; set; }
        public List<AssignmentTypeResponse> AssignmentTypes { get; set; }

    }
}
