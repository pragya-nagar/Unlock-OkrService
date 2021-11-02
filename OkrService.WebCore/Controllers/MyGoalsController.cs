using Microsoft.AspNetCore.Mvc;
using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OKRService.WebCore.Controllers
{
    public class MyGoalsController : ApiControllerBase
    {
        private readonly IMyGoalsService myGoalsService;
        private readonly ICommonService commonService;
        private readonly IProgressBarCalculationService progressBarCalculationService;

        public MyGoalsController(IMyGoalsService myGoalsServices, ICommonService commonServices, IProgressBarCalculationService progressBarCalculationServices)
        {
            myGoalsService = myGoalsServices;
            commonService = commonServices;
            progressBarCalculationService = progressBarCalculationServices;
        }

        [Route("Goals")]
        [HttpPost]
        public async Task<IActionResult> InsertGoalObjective(List<MyGoalsRequest> myGoalsRequests)
        {
            var goalStatusType = (int)GoalStatus.Public;
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadOutputSave = new PayloadCustom<MyGoalsRequest>();
            if (myGoalsRequests.Count == 1)
            {
                goalStatusType = myGoalsRequests[0].GoalStatusId;
            }

            foreach (var item in myGoalsRequests)
            {
                if (item.GoalStatusId == GoalStatus.Draft.GetHashCode())
                {
                    SetModelStateError(new ModelStateError
                    {
                        ObjName = item.ObjectiveName,
                        EmpId = item.EmployeeId,
                        Cycle = item.ObjectiveCycleId,
                        Year = item.Year
                    });
                }
                else
                {
                    SetModelStateError(new ModelStateError
                    {
                        ObjName = item.ObjectiveName,
                        EmpId = item.EmployeeId,
                        Cycle = item.ObjectiveCycleId,
                        Year = item.Year,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,

                    });
                    ValidateKrModelState(item);
                }
            }

            if (ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await myGoalsService.InsertGoalObjective(myGoalsRequests, userIdentity, UserToken);
                if (payloadOutputSave.EntityList.Count > 0)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    if (goalStatusType == (int)GoalStatus.Draft)
                    {
                        payloadOutputSave.MessageList.Add(Constants.Result, Constants.DraftGoalCreationSuccessful);
                    }
                    else
                    {
                        payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalCreationSuccessful);
                    }
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }


            return Ok(payloadOutputSave);
        }

        [Route("Goals")]
        [HttpDelete]
        public async Task<IActionResult> DeleteOkr(long employeeId, long goalObjectiveId, int goalType)
        {
            var payloadOutputSave = new PayloadCustom<bool>();
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (employeeId <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (goalType <= 0 || goalType > 2)
            {
                ModelState.AddModelError(Constants.Goal, "Invalid Goal Type");
            }

            if (goalObjectiveId <= 0)
            {
                ModelState.AddModelError(Constants.GoalObjective, Constants.GoalObjectiveId);
            }
            else
            {
                var goalMappingDetails = myGoalsService.GetGoalObjective(goalObjectiveId);
                if (goalMappingDetails == null)
                {
                    ModelState.AddModelError(Constants.Result, "There is no Objective.");
                }
                else
                {
                    if (goalMappingDetails.EmployeeId != employeeId)
                    {
                        ModelState.AddModelError(Constants.Result, Constants.DeleteOkrPermission);
                    }
                }
            }
            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.DeleteOkrKr(employeeId, goalObjectiveId, goalType, userIdentity, UserToken);

                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalDeleted);
                    payloadOutputSave.Status = Response.StatusCode;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("Result", "There is no update.");
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("TeamOKR")]
        [HttpDelete]
        public async Task<IActionResult> DeleteOkrTeam(long employeeId, long goalObjectiveId, int goalType, long teamId)
        {
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<bool>
            {
                Entity = await myGoalsService.DeleteOkrKrTeam(employeeId, goalObjectiveId, goalType, teamId, userIdentity, UserToken)
            };

            if (payloadOutputSave.Entity)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalDeleted);
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("GoalCreation")]
        [HttpGet]
        public async Task<IActionResult> GetTypeOfGoalCreation()
        {
            var loginUserGetTypeOfGoalCreation = await commonService.GetUserIdentity();
            if (loginUserGetTypeOfGoalCreation == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<TypeOfGoalCreation>
            {
                EntityList = myGoalsService.GetTypeOfGoalCreations()
            };

            if (payloadOutputSave.EntityList != null)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("Goals")]
        [HttpPut]
        public async Task<IActionResult> UpdateObjective(MyGoalsRequest myGoalsRequests)
        {
            var payloadOutputSave = new PayloadCustom<MyGoalsRequest>();

            var loginUserUpdateObjective = await commonService.GetUserIdentity();
            if (loginUserUpdateObjective == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            ValidateKrModelState(myGoalsRequests);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateObjective(myGoalsRequests, loginUserUpdateObjective, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    if (myGoalsRequests.IsPrivate)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.PrivateMsg);
                    }
                    else
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.ObjectiveSaved);
                    }

                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("MyGoal")]
        [HttpGet]
        public async Task<IActionResult> MyGoal(long empId, int cycle, int year)
        {
            var payloadOutputSave = new PayloadCustom<MyGoalResponse>();

            var loginUserMyGoal = await commonService.GetUserIdentity();
            if (loginUserMyGoal == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.MyGoal(empId, cycle, year, UserToken, loginUserMyGoal);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("Contributors")]
        [HttpDelete]
        public async Task<IActionResult> DeleteContributors(long employeeId, long goalKeyId)
        {

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<string>
            {
                Entity = await myGoalsService.DeleteContributors(employeeId, goalKeyId, userIdentity, UserToken)
            };

            if (payloadOutputSave.Entity == "")
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add(Constants.Result, Constants.ContributorDeleted);
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("GetContributors")]
        [HttpGet]
        public async Task<IActionResult> GetContributorsByGoalTypeAndId(int goalType, long goalId)
        {

            var loginUserGetContributorsByGoalTypeAndId = await commonService.GetUserIdentity();
            if (loginUserGetContributorsByGoalTypeAndId == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustomGenric<ContributorsResponse>
            {
                EntityList = myGoalsService.GetContributorsByGoalTypeAndId(goalType, goalId, UserToken)
            };

            if (payloadOutputGet.EntityList != null)
            {
                payloadOutputGet.MessageType = MessageType.Success.ToString();
                payloadOutputGet.IsSuccess = true;
                payloadOutputGet.Status = Response.StatusCode;
            }

            return Ok(payloadOutputGet);
        }

        [Route("keyScore")]
        [HttpPut]
        public async Task<IActionResult> UpdateKeyScore(KeyScoreUpdate keyScoreUpdate)
        {

            var loginUserUpdateKeyScore = await commonService.GetUserIdentity();
            if (loginUserUpdateKeyScore == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<string>
            {
                Entity = await myGoalsService.UpdateKeyScore(keyScoreUpdate, loginUserUpdateKeyScore, UserToken)
            };

            if (payloadOutputSave.Entity == "")
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add("Result", "key Score is updated Successfully");
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("Align")]
        [HttpGet]
        public async Task<IActionResult> AlignOkr(long empId, int cycle, int year)
        {

            var loginUserAlignOkr = await commonService.GetUserIdentity();
            if (loginUserAlignOkr == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<AlignResponse>
            {
                Entity = await myGoalsService.AlignObjective(empId, cycle, year, UserToken, loginUserAlignOkr)
            };

            if (payloadOutputSave.Entity != null)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add("Result", "Okr is Align");
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("AskUnLock")]
        [HttpPost]
        public async Task<IActionResult> UnLockObjectives(UnLockRequest unLockRequest)
        {

            var payloadOutputSave = new PayloadCustom<string>();
            var loginUserUnLockObjectives = await commonService.GetUserIdentity();
            if (loginUserUnLockObjectives == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (unLockRequest != null)
            {
                bool lockStatus = await myGoalsService.IsAlreadyRequestedAsync(unLockRequest);
                if (lockStatus)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add("unlockStatus", "Seems like you have already made a request.");
                }
                else
                {
                    var saveResult = await myGoalsService.LockGoals(unLockRequest, UserToken);
                    if (saveResult == "")
                    {
                        payloadOutputSave.MessageType = MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.MessageList.Add("unlockStatus", "Unlock request raised successfully.");
                    }
                }
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add("unlockStatus", "Request is not valid");
                payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
            }

            return Ok(payloadOutputSave);
        }

        [Route("GetKeyDetail")]
        [HttpGet]
        public async Task<IActionResult> GetKeyDetail(int type, long typeId)
        {

            var loginUserGetKeyDetail = await commonService.GetUserIdentity();
            if (loginUserGetKeyDetail == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<MyGoalDetailResponse>
            {
                Entity = myGoalsService.MyGoalDetailResponse(type, typeId, UserToken)
            };

            if (payloadOutputSave.Entity != null)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.MessageList.Add("GoalType", "There is no Objective.");
                payloadOutputSave.IsSuccess = false;
                payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
            }

            return Ok(payloadOutputSave);
        }

        [Route("UnLockApprove")]
        [HttpPost]
        public async Task<IActionResult> UnLockApprove(UnLockRequest unLockRequest)
        {


            var loginUserUnLockApprove = await commonService.GetUserIdentity();
            if (loginUserUnLockApprove == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<UnLockLog>();

            SetModelStateError(new ModelStateError { EmpId = unLockRequest.EmployeeId, Cycle = unLockRequest.Cycle, Year = unLockRequest.Year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UnLockGoal(unLockRequest, loginUserUnLockApprove);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add("Result", "Objective reset Successfully");
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("BulkUnlockApprove")]
        [HttpPost]
        public async Task<IActionResult> BulkUnlockApprove(List<UnLockRequest> unLockRequest)
        {


            var loginUserBulkUnlockApprove = await commonService.GetUserIdentity();
            if (loginUserBulkUnlockApprove == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<long>
            {
                Entity = await myGoalsService.BulkUnlockApprove(unLockRequest, loginUserBulkUnlockApprove, UserToken)
            };

            if (payloadOutputSave.Entity.ToString() == "0")
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add("Result", "Users are already unlocked.");
                payloadOutputSave.Status = Response.StatusCode;
            }
            else if (payloadOutputSave.Entity.ToString() != "0")
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add("Result", payloadOutputSave.Entity.ToString() + " user objective has been unlocked");
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("UnlockLog")]
        [HttpGet]
        public async Task<IActionResult> UnlockLog()
        {

            var loginUserUnlockLog = await commonService.GetUserIdentity();
            if (loginUserUnlockLog == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<UnLockLog>
            {
                EntityList = myGoalsService.UnlockLog()
            };

            if (payloadOutputSave.EntityList != null)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.MessageList.Add("GoalType", "There is no Objective.");
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("AlignStatus")]
        [HttpGet]
        public async Task<IActionResult> AlignStatus(long employeeId, int sourceType, long sourceId)
        {

            var loginUserAlignStatus = await commonService.GetUserIdentity();
            if (loginUserAlignStatus == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputStatus = new PayloadCustom<AlignStatusResponse>
            {
                Entity = myGoalsService.AlignStatus(employeeId, sourceType, sourceId)
            };

            if (payloadOutputStatus.Entity != null)
            {
                payloadOutputStatus.MessageType = MessageType.Success.ToString();
                payloadOutputStatus.IsSuccess = true;
                payloadOutputStatus.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputStatus.MessageList.Add("GoalType", "There is no Objective.");
                payloadOutputStatus.IsSuccess = true;
                payloadOutputStatus.Status = Response.StatusCode;
            }

            return Ok(payloadOutputStatus);
        }

        [Route("DownloadPdf")]
        [HttpPost]
        public async Task<IActionResult> DownloadPdf(long empId, int cycle, int year)
        {


            var loginUser = await commonService.GetUserIdentity();
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<MyGoalsPdfResponse>();

            SetModelStateError(new ModelStateError { EmpId = empId, Cycle = cycle, Year = year });

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.DownloadPDf(empId, cycle, year, UserToken, loginUser);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add("Result", "Pdf Download successful");
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("UpdateSequence")]
        [HttpPut]
        public async Task<IActionResult> UpdateGoalSequence(List<UpdateSequenceRequest> updateSequenceRequests)
        {

            var loginUserUpdate = await commonService.GetUserIdentity();
            if (loginUserUpdate == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputStatus = new PayloadCustom<bool>
            {
                Entity = await myGoalsService.UpdateOkrSequence(updateSequenceRequests, loginUserUpdate)
            };

            if (payloadOutputStatus.Entity)
            {
                payloadOutputStatus.MessageType = MessageType.Success.ToString();
                payloadOutputStatus.IsSuccess = true;
                payloadOutputStatus.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputStatus.MessageList.Add("Result", "There is no update.");
                payloadOutputStatus.IsSuccess = true;
                payloadOutputStatus.Status = Response.StatusCode;
            }

            return Ok(payloadOutputStatus);
        }


        [Route("KrContributors")]
        [HttpGet]
        public async Task<IActionResult> GetKrStatusContributors(int goalType, long goalId)
        {

            var loginUserGetContributors = await commonService.GetUserIdentity();
            if (loginUserGetContributors == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (goalId <= 0)
                ModelState.AddModelError(Constants.GoalKey, Constants.GoalKeyMessage);

            var payloadOutputGet = new PayloadCustomGenric<KrStatusContributorResponse>();
            if (ModelState.IsValid)
            {
                payloadOutputGet.EntityList = myGoalsService.GetKrStatusContributors(goalType, goalId, UserToken);

                if (payloadOutputGet.EntityList != null)
                {
                    payloadOutputGet.MessageType = MessageType.Success.ToString();
                    payloadOutputGet.IsSuccess = true;
                    payloadOutputGet.Status = Response.StatusCode;
                }
            }

            return Ok(payloadOutputGet);
        }


        [Route("GoalKey")]
        [HttpPut]
        public async Task<IActionResult> UpdateGoalKeyAttributes(MyGoalsDetails goalsDetails)
        {
            var loginUserUpdateGoalKey = await commonService.GetUserIdentity();
            if (loginUserUpdateGoalKey == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<long>();
            if (goalsDetails.GoalKeyId <= 0)
                ModelState.AddModelError(Constants.GoalKey, Constants.GoalKeyMessage);

            if (goalsDetails.GoalKeyId > 0)
            {
                var updateDueDate = new UpdateDueDateRequest()
                {
                    StartDate = goalsDetails.StartDate,
                    GoalType = Constants.GoalKeyType,
                    EndDate = goalsDetails.DueDate,
                    GoalId = goalsDetails.GoalKeyId
                };
                var validateDueDate = ValidateDueDate(updateDueDate, loginUserUpdateGoalKey.EmployeeId,false).ToList();
                var distinct = validateDueDate.Distinct().ToList();
                if (distinct.Count == 1)
                {
                    var item = distinct.FirstOrDefault(x => x.Contains(Constants.StartDate));
                    if (item != null)
                    {
                        ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                    }
                    else
                    {
                        ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
                    }
                }
                else if (distinct.Count == 2)
                {
                    ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                    ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
                }

            }
            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateGoalAttributes(goalsDetails, loginUserUpdateGoalKey, UserToken);
                if (payloadOutputSave.Entity != 0)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalKeyUpdate);
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }


        [Route("GoalKeyDetail/{goalKeyId}")]
        [HttpGet]
        public async Task<IActionResult> GetKeyDetails(long goalKeyId)
        {

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (goalKeyId <= 0)
                ModelState.AddModelError(Constants.GoalKey, Constants.GoalKeyMessage);

            var status = myGoalsService.GetGoalKeyDetails(goalKeyId);

            if (!status.IsActive)
            {
                return StatusCode((int)HttpStatusCode.Forbidden);
            }


            var payloadOutputSave = new PayloadCustom<KeyDetailsResponse>();

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.GetKeyDetails(goalKeyId, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add("Result", Constants.GoalKeyFetched);
                    payloadOutputSave.Status = Response.StatusCode;
                }

            }
            return Ok(payloadOutputSave);
        }

        [Route("GoalObjectiveDetail/{goalObjectiveId}")]
        [HttpGet]
        public async Task<IActionResult> GetGoalByGoalObjectiveId(long goalObjectiveId)
        {

            var userIdentity = await commonService.GetUserIdentity();

            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutput = new PayloadCustom<GoalObjectiveResponse>
            {
                Entity = await myGoalsService.GetGoalByGoalObjectiveId(goalObjectiveId)
            };

            if (payloadOutput.Entity != null)
            {
                payloadOutput.MessageType = MessageType.Success.ToString();
                payloadOutput.IsSuccess = true;
                payloadOutput.Status = Response.StatusCode;
            }
            return Ok(payloadOutput);
        }


        [Route("KrContributor")]
        [HttpPut]
        public async Task<IActionResult> UpdateKrContributor(ContributorKeyResultRequest contributorKeyResultRequest)
        {
            var payloadOutputSave = new PayloadCustom<long>();

            var loginUserUpdateObjective = await commonService.GetUserIdentity();
            if (loginUserUpdateObjective == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (contributorKeyResultRequest.GoalKeyId <= 0)
                ModelState.AddModelError(Constants.GoalKey, Constants.GoalKeyId);

            var status = myGoalsService.GetGoalKeyDetails(contributorKeyResultRequest.GoalKeyId);

            if (status != null && (status.KrStatusId == 2 || status.KrStatusId == 3))
            {
                ModelState.AddModelError(Constants.KrAssignee, Constants.message);
            }

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateContributorsKeyResult(contributorKeyResultRequest, loginUserUpdateObjective, UserToken);
                if (payloadOutputSave.Entity != 0)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    if (contributorKeyResultRequest.KrStatusId == (int)KrStatus.Accepted && contributorKeyResultRequest.GoalObjectiveId > 0 && !contributorKeyResultRequest.IsSelf)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.ContributorKeyAcceptWithTeam);
                    }
                    else if (contributorKeyResultRequest.KrStatusId == (int)KrStatus.Accepted && contributorKeyResultRequest.GoalObjectiveId > 0)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.ContributorKeyAccept);
                    }
                    else if (contributorKeyResultRequest.KrStatusId == (int)KrStatus.Accepted)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.ContributorStandaloneKeyAccept);
                    }
                    else if (contributorKeyResultRequest.KrStatusId == (int)KrStatus.Declined)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.ContributorKeyDecline);
                    }
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("GoalObjective")]
        [HttpPut]
        public async Task<IActionResult> UpdateGoalObjective(UpdateGoalRequest updateGoalRequest)
        {
            var payloadOutputSave = new PayloadCustom<long>();

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (updateGoalRequest.GoalObjectiveId <= 0)
                ModelState.AddModelError(Constants.GoalObjective, Constants.GoalObjectiveId);

            if (updateGoalRequest.GoalObjectiveId > 0)
            {
                var updateDueDate = new UpdateDueDateRequest()
                {
                    StartDate = updateGoalRequest.StartDate,
                    GoalType = Constants.GoalObjectiveType,
                    EndDate = updateGoalRequest.EndDate,
                    GoalId = updateGoalRequest.GoalObjectiveId
                };
                var validateDueDate = ValidateDueDate(updateDueDate, userIdentity.EmployeeId,true).ToList();
                var distinct = validateDueDate.Distinct().ToList();
                if (distinct.Count == 1)
                {
                    if (distinct.Contains(Constants.StartDate))
                    {
                        ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                    }
                    else if (distinct.Contains(Constants.DueDate))
                    {
                        ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
                    }
                    else if (distinct.Contains(Constants.ParentDueDate))
                    {
                        ModelState.AddModelError(Constants.DueDate, Constants.ParentUpdateDueDate);
                    }
                }
                else if (distinct.Count == 2)
                {
                    ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                    ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
                }
            }
            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateGoalObjective(updateGoalRequest, userIdentity, UserToken);
                if (payloadOutputSave.Entity != 0)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;

                    if (!updateGoalRequest.IsPrivate)
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.GoalUpdatedSuccessfully);
                    }
                    else
                    {
                        payloadOutputSave.MessageList.Add("Result", Constants.PrivateMsg);
                    }

                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("Contributor")]
        [HttpPost]
        public async Task<IActionResult> AddContributor([FromBody] ContributorDetailRequest contributorDetails)
        {
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            if (contributorDetails.EmployeeId <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);
            if (contributorDetails.ImportedId <= 0)
                ModelState.AddModelError(Constants.ImportedId, Constants.ImportedIdMessage);
            if (contributorDetails.ImportedType <= 0)
                ModelState.AddModelError(Constants.ImportedType, Constants.ImportedTypeMessage);
            if (myGoalsService.IsKeyImportedGoal(contributorDetails.ImportedId, contributorDetails.EmployeeId))
            {
                ModelState.AddModelError(Constants.IsAlreadyAligned, Constants.IsAlreadyAlignedMessage);
            }
            ////User cannot assign himself as contributor in his own KR
            if (myGoalsService.GetKeyFromKeyId(contributorDetails.ImportedId).EmployeeId == contributorDetails.EmployeeId)
            {
                ModelState.AddModelError(Constants.Contributor, Constants.ContributorMessage);
            }
            var payloadOutputSave = new PayloadCustom<ContributorDetailRequest>();

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.AddContributor(contributorDetails, userIdentity, UserToken);
                if (payloadOutputSave.Entity != null)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalCreationSuccessful);
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("UpdateKrValue")]
        [HttpPut]

        public async Task<IActionResult> UpdateKrValue(KrValueUpdate krValueUpdate)
        {
            var userIdentity = commonService.GetUserIdentity().Result;
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadOutputSave = new PayloadCustom<KrCalculationResponse>
            {
                Entity = await progressBarCalculationService.UpdateKrValue(krValueUpdate, userIdentity, UserToken, null, true),
                MessageType = MessageType.Success.ToString(),
                IsSuccess = true
            };
            payloadOutputSave.MessageList.Add(Constants.Result, Constants.KeyValueUpdate);
            payloadOutputSave.Status = Response.StatusCode;

            return Ok(payloadOutputSave);
        }

        [Route("GoalDescription")]
        [HttpPut]
        public async Task<IActionResult> UpdateGoalDescription(UpdateGoalDescriptionRequest updateGoalDescriptionRequest)
        {
            var payloadOutputSave = new PayloadCustom<bool>();

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (updateGoalDescriptionRequest.GoalId <= 0)
                ModelState.AddModelError(Constants.Goal, Constants.GoalMsg);
            if (updateGoalDescriptionRequest.GoalType <= 0)
                ModelState.AddModelError(Constants.Goal, Constants.GoalTypeMsg);
            if (string.IsNullOrEmpty(updateGoalDescriptionRequest.Description))
                ModelState.AddModelError(Constants.Goal, Constants.ObjectiveNameMsg);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateGoalDescription(updateGoalDescriptionRequest, userIdentity, UserToken);
                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Goal, Constants.GoalSuccessMsg);
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("BecomeContributor")]
        [HttpPost]
        public async Task<IActionResult> BecomeContributor(AddContributorRequest addContributorRequest)
        {
            var payloadOutputSave = new PayloadCustom<long>();

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (addContributorRequest.GoalKeyId <= 0)
                ModelState.AddModelError(Constants.GoalKey, Constants.GoalKeyId);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.BecomeContributor(addContributorRequest, userIdentity, UserToken);
                if (payloadOutputSave.Entity == 1)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Goal, Constants.GoalSuccessMsg);
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }
            return Ok(payloadOutputSave);
        }

        [Route("LinkObjectiveDetail")]
        [HttpGet]
        public async Task<IActionResult> LinkObjectivesAsync(long searchEmployeeId, int searchEmployeeCycleId)
        {
            var payloadOutputSave = new PayloadCustom<LinkedObjectiveResponse>();

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            payloadOutputSave.EntityList = await myGoalsService.LinkObjectivesAsync(searchEmployeeId, searchEmployeeCycleId, UserToken, userIdentity);
            if (payloadOutputSave.EntityList.Count > 0)
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add(Constants.Goal, Constants.NoRecordMsg);
                payloadOutputSave.Status = Response.StatusCode;
            }

            return Ok(payloadOutputSave);
        }

        [Route("ResetOkr")]
        [HttpPut]
        public async Task<IActionResult> ResetOkr(long employeeId, long goalObjectiveId, int goalType)
        {
            var payloadOutputSave = new PayloadCustom<bool>();

            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (employeeId <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (goalObjectiveId <= 0)
            {
                ModelState.AddModelError(Constants.GoalObjective, Constants.GoalObjectiveId);
            }
            else
            {
                var goalMappingDetails = myGoalsService.GetGoalObjective(goalObjectiveId);
                if (goalMappingDetails == null)
                {
                    ModelState.AddModelError(Constants.Result, Constants.NoOkrReset);
                }
                else
                {
                    if (goalMappingDetails.EmployeeId != employeeId)
                    {
                        ModelState.AddModelError(Constants.Result, Constants.ResetOkrPermission);
                    }
                }
            }

            if (goalType <= 0 || goalType > 2)
            {
                ModelState.AddModelError(Constants.Goal, "Invalid Goal Type");
            }


            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.ResetOkr(employeeId, goalObjectiveId, goalType, userIdentity, UserToken);
                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.GoalReset);
                    payloadOutputSave.Status = Response.StatusCode;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("Result", "There is no update.");
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }
            return Ok(payloadOutputSave);
        }

        [Route("ChangeOwner")]
        [HttpPut]
        public async Task<IActionResult> ChangeOwner(long goalObjectiveId, long newOwner)
        {
            var payloadOutputSave = new PayloadCustom<bool>();
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (newOwner <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (goalObjectiveId <= 0)
            {
                ModelState.AddModelError(Constants.GoalObjective, Constants.GoalObjectiveId);
            }

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.ChangeOwner(userIdentity, goalObjectiveId, newOwner, UserToken);
                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.OwnerChange);
                    payloadOutputSave.Status = Response.StatusCode;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("Result", "You can only change owner if your okr in public mode");
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }
            return Ok(payloadOutputSave);
        }

        [Route("UpdateKrValueAlignmentMap")]
        [HttpPut]
        public async Task<IActionResult> UpdateKrValueAlignmentMap(KrValueUpdate krValueUpdate)
        {
            var userIdentity = commonService.GetUserIdentity().Result;
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var payloadOutputSave = new PayloadCustom<KrCalculationAlignmentMapResponse>
            {
                EntityList = await progressBarCalculationService.UpdateKrValueAlignmentMap(krValueUpdate, userIdentity, UserToken, null, true),
                MessageType = MessageType.Success.ToString(),
                IsSuccess = true
            };
            payloadOutputSave.MessageList.Add(Constants.Result, Constants.KeyValueUpdate);
            payloadOutputSave.Status = Response.StatusCode;

            return Ok(payloadOutputSave);
        }

        [Route("UpdateTeamLeaderOkr")]
        [HttpPut]
        public async Task<IActionResult> UpdateTeamLeaderOkr(UpdateTeamLeaderOkrRequest teamLeaderOkrRequest)
        {
            var payloadOutputSave = new PayloadCustom<bool>();
            var userIdentity = await commonService.GetUserIdentity();
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            if (teamLeaderOkrRequest.OldLeader <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.OldLeaderMsg);

            if (teamLeaderOkrRequest.NewLeader <= 0)
                ModelState.AddModelError(Constants.EmpId, Constants.NewLeaderMsg);

            if (teamLeaderOkrRequest.TeamId <= 0)
                ModelState.AddModelError(Constants.TeamId, Constants.TeamIdMsg);

            if (teamLeaderOkrRequest.CycleId <= 0)
                ModelState.AddModelError(Constants.Cycle, Constants.CycleMsg);

            if (ModelState.IsValid)
            {
                payloadOutputSave.Entity = await myGoalsService.UpdateTeamLeaderOkr(teamLeaderOkrRequest, userIdentity, UserToken);
                if (payloadOutputSave.Entity)
                {
                    payloadOutputSave.MessageType = MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.MessageList.Add(Constants.Result, Constants.UpdateLeaderTeamOkr);
                    payloadOutputSave.Status = Response.StatusCode;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("Result", "There is no update.");
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
            }

            return Ok(payloadOutputSave);
        }

        [Route("UpdateDueDate")]
        [HttpPut]

        public async Task<IActionResult> UpdateDueDate(UpdateDueDateRequest updateDueDate)
        {
            var payloadOutputSave = new PayloadCustom<DueDateResponse>();
            var userIdentity = commonService.GetUserIdentity().Result;
            if (userIdentity == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            var validateDueDate = ValidateDueDate(updateDueDate, userIdentity.EmployeeId,true).ToList();
            var distinct = validateDueDate.Distinct().ToList();
            if (distinct.Count == 1)
            {
                if (distinct.Contains(Constants.StartDate))
                {
                    ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                }
                else if(distinct.Contains(Constants.DueDate))
                {
                    ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
                }
                else if(distinct.Contains(Constants.ParentDueDate))
                {
                    ModelState.AddModelError(Constants.DueDate, Constants.ParentUpdateDueDate);
                }
            }
            else if (distinct.Count == 2)
            {
                ModelState.AddModelError(Constants.StartDate, Constants.UpdateStartDate);
                ModelState.AddModelError(Constants.DueDate, Constants.UpdateDueDate);
            }
            if(ModelState.IsValid)
            {
                payloadOutputSave.EntityList = await myGoalsService.UpdateDueDateAlignment(updateDueDate, userIdentity, UserToken, true);
                payloadOutputSave.MessageType = MessageType.Success.ToString();
                payloadOutputSave.IsSuccess = true;
                payloadOutputSave.MessageList.Add(Constants.Result, Constants.DueDateUpdate);
                payloadOutputSave.Status = Response.StatusCode;
            }
            else
            {
                payloadOutputSave = GetPayloadStatus(payloadOutputSave);
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
                Entity = await myGoalsService.IsAnyOkr(loginUser.EmployeeId)
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

        #region Private Methods

        private void SetModelStateError(ModelStateError modelState)
        {
            if (modelState.EmpId != null && modelState.EmpId == 0)
                ModelState.AddModelError(Constants.EmpId, Constants.EmpIdMsg);

            if (modelState.Cycle != null && modelState.Cycle == 0)
                ModelState.AddModelError(Constants.Cycle, Constants.CycleMsg);

            if (modelState.Year != null && modelState.Year == 0)
                ModelState.AddModelError(Constants.Year, Constants.YearMsg);

            if (modelState.ObjName != "-DoNotValidate-" && modelState.ObjName == "")
                ModelState.AddModelError(Constants.ObjectiveName, Constants.ObjectiveNameMsg);

            if (modelState.StartDate != null && Convert.ToString(modelState.StartDate) == null)
                ModelState.AddModelError(Constants.StartDate, Constants.StartDateMsg);

            if (modelState.EndDate != null && Convert.ToString(modelState.EndDate) == null)
                ModelState.AddModelError(Constants.EndDate, Constants.EndDateMsg);


        }

        private void ValidateKrModelState(MyGoalsRequest myGoalsRequest)
        {
            foreach (var data in myGoalsRequest.MyGoalsDetails)
            {
                ModelStateError modelState = new ModelStateError()
                {
                    KeyDesc = data.KeyDescription,
                    MetricId = data.MetricId,
                    CurrencyId = data.CurrencyId,
                    TargetValue = data.TargetValue,
                    StartDate = data.StartDate,
                    DueDate = data.DueDate
                };

                if (modelState.KeyDesc != "-DoNotValidate-" && modelState.KeyDesc == "")
                    ModelState.AddModelError(Constants.KeyDescription, Constants.KeyDescriptionMsg);

                if (modelState.MetricId == (int)Metrics.Currency && modelState.CurrencyId == 0)
                    ModelState.AddModelError(Constants.CurrencyId, Constants.CurrencyIdMessage);

                ////if (modelState.MetricId != (int)Metrics.NoUnits && modelState.MetricId != (int)Metrics.Boolean && modelState.MetricId != 0)
                ////    ModelState.AddModelError(Constants.TargetValueKr, Constants.TargetValueMessage);

                ////if (modelState.MetricId == 0)
                ////    ModelState.AddModelError(Constants.Metric, Constants.MetricMessage);

                ////if (modelState.MetricId == (int)Metrics.Boolean && modelState.MetricId != 0 && modelState.TargetValue > 0)
                ////    ModelState.AddModelError(Constants.BooleanMetric, Constants.BooleanMetricMessage);

                ////if (modelState.StartDate < myGoalsRequest.StartDate || modelState.StartDate >= myGoalsRequest.EndDate)
                ////   ModelState.AddModelError(Constants.StartDate, Constants.StartDateMessage);

                if (modelState.DueDate > myGoalsRequest.EndDate)
                    ModelState.AddModelError(Constants.DueDate, Constants.DueDateMsg);

                foreach (var contributor in data.Contributors)
                {
                    ModelStateError model = new ModelStateError()
                    {
                        EmpId = contributor.EmployeeId,
                        StartDate = contributor.StartDate,
                        DueDate = contributor.DueDate
                    };

                    ////if (model.StartDate < data.StartDate || model.StartDate > data.DueDate)
                    ////    ModelState.AddModelError(Constants.StartDate, Constants.StartDateContMessage);

                    ////if (model.DueDate > data.DueDate)
                    ////    ModelState.AddModelError(Constants.DueDate, Constants.DueDateContMsg);

                }

            }
        }

        private List<string> ValidateDueDate(UpdateDueDateRequest updateDueDateRequest, long empId , bool isAlignment)
        {
            List<string> validate = new List<string>();
            if (updateDueDateRequest.GoalType == Constants.GoalObjectiveType)
            {              
               
                var goalKeyDetails = myGoalsService.GetGoalKey(updateDueDateRequest.GoalId);
                foreach (var item in goalKeyDetails)
                {
                    if (item.DueDate > updateDueDateRequest.EndDate)
                    {
                        item.DueDate = updateDueDateRequest.EndDate;
                    }

                    if (item.StartDate < updateDueDateRequest.StartDate)
                    {
                        item.StartDate = updateDueDateRequest.StartDate;
                    }

                    if (updateDueDateRequest.StartDate > item.DueDate)
                    {
                        validate.Add(Constants.StartDate);
                    }

                    if (updateDueDateRequest.EndDate < item.StartDate)
                    {
                        validate.Add(Constants.DueDate);
                    }
                    var allLevelKrContributors = commonService.GetAllLevelKrContributors(Constants.GoalKeyType, item.GoalKeyId, Convert.ToInt64(item.EmployeeId)).Where(x => x.GoalObjectiveId == Constants.Zero).ToList();
                    foreach (var krstandcontri in allLevelKrContributors)
                    {
                        if (krstandcontri.StartDate < updateDueDateRequest.StartDate)
                        {
                            krstandcontri.StartDate = updateDueDateRequest.StartDate;
                        }
                        if (krstandcontri.DueDate > updateDueDateRequest.EndDate)
                        {
                            krstandcontri.DueDate = updateDueDateRequest.EndDate;
                        }
                        if (updateDueDateRequest.StartDate > krstandcontri.DueDate)
                        {
                            validate.Add(Constants.StartDate);
                        }
                        if (updateDueDateRequest.EndDate < krstandcontri.StartDate)
                        {
                            validate.Add(Constants.DueDate);
                        }
                    }
                }
                var allLevelObjContributors = commonService.GetObjectiveSubCascading(updateDueDateRequest.GoalId).Where(x => x.GoalId != updateDueDateRequest.GoalId && x.GoalId > 0).ToList();
                foreach (var krparentcontr in allLevelObjContributors)
                {
                    var key = myGoalsService.GetGoalKey(krparentcontr.GoalId);
                    foreach (var item in key)
                    {
                        if (item.StartDate < updateDueDateRequest.StartDate)
                        {
                            item.StartDate = updateDueDateRequest.StartDate;
                        }
                        if (item.DueDate > updateDueDateRequest.EndDate)
                        {
                            item.DueDate = updateDueDateRequest.EndDate;
                        }
                        if (updateDueDateRequest.StartDate > item.DueDate)
                        {
                            validate.Add(Constants.StartDate);
                        }
                        if (updateDueDateRequest.EndDate < item.StartDate)
                        {
                            validate.Add(Constants.DueDate);
                        }
                    }
                }
            }
            else
            {
              
                var allLevelKrContributors = commonService.GetAllLevelKrContributors(updateDueDateRequest.GoalType, updateDueDateRequest.GoalId, empId).Where(x => x.GoalId != updateDueDateRequest.GoalId);
                foreach (var item in allLevelKrContributors)
                {
                    if (item.StartDate < updateDueDateRequest.StartDate)
                    {
                        item.StartDate = updateDueDateRequest.StartDate;
                    }
                    if (item.DueDate > updateDueDateRequest.EndDate)
                    {
                        item.DueDate = updateDueDateRequest.EndDate;
                    }
                    if (updateDueDateRequest.StartDate > item.DueDate)
                    {
                        validate.Add(Constants.StartDate);
                    }
                    if (updateDueDateRequest.EndDate < item.StartDate)
                    {
                        validate.Add(Constants.DueDate);
                    }
                }
            }

            return validate;
        }

        #endregion
    }
}
