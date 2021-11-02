
namespace OKRService.ViewModel.Response
{
    public class ProgressReportResponse
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal PrivateOkrScore { get; set; }
        public decimal OrgOkrScore { get; set; }
        public decimal TotalOkrScore { get; set; }
    }

    public class ProgressReport
    {
        public long EmployeeId { get; set; }
        public decimal PrivateOkrScore { get; set; }
        public decimal OrgOkrScore { get; set; }
        public decimal TotalOkrScore { get; set; }
    }
}
