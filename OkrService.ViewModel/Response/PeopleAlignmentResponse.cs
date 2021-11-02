
namespace OKRService.ViewModel.Response
{
    public class PeopleAlignmentResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public long EmployeeId { get; set; }
        public int ObjectiveCount { get; set; }
        public int KeyResultCount { get; set; }
        public long AlignFromId { get; set; }
        public long AlignToId { get; set; }
        public int AlignLevel { get; set; }
        public string SourceAlign { get; set; }
    }
}
