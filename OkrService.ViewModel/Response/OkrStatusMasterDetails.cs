using System.Collections.Generic;

namespace OKRService.ViewModel.Response
{
    public class OkrStatusMasterDetails
    {
        public IList<OkrStatusDetails> OkrStatusDetails { get; set; }
        public IList<ObjectiveDetails> ObjectiveDetails { get; set; }
    }

    public class OkrStatusDetails
    {
        public int Id { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
    }

    public class ObjectiveDetails
    {
        public long ObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public bool IsActive { get; set; }
    }
}
