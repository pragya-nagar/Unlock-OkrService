
namespace OKRService.ViewModel.Response
{
    public class KeyContributorsResponse
    {
        public long GoalKeyId { get; set; }
        public long? EmployeeId { get; set; } = null;
        public long OrganisationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public int KrUpdates { get; set; }      
        public string KeyDescription { get; set; }  
        public decimal Score { get; set; }
        public int Progress { get; set; }
    }
}
