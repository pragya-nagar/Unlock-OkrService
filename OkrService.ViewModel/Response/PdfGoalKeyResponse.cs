using System;
using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class PdfGoalKeyResponse
    {
        public string KeyDescription { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Score { get; set; }
        public List<ContributorPdfResponse> Contributors { get; set; }
    }

}
