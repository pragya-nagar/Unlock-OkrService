using System;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class TypeOfGoalCreation
    {
        public long TypeOfGoalCreationId { get; set; }
        public string PrimaryText { get; set; }
        public string SecondaryText { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; } = null;
        public DateTime? UpdatedOn { get; set; } = null;
        public bool IsActive { get; set; } = true;
    }
}
