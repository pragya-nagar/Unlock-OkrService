using System;

namespace OKRService.ViewModel.Response
{
    public class ContributorsDotResponse
    {
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }   
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }     
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public string ScoreRange { get; set; }
        public int KrCount { get; set; }
        public int ObjCount { get; set; }
        public long OrganisationId { get; set; }
        public DateTime DueDate { get; set; }      
    }
}
