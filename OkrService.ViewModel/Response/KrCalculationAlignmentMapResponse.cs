
namespace OKRService.ViewModel.Response
{
    public class KrCalculationAlignmentMapResponse
    {
        public long OkrId { get; set; }
        public long KrId { get; set; }
        public decimal KrScore { get; set; }
        public decimal OkrScore { get; set; }
        public decimal InValue { get; set; }
        public decimal OutValue { get; set; }
        public string CurrencyInValue { get; set; }
        public string CurrencyOutValue { get; set; }
        public decimal KrCurrentValue { get; set; }
    }
}
