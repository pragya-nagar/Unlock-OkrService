using System;


namespace OKRService.ViewModel.Response
{
    public class CycleLockDetails
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OrganisationCycleId { get; set; }
        public int Year { get; set; }
    }
}
