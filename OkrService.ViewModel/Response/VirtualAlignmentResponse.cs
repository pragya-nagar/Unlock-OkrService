using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Response
{
    public class VirtualAlignmentResponse
    {
        public long GoalObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public string DueCycle { get; set; }
        public int DueYear { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public List<VirtualDetail> VirtualDetails { get; set; }
    }

    public class VirtualDetail
    {
        public long GoalObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public string DueCycle { get; set; }
        public int DueYear { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
        public string FirstName { get; set; }
        public long EmployeeId { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public DateTime DueDate { get; set; }
    }
}
