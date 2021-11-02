using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class KrStatusContributorResponse
    {
        public long? GoalKeyId { get; set; }
        public long? EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public int KrStatusId { get; set; }
        public string KrAssigneeMessage { get; set; }
        public string KrAssignerMessage { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TargetValue { get; set; }
        public DateTime DueDate { get; set; }
        public string KeyResult { get; set; }
        public string ObjectiveName { get; set; }
        public int AssignmentTypeId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
