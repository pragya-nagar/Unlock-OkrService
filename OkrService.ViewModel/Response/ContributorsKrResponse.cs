using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class ContributorsKrResponse
    {
        public long GoalKeyId { get; set; }
        public long? EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public string KeyDescription { get; set; }
        public string KeyNote { get; set; }
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public string ScoreRange { get; set; }
        public int KrCount { get; set; }
        public long OrganisationId { get; set; }
        public DateTime DueDate { get; set; }
    }
}
