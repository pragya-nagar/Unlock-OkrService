using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class UserGoalKeyResponse
    {
        public long GoalKeyId { get; set; }
        public long? EmployeeId { get; set; } = null;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }   
        public string ObjectiveDescription { get; set; }
        public string ObjectiveName { get; set; }
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public int KrUpdate { get; set; }
        public int UserKrUpdate { get; set; }
        public List<KeyContributorsResponse> KeyContributorsResponses { get; set; }
    }
}
