using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class MetricDataMasterResponse
    {
        public int DataId { get; set; }
        public int MetricId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Symbol { get; set; }
    }
}
