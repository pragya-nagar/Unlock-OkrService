using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class AlignmentMapOkrs
    {
        public long GoalObjectiveId { get; set; }
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public int Year { get; set; }
        public bool IsPrivate { get; set; }
        public string ObjectiveName { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Progress { get; set; }
        public decimal Score { get; set; }
        public long GoalKeyId { get; set; }
        public string KeyDescription { get; set; }
        public int AssignmentTypeId { get; set; }
        public long TeamId { get; set; }
        public int KrStatusId { get; set; }
        public List<KeysResponse> AlignedKeys { get; set; }
    }
}
