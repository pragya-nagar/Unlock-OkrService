using System;
using System.Collections.Generic;
using System.Text;

namespace OKRService.ViewModel.Request
{
    public class Gremlin
    {
        public class UserProperties
        {
            public List<PropertyDetail> FirstName { get; set; }
            public List<PropertyDetail> LastName { get; set; }
            public List<PropertyDetail> EmployeeCode { get; set; }
            public List<PropertyDetail> EmailId { get; set; }
            public List<PropertyDetail> ImagePath { get; set; }
            public List<PropertyDetail> Designation { get; set; }
            public List<PropertyDetail> pk { get; set; }
        }
        public class UserModel
        {
            public string id { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public UserProperties properties { get; set; }
        }

        public class ObjProperties
        {
            public List<PropertyDetail> ObjectiveName { get; set; }
            public List<PropertyDetail> Score { get; set; }
            public List<PropertyDetail> Progress { get; set; }
            public List<PropertyDetail> IsPrivate { get; set; }
            public List<PropertyDetail> StartDate { get; set; }
            public List<PropertyDetail> EndDate { get; set; }
            public List<PropertyDetail> Year { get; set; }
            public List<PropertyDetail> ObjectiveCycleId { get; set; }
            public List<PropertyDetail> ObjectiveDescription { get; set; }
            public List<PropertyDetail> pk { get; set; }
        }
        public class ObjModel
        {
            public string id { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public ObjProperties properties { get; set; }
        }

        public class KRProperties
        {
            public List<PropertyDetail> KRName { get; set; }
            public List<PropertyDetail> DueDate { get; set; }
            public List<PropertyDetail> Score { get; set; }
            public List<PropertyDetail> Progress { get; set; }
            public List<PropertyDetail> pk { get; set; }
        }
        public class KRModel
        {
            public string id { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public KRProperties properties { get; set; }
        }


        public class Element
        {
            public string id { get; set; }
            public string label { get; set; }
            public string type { get; set; }
            public string inVLabel { get; set; }
            public string outVLabel { get; set; }
            public string inV { get; set; }
            public string outV { get; set; }
        }

        public class PropertyDetail
        {
            public string id { get; set; }
            public string value { get; set; }
        }

        public class GraphMyGoalResponse
        {
            public int OkrCount { get; set; }
            public int KeyCount { get; set; }
            public bool IsLocked { get; set; }
            public bool IsScoreLocked { get; set; }
            public List<GraphOkrResponse> MyGoalOkrResponses { get; set; }
        }
        public class GraphOkrResponse
        {
            public string GoalObjectiveId { get; set; }
            public string EmployeeId { get; set; } = "0";
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ImagePath { get; set; }
            public int Year { get; set; }
            public bool IsPrivate { get; set; }
            public string ObjectiveName { get; set; }
            public string ObjectiveDescription { get; set; }
            public long ObjectiveCycleId { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime DueDate { get; set; }
            public string DueCycle { get; set; }
            public decimal Score { get; set; }
            public DateTime GoalProgressTime { get; set; }
            public long Source { get; set; }
            public int ImportedType { get; set; }
            public string ImportedId { get; set; }
            public bool IsNewItem { get; set; }
            public bool IsAnyFeedback { get; set; }
            public int AlignLevel { get; set; }
            public List<GraphContributorsResponse> Contributors { get; set; }
            public List<GraphGoalKeyResponse> MyGoalsDetails { get; set; }

        }
        public class GraphGoalKeyResponse
        {
            public string GoalKeyId { get; set; }
            public string EmployeeId { get; set; }
            public string KeyDescription { get; set; }
            public DateTime DueDate { get; set; }
            public decimal Score { get; set; }
            public DateTime KeyProgressTime { get; set; }
            public bool IsNewItem { get; set; }
            public int ImportedType { get; set; }
            public long ImportedId { get; set; }
            public long Source { get; set; }
            public bool IsAnyFeedback { get; set; }
            public List<GraphContributorsResponse> Contributors { get; set; }
        }
        public class GraphContributorsResponse
        {
            public string EmployeeId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ImagePath { get; set; }
            public string Designation { get; set; }
        }

    }
}
