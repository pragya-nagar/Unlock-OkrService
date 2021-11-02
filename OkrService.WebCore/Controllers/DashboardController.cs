using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OKRService.WebCore.Controllers
{
    public class DashboardController : ApiControllerBase
    {
        private readonly IDashboardService dashboardService;
        private readonly ICommonService commonService;

        public DashboardController(IDashboardService dashboardServices, ICommonService commonServices)
        {
            dashboardService = dashboardServices;
            commonService = commonServices;
        }

        [Route("GetGoalDetail")]
        [HttpGet]
        public async Task<IActionResult> GetGoalDetailById(long goalObjectiveId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<DashboardOkrResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { ObjId = goalObjectiveId, Cycle = cycle, Year = year });

            var goalObjective = await dashboardService.GetDeletedGoalObjective(goalObjectiveId);
            if (goalObjective != null)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.GetGoalDetailById(goalObjectiveId, cycle, year, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.Entity.GoalObjectiveId != 0)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;

                }
                else
                {
                    payloadOutputSave.MessageList.Add("empId", "There is no Okr for the particular GoalId.");
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

        [Route("TeamOkrView")]
        [HttpGet]
        public async Task<IActionResult> GetTeamOkrCardDetails(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<TeamOkrCardResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await dashboardService.GetTeamOkrCardDetails(empId, cycle, year, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.EntityList.Count > 0)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("empId", "There is no Team Okr for the particular EmployeeId.");
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

        [Route("NudgeTeam")]
        [HttpPost]
        public async Task<IActionResult> NudgeTeamAsync(NudgeTeamRequest nudgeTeamRequest)
        {
            var payloadOutputSave = new PayloadCustom<bool>();
            
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { TeamId = nudgeTeamRequest.TeamId, Cycle = nudgeTeamRequest.Cycle, Year = nudgeTeamRequest.Year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.NudgeTeamAsync(nudgeTeamRequest, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageList.Add("teamId", "Nudge successfully sent to all team members");
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("teamId", "There is no active contributor for "+ nudgeTeamRequest.TeamName);
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

        [Route("TeamOkrGoalDetailsById")]
        [HttpGet]
        public async Task<IActionResult> GetTeamOkrGoalDetailsById(long teamId, long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<TeamOkrResponse>();
            
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { TeamId = teamId, EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.GetTeamOkrGoalDetailsById(teamId, empId, cycle, year, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("empId", "There is no Team Okr for the particular EmployeeId.");
                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("TeamOkrSequence")]
        [HttpPut]
        public async Task<IActionResult> UpdateTeamOkrCardSequence(List<UpdateTeamSequenceRequest> updateTeamSequenceRequests)
        {
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<bool>
            {
                Entity = await dashboardService.UpdateTeamOkrCardSequence(updateTeamSequenceRequests, loginUserMyGoalAsync)
            };

            if (payloadOutputSave.Entity)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.MessageList.Add("Result", "There is no update.");
                payloadOutputSave.IsSuccess = false;
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("DirectReports")]
        [HttpGet]
        public async Task<IActionResult> DirectReports(long empId, [FromQuery] List<string> searchTexts, int cycle, int year, string sortBy)
        {
            var payloadOutputSave = new PayloadCustom<DirectReportsResponse>();
            
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await dashboardService.AllDirectReportsResponseAsync(empId, searchTexts, cycle, year, UserToken, userIdentity,sortBy);
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
        
        [Route("NudgeDirectReporting")]
        [HttpPost]
        public async Task<IActionResult> NudgeDirectReportingAsync(long empId)
        {
            
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadOutputSave = new PayloadCustom<bool>
            {
                Entity = await dashboardService.NudgeDirectReportAsync(empId, UserToken, loginUserMyGoalAsync)
            };
            if (payloadOutputSave.Entity)
            {
                payloadOutputSave.MessageList.Add("empId","Nudge successfully sent");
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payloadOutputSave.MessageList.Add("empId", "No nudge sent");
                payloadOutputSave.IsSuccess = false;
                payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
            }


            return Ok(payloadOutputSave);
        }

        [Route("AllOkrDashboard")]
        [HttpGet]
        public async Task<IActionResult> AllOkrDashboardAsync(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<AllOkrDashboardResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.AllOkrDashboardAsync(empId, cycle, year, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
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

        [Route("DeltaScore")]
        [HttpGet]
        public async Task<IActionResult> DeltaScore(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<DeltaResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if(empId > 0 && loginUserMyGoalAsync.EmployeeId != empId)
                ModelState.AddModelError(Constants.EmpId, Constants.InvalidToken);

            var allEmployee = commonService.GetAllUserFromUsers(UserToken);
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(loginUserMyGoalAsync.OrganisationId, UserToken);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            if (quarterDetails == null)
                ModelState.AddModelError(Constants.Year, Constants.InvalidYear);
            
            var userDetail = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
            if(userDetail == null)
                 ModelState.AddModelError(Constants.EmpId, Constants.InvalidEmpId);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.DeltaScore(empId, cycle, year, loginUserMyGoalAsync, UserToken, allEmployee, quarterDetails, cycleDurationDetails);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("result", "No record found");
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

        [Route("RecentContribution")]
        [HttpGet]
        public async Task<IActionResult> RecentContribution(long empId, int cycle, int year)
        {
           
            var payloadOutputSave = new PayloadCustom<RecentContributionResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (empId > 0 && loginUserMyGoalAsync.EmployeeId != empId)
                ModelState.AddModelError(Constants.EmpId, Constants.InvalidToken);

            var allEmployee = commonService.GetAllUserFromUsers(UserToken);
            var cycleDurationDetails = commonService.GetOrganisationCycleDurationId(loginUserMyGoalAsync.OrganisationId, UserToken);
            var cycleDetail = cycleDurationDetails.CycleDetails.FirstOrDefault(x => Convert.ToInt32(x.Year) == year);
            var quarterDetails = cycleDetail?.QuarterDetails.FirstOrDefault(x => x.OrganisationCycleId == cycle);

            if (quarterDetails == null)
                ModelState.AddModelError(Constants.Year, Constants.InvalidYear);

            var userDetail = allEmployee.Results.FirstOrDefault(x => x.EmployeeId == empId);
            if (userDetail == null)
                ModelState.AddModelError(Constants.EmpId, Constants.InvalidEmpId);

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await dashboardService.RecentContribution(empId, cycle, year, loginUserMyGoalAsync, allEmployee);
                if (payloadOutputSave.EntityList != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("result", "No record found");
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

        [Route("TeamGoals")]
        [HttpGet]
        public async Task<IActionResult> TeamGoalsAsync(long teamId, long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<List<EmailTeamLeaderResponse>>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { TeamId = teamId, EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.GetTeamGoals(teamId, cycle, year);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("teamId", "There is no Okr for the particular TeamId.");
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

        [Route("VirtualAlignment")]
        [HttpGet]
        public async Task<IActionResult> VirtualAlignment(long goalObjectiveId)
        {
            var payloadOutputSave = new PayloadCustom<VirtualAlignmentResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            payloadOutputSave.Entity = await dashboardService.GetVirtualAlignment(goalObjectiveId, loginUserMyGoalAsync, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("teamId", "There is no virtual linking for this particular objective.");
                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
         

            return Ok(payloadOutputSave);
        }

        [Route("IsAnyOkr")]
        [HttpGet]
        public async Task<IActionResult> IsAnyOkr()
        {
            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<bool>
            {
                Entity = await dashboardService.IsAlreadyCreatedOkr(loginUser.EmployeeId)
            };

            if (payloadOutputSave.Entity)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.MessageList.Add("Result", "There is no OKR.");
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("TeamDetailsById")]
        [HttpGet]
        public async Task<IActionResult> TeamDetailsById([Required] long teamId,[Required] long sourceId, long goalKeyId)
        {
            var payloadOutputSave = new PayloadCustom<TeamDetails>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            payloadOutputSave.Entity = await dashboardService.TeamDetailsById(teamId, sourceId,goalKeyId, UserToken);
            if (payloadOutputSave.Entity != null)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payloadOutputSave.MessageList.Add("teamId", "There is no team member available.");
                payloadOutputSave.IsSuccess = false;
                payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
            }


            return Ok(payloadOutputSave);
        }

        [Route("Archive")]
        [HttpGet]
        public async Task<IActionResult> ArchiveDashboardAsync(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<AllOkrDashboardResponse>();
            var loginUserMyGoalAsync = await commonService.GetUserIdentity();
            if (loginUserMyGoalAsync == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (empId > 0 && loginUserMyGoalAsync.EmployeeId != empId)
                ModelState.AddModelError(Constants.EmpId, Constants.InvalidToken);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await dashboardService.ArchiveDashboardAsync(empId, cycle, year, UserToken, loginUserMyGoalAsync);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("empId", "There is no archive okr for the particular EmployeeId.");
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

        [Route("EmployeeScoreDetails")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeScoreDetails(long empId, int cycle, int year)
        {
            var userIdentity = commonService.GetUserIdentity().Result;
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadOutputSave = new PayloadCustom<EmployeeScoreResponse>
            {
                Entity= await dashboardService.GetEmployeeScoreDetails(empId, cycle, year,UserToken,userIdentity),
                MessageType = MessageType.Success.ToString(),
                IsSuccess = true
            };
            payloadOutputSave.MessageList.Add(Constants.Result, Constants.Count);
            payloadOutputSave.Status = Response.StatusCode;

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
                ModelState.AddModelError(Constants.Goal, Constants.GoalMsg);

            if (modelState.TeamId != null && modelState.TeamId == 0)
                ModelState.AddModelError(Constants.TeamId, Constants.TeamIdMsg);
        }

        #endregion
    }
}
