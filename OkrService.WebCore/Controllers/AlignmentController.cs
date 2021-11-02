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
    public class AlignmentController : ApiControllerBase
    {
        private readonly IAlignmentService alignmentService;
        private readonly ICommonService commonService;

        public AlignmentController(IAlignmentService alignmentServices, ICommonService commonServices)
        {
            alignmentService = alignmentServices;
            commonService = commonServices;
        }

        [Route("OkrView")]
        [HttpGet]
        public async Task<IActionResult> OkrView(long empId, [FromQuery] List<string> searchTexts, int cycle, int year, bool isTeams, long teamId)
        {
            var payloadOutputSave = new PayloadCustom<OkrViewResponse>();
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });
            if (isTeams && teamId == 0)
            {
                payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                return Ok(payloadOutputSave);
            }

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await alignmentService.OkrViewAllLevelResponseAsync(empId, searchTexts, cycle, year, isTeams, teamId, UserToken, userIdentity);
                if (payloadOutputSave.EntityList != null)
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

        [Route("TeamsOkr")]
        [HttpGet]
        public async Task<IActionResult> TeamsOkr(long empId, [FromQuery] List<string> searchTexts, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<AllTeamOkrViewResponse>();
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await alignmentService.AllTeamOkr(empId, searchTexts, cycle, year, UserToken, userIdentity);
                if (payloadOutputSave.EntityList != null)
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




        #region Private Methods

        private void SetModelStateError(ModelStateError modelState)
        {
            if (modelState.EmpId != null && modelState.EmpId == 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (modelState.Cycle != null && modelState.Cycle == 0)
                ModelState.AddModelError(Constants.Cycle, Constants.CycleMsg);

            if (modelState.Year != null && modelState.Year == 0)
                ModelState.AddModelError(Constants.Year, Constants.YearMsg);

            if (modelState.ObjId != null && modelState.ObjId == 0)
                ModelState.AddModelError(Constants.ObjId, Constants.ObjIdMsg);

            if (modelState.OrgId != null && modelState.OrgId == 0)
                ModelState.AddModelError(Constants.OrgId, Constants.OrgIdMsg);
        }

        #endregion
    }
}
