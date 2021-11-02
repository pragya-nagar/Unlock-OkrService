using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class TeamHeadDetails
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long? OrganisationHead { get; set; }
        public string ImagePath { get; set; }
        public long TeamCount { get; set; }
        public long MembersCount { get; set; }
        public string ParentName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public string ParentTeamColorCode { get; set; }
        public string ParentTeamBackGroundColorCode { get; set; }
        public List<TeamEmployeeDetails> TeamEmployees { get; set; }
    }
}
