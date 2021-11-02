using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class MyGoalsPdfResponse
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public decimal AverageScore { get; set; }
        public string MyGoalsPeriod { get; set; }
        public int OkrCount { get; set; }
        public int KeyCount { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Team { get; set; }
        public int AtRisk { get; set; }
        public int OnTrack { get; set; }
        public int Lagging { get; set; }
        public int NotStarted { get; set; }
        public List<PdfUniqueContributorsResponse> pdfUniqueContributorsResponses { get; set; }
        public List<PdfOkrResponse> pdfOkrResponses { get; set; }
    }   
}
