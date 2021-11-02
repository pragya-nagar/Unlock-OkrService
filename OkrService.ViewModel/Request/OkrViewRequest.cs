using System;
using System.Collections.Generic;
using System.Text;
using OKRService.ViewModel.Response;

namespace OKRService.ViewModel.Request
{
    public class OkrViewRequest
    {
        public long EmployeeId { get; set; }
        public string EmployeeUniqueId { get; set; }
        public string EmployeeParentId { get; set; }
        public int CycleId { get; set; }
        public int Year { get; set; }
        public bool IsNested { get; set; }
        public bool IsParent { get; set; }
        public bool IsSourceExist { get; set; }
        public bool IsContributorExist { get; set; }
        public long OrgId { get; set; }
        public string Token { get; set; }
        public int ActionLevel { get; set; }
        public UserIdentity UserResponse { get; set; }
        public long LoggedInUserId { get; set; }
        public CycleDetails CycleDetail { get; set; }
        public OrganisationCycleDetails OrganisationCycleDetails { get; set; }
        public List<long> ParentObjList { get; set; }
        public EmployeeResult AllEmployee { get; set; }

    }
}
