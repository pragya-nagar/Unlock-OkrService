
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class PeopleMapResponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public int PeopleFollow { get; set; }
        public int ObjectiveCount { get; set; }
        public bool IsLock { get; set; }
        public List<PeopleAlignmentResponse> PeopleAlignmentResponse { get; set;}
    }
}
