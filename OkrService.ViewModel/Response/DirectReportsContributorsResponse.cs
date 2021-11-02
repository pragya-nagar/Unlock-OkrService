using System;

namespace OKRService.ViewModel.Response
{
    public class DirectReportsContributorsResponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
        public string UserType { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public decimal Score { get; set; }
    }
}
