using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OkrService.DataContract
{
    public class Identity
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long RoleId { get; set; }
        public long EmployeeId { get; set; }
        public string EmailId { get; set; }
        public int Status { get; set; }
        public long ReportingTo { get; set; }
        public string ImageDetail { get; set; }
    }
}
