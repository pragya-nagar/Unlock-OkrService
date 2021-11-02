using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class AlignmentMapResponse
    {
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public int Year { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public long ObjectiveCycleId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DueCycle { get; set; }
        public decimal Score { get; set; }
        public int ImportedType { get; set; }
        public long ImportedId { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsNewItem { get; set; }
        public long Source { get; set; }
        public int AlignLevel { get; set; }
        public bool IsAnyFeedback { get; set; }
        public List<ContributorsResponse> Contributors { get; set; }
        public List<MyGoalKeyResponse> MyGoalsDetails { get; set; }       
    }

    public class GoalMapAlignment
    {
        public long GoalObjId { get; set; }
        public int AlignLevel { get; set; }
    }
}
 