using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IPeopleService
    {
        Task<PeopleResponse> EmployeeView(long empId, int cycle, int year, string token, UserIdentity identity);
        //Task<PeopleMapResponse> PeopleMapResponse(long empId, int cycle, string token);
        Task<List<GoalObjective>> GetEmployeeOkrByCycleId(long empId, int cycleId, int year);
        Task<List<GoalKey>> GetGoalKey(long goalObjectiveId);
        Task<int> EmployeeOkrCount(long empId, int cycleId);
        Task<List<PeopleViewResponse>> AllPeopleViewResponse(PeopleViewRequest peopleViewRequest, List<string> searchTexts);
        Task<List<PeopleViewResponse>> PeopleViewSource(PeopleViewRequest peopleViewRequest);
        Task<List<PeopleViewResponse>> PeopleViewContributor(PeopleViewRequest peopleViewRequest);
    }
}
