
namespace OKRService.ViewModel.Response
{
    public class QuarterReportResponse
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameRTo { get; set; } = "";
        public string LastNameRTo { get; set; } = "";
        public string ObjectiveName { get; set; }
        public string ObjectiveDesc { get; set; }
        public decimal Score { get; set; }
        public string Cycle { get; set; }
        public string StandAloneKeyDesc { get; set; }
    }
}