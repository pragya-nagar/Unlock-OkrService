using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class LastSevenDaysProgressResponse
    {
        public decimal Score { get; set; }
        public DateTime? GoalProgressTime { get; set; }
    }

    public class ContributorsLastSevenDaysProgressResponse
    {       
        public long? EmployeeId { get; set; } = null;
        public long? OrganisationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public decimal ContributorsContribution { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class LastSevenDaysStatusCardProgress
    {
        public decimal LastSevenDaysProgressAtRisk { get; set; }
        public decimal LastSevenDaysProgressLagging { get; set; }
        public decimal LastSevenDaysProgressOnTrack { get; set; }
    }
}
