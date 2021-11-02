using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class UnlockSupportTeam
    {
        public long Id { get; set; }
        public string EmailId { get; set; }
        public string FullName { get; set; }
    }
}
