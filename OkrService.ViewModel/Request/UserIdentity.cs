
using System;

namespace OKRService.ViewModel.Request
{
    public class UserIdentity
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long RoleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public bool IsActive { get; set; }
        public string ReportingTo { get; set; }
        public string ImageDetail { get; set; }
        public long OrganisationId { get; set; }
        public DateTime? LastLoginDateTime { get; set; }
    }
}
