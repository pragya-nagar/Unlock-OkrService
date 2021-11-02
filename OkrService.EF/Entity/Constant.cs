using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class Constant
    {
        public long ConstantId { get; set; }
        public string ConstantName { get; set; }
        public string ConstantValue { get; set; }
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public long? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
