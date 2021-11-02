
using System;

namespace OKRService.ViewModel.Request
{
    public class UpdateGoalDescriptionRequest
    {
        public long GoalId { get; set; }
        public int GoalType { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
    }
}
