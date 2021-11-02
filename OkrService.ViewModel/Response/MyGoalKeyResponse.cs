using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class MyGoalKeyResponse
    {
        public long GoalKeyId { get; set; }
        public long EmployeeId { get; set; }
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }       
        public decimal Score { get; set; }
        public DateTime KeyProgressTime { get; set; }
        public bool IsNewItem { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public long Source { get; set; }
        public bool IsAnyFeedback { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
    }
}
