using System.Collections.Generic;
using OKRService.EF;
using OKRService.ViewModel.Request;
using System.Threading.Tasks;
using OKRService.ViewModel.Response;

namespace OKRService.Service.Contracts
{
    public interface IProgressBarCalculationService
    {
        Task<KrCalculationResponse> UpdateKrValue(KrValueUpdate krValueUpdate, UserIdentity userIdentity, string token,GoalKey goalKeyRecord = null, bool isScoreUpdate = false);
        void UpdateGoalKeyAndMaintainHistory(GoalKey goalKey, UserIdentity userIdentity);
        Task<List<KrCalculationAlignmentMapResponse>> UpdateKrValueAlignmentMap(KrValueUpdate krValueUpdate, UserIdentity userIdentity, string token, GoalKey goalKeyRecord = null, bool isScoreUpdate = false);
    }
}
