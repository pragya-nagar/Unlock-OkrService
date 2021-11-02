using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class TeamDetails
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long? ParentId { get; set; }
        public long? OrganisationHead { get; set; }
        public string ImagePath { get; set; }
        public long MembersCount { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public string LeaderFirstName { get; set; }
        public string LeaderLastName { get; set; }
        public string LeaderDesignation { get; set; }
        public List<TeamEmployeeDetails> TeamEmployees { get; set; }
    }
    public class TeamEmployeeDetails
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string ImagePath { get; set; }
        public long OrganisationId { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public bool IsAlreadyAligned { get; set; }
    }
}
