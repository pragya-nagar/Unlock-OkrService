using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class PdfOkrResponse
    {
        public string ObjectiveName { get; set; }
        public decimal Score { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PdfGoalKeyResponse> pdfGoalKeyResponses { get; set; }
    }
}
