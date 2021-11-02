
using System.Diagnostics.CodeAnalysis;

namespace OKRService.EF
{
    [ExcludeFromCodeCoverage]
    public partial class MessageMaster
    {
        public int MessageMasterId { get; set; }
        public string MessageDesc { get; set; }
        public bool IsActive { get; set; } = true;
    }
}