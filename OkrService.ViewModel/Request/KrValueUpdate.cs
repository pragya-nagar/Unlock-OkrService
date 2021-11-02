
namespace OKRService.ViewModel.Request
{
    public class KrValueUpdate
    {
        public long GoalKeyId { get; set; }
        public decimal CurrentValue { get; set; }
        public int Year { get; set; }
    }
}
