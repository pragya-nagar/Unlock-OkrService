using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRService.Service.Contracts
{
    public interface IAlignmentService
    {
        //Task<AlignmentResponse> AlignmentResponse(long empId, int cycle, int year, long orgId, string token);
        //List<AlignmentMapResponse> GetAllParentAlignment(AlignParentRequest alignParentRequest);
        //Task<AlignmentResponse> AlignmentMapResponse(long empId, int cycle, int year, long orgId, string token);
        //Task<List<GoalMapAlignment>> GetAlignmentMapResponse(int cycleId, long year, long employeeId);
        //GraphMyGoalResponse GetDataToCosmosDB(string emailId);
        //Task<AllOkrViewResponse> AssociateContributorsResponseAsync(long objectiveId, int objectiveType, int cycleId, int year, string token, UserIdentity userIdentity);
        //Task<AllOkrViewResponse> AllOkrViewResponse(long employeeId, int cycleId, int year, string token, UserIdentity userIdentity);
        //Task<AllOkrViewResponse> AssociateSourceResponseAsync(long objectiveId, int objectiveType, int cycleId, int year, string token, UserIdentity userIdentity);
        //Task<AllOkrViewResponse> OkrViewAllLevelNestedResponseAsync(long employeeId, int cycleId, int year, string token, UserIdentity userIdentity);
        Task<List<OkrViewResponse>> OkrViewAllLevelResponseAsync(long employeeId, List<string> searchTexts, int cycleId, int year,bool isTeams, long teamId ,string token, UserIdentity userIdentity);
        //Task<List<AllTeamOkrViewResponse>> AllTeamOkrViews(long empId, int cycle, int year, string token, UserIdentity identity);
        Task<List<AllTeamOkrViewResponse>> AllTeamOkr(long empId, List<string> searchTexts,int cycle, int year, string token, UserIdentity identity);
    }
}
