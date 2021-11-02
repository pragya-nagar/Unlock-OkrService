using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OKRService.WebCore.Controllers
{
    public class PeopleController : ApiControllerBase
    {
        private readonly IPeopleService peopleService;
        private readonly ICommonService commonService;

        public PeopleController(IPeopleService peopleServices, ICommonService commonServices)
        {
            peopleService = peopleServices;
            commonService = commonServices;
        }

        [Route("EmployeeView")]
        [HttpGet]
        public async Task<IActionResult> EmployeeView(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<PeopleResponse>();
            var loginUserEmployeeView = await commonService.GetUserIdentity();
            if (loginUserEmployeeView == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await peopleService.EmployeeView(empId, cycle, year, UserToken, loginUserEmployeeView);
                if (payloadOutputSave.Entity.OkrCount > 0)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("empId", "There is no Okr for the particular EmployeeId.");
                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("PeopleView")]
        [HttpGet]
        public async Task<IActionResult> PeopleView(long empId, int cycle, int year, [FromQuery] List<string> searchTexts)
        {
            var payloadOutputSave = new PayloadCustom<List<PeopleViewResponse>>();
            var loginUserPeopleAlignmentView = await commonService.GetUserIdentity();
            if (loginUserPeopleAlignmentView == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle });

            if (ModelState.IsValid)
            {
                var peopleViewRequest = new PeopleViewRequest()
                {
                    EmployeeId = empId,
                    CycleId = cycle,
                    Year = year,
                    IsNested = false,
                    OrgId = loginUserPeopleAlignmentView.OrganisationId,
                    Token = UserToken,
                    ActionLevel = 0,
                    AllEmployee = new EmployeeResult(),
                    CycleDetail = new CycleDetails(),
                    OrganisationCycleDetails = null,
                    ParentObjList = new List<ParentObjectiveResponse>(),
                    SourceParentObjList = new List<long>(),
                    PeopleViewResponse = new List<PeopleViewResponse>()
                };
                payloadOutputSave.Entity = await peopleService.AllPeopleViewResponse(peopleViewRequest, searchTexts);
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

        #region Private Methods

        private void SetModelStateError(ModelStateError modelStateError)
        {
            if (modelStateError.EmpId == 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (modelStateError.Cycle == 0)
                ModelState.AddModelError(Constants.Cycle, Constants.CycleMsg);

            if (modelStateError.Year != null && modelStateError.Year == 0)
                ModelState.AddModelError(Constants.Year, Constants.YearMsg);
        }

        #endregion
    }
}
