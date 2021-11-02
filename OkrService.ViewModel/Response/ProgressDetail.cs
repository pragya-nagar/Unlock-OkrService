
namespace OKRService.ViewModel.Response
{
    public class ProgressDetail
    {
        public int ObjectiveCount { get; set; }
        public int KeyResultCount { get; set; }
        public decimal Score { get; set; }
        public decimal LastSevenDaysProgress { get; set; }
        public string Description { get; set; }
    }
}
