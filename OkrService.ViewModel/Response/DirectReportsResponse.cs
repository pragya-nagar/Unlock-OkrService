
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class DirectReportsResponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string ImagePath { get; set; }
        public long OrganisationId { get; set; }
        public string OrganizationName { get; set; }
        public int Contributions { get; set; }
        public int ObjectivesCount { get; set; }
        public int KeyResultCount { get; set; }
        public decimal Score { get; set; }
        public int Progress { get; set; }
        public string DirectReportColorCode { get; set; }
        public decimal TeamScore { get; set; }
        public int TeamProgress { get; set; }
        public string TeamColorCode { get; set; }
        public int ContributorsCount { get; set; }
        public decimal LastSevenDaysProgress { get; set; }
        public List<DirectReportsContributorsResponse> DirectReportsContributors { get; set; }
        public List<DirectReportsObjectives> DirectReportsObjectives { get; set; }
        public List<ContributorsLastSevenDaysProgressResponse> ContributorsLastSevenDaysProgress { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public bool IsDeltaVisible { get; set; } = false;

        public int MostProgress { get; set; }
        public int LeastProgress7days { get; set; }
        public int LeastProgress15days { get; set; }
        public int LeastProgress30days { get; set; }
    }
}
