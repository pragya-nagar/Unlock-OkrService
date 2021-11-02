using Microsoft.AspNetCore.Mvc;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Response;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OKRService.WebCore.Controllers
{
    public class ReportController : ApiControllerBase
    {
        private readonly IReportService reportService;
        private readonly ICommonService commonService;

        public ReportController(IReportService reportServices, ICommonService commonServices)
        {
            reportService = reportServices;
            commonService = commonServices;
        }

        [Route("MostLeastOkrRisk")]
        [HttpGet]
        public async Task<IActionResult> MostLeastOkrRisk(long empId, int cycle, long orgId, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<ReportMostLeastObjective>>();
            var loginUserMostLeastOkrRisk = await commonService.GetUserIdentity();
            if (loginUserMostLeastOkrRisk == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = reportService.ReportMostLeastObjective(empId, cycle, year, orgId, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("MostLeastOkrKeyResultRisk")]
        [HttpGet]
        public async Task<IActionResult> MostLeastOkrKeyResultRisk(long empId, int cycle, long orgId, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<ReportMostLeastObjectiveKeyResult>>();
            var loginUserMostLeastOkrKeyResultRisk = await commonService.GetUserIdentity();
            if (loginUserMostLeastOkrKeyResultRisk == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = reportService.ReportMostLeastObjectiveKeyResult(empId, cycle, year, orgId, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("AvgOkrReport")]
        [HttpGet]
        public async Task<IActionResult> GetAvgOkrScoreReport(long empId, int cycleId, int year)
        {
            var payloadOutputSave = new PayloadCustom<AvgOkrScoreResponse>();
            var loginUserGetAvgOkrScoreReport = await commonService.GetUserIdentity();
            if (loginUserGetAvgOkrScoreReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            payloadOutputSave.Entity = await reportService.GetAvgOkrScoreReport(empId, cycleId, year, UserToken, loginUserGetAvgOkrScoreReport);
            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
            payloadOutputSave.IsSuccess = true;
            return Ok(payloadOutputSave);
        }

        [Route("WeeklyKrReports")]
        [HttpGet]
        public async Task<IActionResult> GetWeeklyKrUpdates(long empId, int cycleId, int year)
        {
            var payloadOutputSave = new PayloadCustom<UserGoalKeyResponse>();
            var loginUserGetWeeklyKrUpdates = await commonService.GetUserIdentity();
            if (loginUserGetWeeklyKrUpdates == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            payloadOutputSave.EntityList = await reportService.GetWeeklyKrUpdatesReport(empId, cycleId, year, UserToken, loginUserGetWeeklyKrUpdates);
            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
            payloadOutputSave.IsSuccess = true;
            return Ok(payloadOutputSave);
        }

        [Route("KrUpdateGrowthCycle")]
        [HttpGet]
        public async Task<IActionResult> GetWeeklyReport(long empId, int cycleId, int year)
        {
            var payloadOutputSave = new PayloadCustom<WeeklyReportResponse>();
            var loginUserGetWeeklyReport = await commonService.GetUserIdentity();
            if (loginUserGetWeeklyReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            payloadOutputSave.Entity = await reportService.WeeklyReportResponse(empId, cycleId, year, UserToken, loginUserGetWeeklyReport);
            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
            payloadOutputSave.IsSuccess = true;
            return Ok(payloadOutputSave);
        }

        [Route("ProgressReport")]
        [HttpGet]
        public async Task<IActionResult> ProgressReport(int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<ProgressReportResponse>>();
            var loginUserProgressReport = await commonService.GetUserIdentity();
            if (loginUserProgressReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = reportService.ProgressReport(cycle, year, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("QuarterReport")]
        [HttpGet]
        public async Task<IActionResult> QuarterReport(int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<QuarterReportResponse>>();
            var loginUserQuarterReport = await commonService.GetUserIdentity();
            if (loginUserQuarterReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = reportService.QuarterReport(cycle, year, UserToken, loginUserQuarterReport);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("StatusReport")]
        [HttpGet]
        public async Task<IActionResult> StatusReport(int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<StatusReportResponse>>();
            var loginUserQuarterReport = await commonService.GetUserIdentity();
            if (loginUserQuarterReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = reportService.StatusReport(cycle, year, UserToken, loginUserQuarterReport);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("TeamPerformance")]
        [HttpGet]
        public async Task<IActionResult> TeamPerformance(long empId, int cycleId, int year)
        {
            var payloadOutputSave = new PayloadCustom<TeamPerformanceResponse>();
            var loginUserGetAvgOkrScoreReport = await commonService.GetUserIdentity();
            if (loginUserGetAvgOkrScoreReport == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            payloadOutputSave.Entity = await reportService.TeamPerformance(empId, cycleId, year, UserToken, loginUserGetAvgOkrScoreReport);
            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
            payloadOutputSave.IsSuccess = true;
            return Ok(payloadOutputSave);
        }


        #region Private Methods

        private void SetModelStateError(ModelStateError modelState)
        {
            if (modelState.EmpId != null && modelState.EmpId == 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (modelState.Cycle == 0)
                ModelState.AddModelError(Constants.Cycle, Constants.CycleMsg);

            if (modelState.Year == 0)
                ModelState.AddModelError(Constants.Year, Constants.YearMsg);
        }

        #endregion
    }
}
