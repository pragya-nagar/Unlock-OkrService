using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class ErrorLog
    {
        public long LogId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string PageName { get; set; }
        public string FunctionName { get; set; }
        public string ApplicationName { get; set; } = "OkrService";
        public string ErrorDetail { get; set; }
    }
}
