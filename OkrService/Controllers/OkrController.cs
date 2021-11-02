using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OkrService.DataContract;
using OkrService.Services;
using OkrService.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Cors;

namespace OkrService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OkrController : ControllerBase
    {

        [FromHeader(Name = "ControlerHeader")]
        [Required]
        public string ControlerHeader { get; set; }

        private readonly IOkrService _okrServices;

        public OkrController(IOkrService okrServices)
        {
            _okrServices = okrServices;
        }
        // GET: api/Okr
        // [ApiExplorerSettings(IgnoreApi = true)]
        [Route("ImportOkr")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult ImportOkr(long supId, int qtr, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<ImportOkr>();
            //var header = ControlerHeader;

            try
            {

                if (supId == 0)
                {
                    ModelState.AddModelError("supId", "SupervisorId can't be 0. ");
                }
                if (qtr == 0)
                {
                    ModelState.AddModelError("Quarter", "Quarter can't be 0. ");
                }
                if (year == 0)
                {
                    ModelState.AddModelError("Year", "Year can't be 0. ");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.ImportOkr(supId, year, qtr, header);
                    if (payloadOutputGet.Entity != null)
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Error.ToString();
                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.MessageList.Add("Result", "There is no List.");
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;

                }



                return Ok(payloadOutputGet);
            }
            catch (Exception ex)
            {

                payloadOutputGet.MessageType = Common.MessageType.Error.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "GetOkrDetails", "OkrService", ex + "InnerException:" + ex.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("SaveProgression")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveProgress(List<ProgressionRequest> progressionRequest)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<ProgressionMaster>();
            try
            {
                foreach (var item in progressionRequest)
                {
                    var count = progressionRequest.Count;
                    if (item.Year < 2020 || item.Year > 9999)
                    {
                        payloadOutputSave.IsSuccess = true;
                        ModelState.AddModelError("year", "Year range must be between 2020 to 9999. ");
                    }

                    if (item.ProgressName == "")
                    {
                        payloadOutputSave.IsSuccess = true;
                        ModelState.AddModelError("progressName", "ProgressName can't be blank. ");
                    }

                    if (item.Period < 1 || item.Period > 4)
                    {
                        payloadOutputSave.IsSuccess = true;
                        ModelState.AddModelError("period", "Period range should be between 1-4 ");
                    }

                }

                if (ModelState.IsValid)
                {
                    var insertedid = _okrServices.SaveProgression(progressionRequest, loginUser);
                    if (insertedid.Count > 0)
                    {
                        payloadOutputSave.EntityList = insertedid;
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        //  var a = state.Key.Contains(state.Key);

                        foreach (var error in state.Value.Errors)
                        {
                            if (!payloadOutputSave.MessageList.ContainsKey(state.Key))
                            {
                                payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                            }
                        }



                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "SaveProgress", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("GetProgression")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult GetProgression(ProgressionResponse progressionResponse)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<ProgressionMaster>();
            try
            {
                payloadOutputSave.EntityList = _okrServices.GetProgression(progressionResponse.Year, progressionResponse.Period).ToList();
                if (payloadOutputSave.EntityList.Count > 0)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
                else
                {
                    payloadOutputSave.MessageList.Add("year", "There is no progression for that particular period and year.");
                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "GetProgression", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("SaveOkr")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult SaveOkr(List<OkrRequest> okrRequests)
        {

            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            // var user = _okrServices.GetAllUserFromUserManagement(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<SaveOkrResponce>();
            var Status = _okrServices.GetStatusMasters().Count;

            try
            {
                foreach (var item in okrRequests)
                {
                    if (item.Status != 0)
                    {



                        if (item.Year < 2020 || item.Year > 9999)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("year", "Year range must be between 2020 to 9999. ");
                        }
                        else if (item.Quarter < 1 || item.Quarter > 4)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("quarter", "Quarter range should be between 1-4 ");
                        }
                        else if (item.Type < 1 || item.Type > 2)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("type", "Type should be 1 or 2 ");
                        }
                        else if (item.OkrObjective == "")
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("okrObjective", "OkrObjective can't be blank.");
                        }
                        else if (item.Status >= Status)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("status", "OkrStatus range between 0-7 ");
                        }
                        else if (item.EmployeeId == 0)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("employeeId", "EmployeeId can't be 0 ");
                        }
                        else if (item.EmployeeId != loginUser.EmployeeId)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("employeeId", "EmployeeId should be login user.");
                        }
                        else if (item.Owner == 0)
                        {
                            payloadOutputSave.IsSuccess = true;
                            ModelState.AddModelError("owner", "Owner can't be 0 ");
                        }
                        else if (item.Owner != 0)
                        {
                            var ownerId = _okrServices.GetAllUserFromUserManagement(header).Count(x => x.employeeId == item.Owner);
                            if (ownerId == 0)
                            {
                                payloadOutputSave.IsSuccess = true;

                                ModelState.AddModelError("Owner", "Please select active Owner. ");
                            }
                        }
                    }


                    foreach (var data in item.OkrDetailsList)
                    {
                        if (data.Status != 0)
                        {


                            DateTime dtFirstDay = new DateTime(item.Year, 3 * item.Quarter - 2, 1);
                            DateTime dtLastDay = new DateTime(item.Year, 3 * item.Quarter + 1, 1).AddDays(-1);

                            if (data.KeyDesc == "")
                            {
                                payloadOutputSave.IsSuccess = true;
                                ModelState.AddModelError("keyDesc", "Key Description can't be blank ");
                            }
                            else if (data.Status >= Status)
                            {
                                payloadOutputSave.IsSuccess = true;
                                ModelState.AddModelError("status", "OkrDetailStatus range between 0-7 ");
                            }
                            else if (!(data.DueDate >= dtFirstDay && data.DueDate <= dtLastDay))
                            {
                                payloadOutputSave.IsSuccess = true;
                                ModelState.AddModelError("dueDate", "Due date should be in this quarter.");
                            }
                        }
                    }

                }

                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.SaveOkr(okrRequests, header);
                    if (payloadOutputSave.EntityList != null)
                    {
                        var returnStatus = okrRequests.FirstOrDefault();
                        if (returnStatus.Status == 1)
                        {
                            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                            payloadOutputSave.IsSuccess = true;
                            payloadOutputSave.MessageList.Add("Result", "Okr saved as Draft");
                            payloadOutputSave.Status = Response.StatusCode;
                        }
                        if (returnStatus.Status == 3)
                        {
                            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                            payloadOutputSave.IsSuccess = true;
                            payloadOutputSave.MessageList.Add("Result", "Okr is saved and completed.");
                            payloadOutputSave.Status = Response.StatusCode;
                        }

                        if (returnStatus.Status == 0)
                        {
                            payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                            payloadOutputSave.IsSuccess = true;
                            payloadOutputSave.MessageList.Add("Result", "Okr is deleted");
                            payloadOutputSave.Status = Response.StatusCode;
                        }
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            if (!payloadOutputSave.MessageList.ContainsKey(state.Key))
                            {
                                payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                            }

                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputSave);


            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "SaveOkr", "SaveOkr", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("MyOkr")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult MyOkr(long empId, int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<OkrRequest>();
            try
            {
                if (empId == 0)
                {
                    ModelState.AddModelError("empId", "EmployeeId cant be blank.");
                }

                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "EmployeeId cant be blank.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "EmployeeId cant be blank.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.MyOkr(empId, quarter, year, header);
                    if (payloadOutputSave.EntityList.Count > 0)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("empId", "There is no Okr for the particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadOutputSave);


            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "MyOkr", "MyOkr", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("StatusMaster")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult GetStatusMaster()
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadGet = new PayloadCustom<StatusMaster>();
            try
            {
                payloadGet.EntityList = _okrServices.GetStatusMasters();
                if (payloadGet.EntityList.Count > 0)
                {
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("Id", "There is no Status");
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadGet);
            }
            catch (Exception e)
            {
                payloadGet.MessageType = Common.MessageType.Warning.ToString();
                payloadGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "GetStatusMaster", "MyOkr", e + "InnerException:" + e.InnerException);
                return NotFound(payloadGet);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("Reset")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult Reset(long employeeId, int year, int quarter, long okrId, long resetType)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadGet = new PayloadCustom<string>();
            try
            {


                if (employeeId == 0)
                {
                    ModelState.AddModelError("employeeId", "EmployeeId cant be blank.");
                }
                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "quarter cant be 0.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "year cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadGet.Entity = _okrServices.ResetOkr(employeeId, year, quarter, okrId, loginUser.EmployeeId, resetType);
                    if (payloadGet.Entity == "")
                    {
                        payloadGet.MessageType = Common.MessageType.Success.ToString();
                        payloadGet.MessageList.Add("Result", "Reset is done successfully.");
                        payloadGet.IsSuccess = true;
                        payloadGet.Status = Response.StatusCode;
                    }
                    else
                    {
                        payloadGet.MessageList.Add("Result", "Reset is not done successfully");
                        payloadGet.IsSuccess = false;
                        payloadGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {

                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadGet.IsSuccess = false;
                    payloadGet.Status = (int)HttpStatusCode.BadRequest;
                }


                return Ok(payloadGet);
            }

            catch (Exception e)
            {
                payloadGet.MessageType = Common.MessageType.Warning.ToString();
                payloadGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "Reset", "MyOkr", e + "InnerException:" + e.InnerException);
                return NotFound(payloadGet);
            }
        }

        [Route("QuarterSummary")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult QuarterSummary(int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<QuarterSummaryResponse>();
            try
            {
                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "quarter cant be 0.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "year cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.QuarterSummary(quarter, year, header);
                    if (payloadOutputSave.EntityList.Count > 0)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("year", "There is no Okr for that particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "QuarterSummary", "QuarterSummary", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("OkrStatusReport")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult OkrStatusReport(int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<OkrStatusResponse>();
            try
            {
                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "quarter cant be 0.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "year cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.OkrStatusReport(quarter, year, header);
                    if (payloadOutputSave.EntityList.Count > 0)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("year", "There is no Okr for that particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }


                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "QuarterSummary", "QuarterSummary", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("OkrProgressReport")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult OkrProgressReport(int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<OkrProgressResponse>();
            try
            {
                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "quarter cant be 0.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "year cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.OkrProgressReport(quarter, year, header);
                    if (payloadOutputSave.EntityList.Count > 0)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("year", "There is no Okr for that particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "OkrProgressReport", "OkrProgressReport", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }
        [Route("ProgressionSettings")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult GetSettingProgression()
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<OkrSettingsForProgression>();
            try
            {
                payloadOutputGet.EntityList = _okrServices.OkrSettingsForProgressions();
                if (payloadOutputGet.EntityList.Count > 0)
                {
                    payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputGet.IsSuccess = true;
                    payloadOutputGet.Status = Response.StatusCode;
                }
                else
                {

                    //  payloadOutputGet.MessageList.Add("year", "There is no Okr for that particular quarter and year.");
                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "OkrProgressReport", "OkrProgressReport", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }
        [Route("SaveComments")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SaveComments(CommentRequest commentRequest)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<CommentResponce>();
            try
            {
                if (commentRequest.Comment == "")
                {
                    ModelState.AddModelError("comment", "Comments cant be blank.");
                }
                else if (commentRequest.Type < 1 || commentRequest.Type > 2)
                {
                    ModelState.AddModelError("type", "Type should be 1 0r 2");
                }
                else if (commentRequest.TypeId == 0)
                {
                    ModelState.AddModelError("typeId", "TypeId cant be 0.");
                }
                else if (commentRequest.CreatedBy == 0)
                {
                    ModelState.AddModelError("createdBy", "CreatedBy cant be 0.");
                }
                else if (commentRequest.CreatedBy != loginUser.EmployeeId)
                {
                    ModelState.AddModelError("createdBy", "CreatedBy should be login user.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.SaveComments(commentRequest, header);
                    if (payloadOutputGet.Entity != null)
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.MessageList.Add("CommentId", "Insert is successfull");
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {


                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "SaveComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }


        }
        [Route("GetComments")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult GetComments(int type, long typeId)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<CommentResponce>();
            try
            {
                if (type == 0)
                {
                    ModelState.AddModelError("type", "Type cant be 0");
                }
                else if (typeId == 0)
                {
                    ModelState.AddModelError("typeId", "TypeId cant be 0");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputGet.EntityList = _okrServices.GetComments(type, typeId, header);
                    if (payloadOutputGet.EntityList.Count > 0)
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();

                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputGet.MessageList.Add("createdBy", "There  is no comments for particular employee");
                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "GetComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }

        }

        [Route("ReadComments")]
        [HttpPut]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ReadComments(long employeeId, int type, long typeId)
        {
            var header = ControlerHeader;
            if (_okrServices.GetIdentity(header) == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<string>();
            try
            {
                if (employeeId == 0)
                {
                    ModelState.AddModelError("employeeId", "EmployeeId cant be 0.");
                }
                else if (type == 0)
                {
                    ModelState.AddModelError("type", "type cant be 0.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.ReadComments(employeeId, type, typeId, header);
                    if (payloadOutputGet.Entity == "")
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.MessageList.Add("commentsId", "Comment is Read Successfully");
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {
                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "ReadComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("ImportEmployeeOkr")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult ImportEmployeeOkr(long employeeId, int year, int currentQtr)
        {
            var header = ControlerHeader;
            if (_okrServices.GetIdentity(header) == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<ImportEmployeeOkr>();
            try
            {
                if (employeeId == 0)
                {
                    ModelState.AddModelError("employeeId", "EmployeeId cant be 0.");
                }
                if (year == 0)
                {
                    ModelState.AddModelError("year", "Year cant be 0.");
                }
                if (currentQtr == 0)
                {
                    ModelState.AddModelError("currentQtr", "CurrentQtr cant be 0.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.ImportEmployeeOkr(employeeId, year, currentQtr, header);
                    if (payloadOutputGet.Entity != null)
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.MessageList.Add("Result", "Import has been done Successfully");
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {


                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "ReadComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("GetOkrByOkrId")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult GetOkrByOkrId(long okrId)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<OkrRequest>();
            try
            {
                if (okrId == 0)
                {
                    ModelState.AddModelError("okrId", "OkrId cant be 0.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputGet.EntityList = _okrServices.OkrByOkrId(okrId, header, loginUser);
                    if (payloadOutputGet.EntityList.Count > 0)
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();

                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputGet.MessageList.Add("Result", "There is no Okr for particular OkrId");
                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }


                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "GetOkrByOkrId", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("DeleteOkr")]
        [HttpDelete]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult DeleteOkr(long okrId, long keyId)
        {
            var header = ControlerHeader;
            if (_okrServices.GetIdentity(header) == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<string>();
            try
            {
                if (okrId == 0)
                {
                    payloadOutputGet.MessageList.Add("okrId", "OkrId cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.DeleteOkr(okrId, keyId, header);
                    if (payloadOutputGet.Entity == "")
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.MessageList.Add("status", "Delete has been done successfully");
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {


                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {

                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputGet.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "DeleteOkr", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("DeleteComments")]
        [HttpDelete]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult DeleteComments(long commentsId)
        {
            var payloadOutputGet = new PayloadCustom<string>();
            try
            {
                if (commentsId == 0)
                {
                    payloadOutputGet.MessageList.Add("commentsId", "commentsId cant be 0.");
                }
                if (ModelState.IsValid)
                {
                    payloadOutputGet.Entity = _okrServices.DeleteComments(commentsId);

                    if (payloadOutputGet.Entity == "")
                    {
                        payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputGet.MessageList.Add("status", "Delete of Comments  has been done successfully");
                        payloadOutputGet.IsSuccess = true;
                        payloadOutputGet.Status = Response.StatusCode;
                    }
                    else
                    {


                        payloadOutputGet.IsSuccess = false;
                        payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                    }
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "DeleteComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("Dashboard")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult Dashboard(long empId, int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<Dashboard>();
            try
            {
                if (empId == 0)
                {
                    ModelState.AddModelError("empId", "EmployeeId cant be blank.");
                }

                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "EmployeeId cant be blank.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "EmployeeId cant be blank.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputSave.Entity = _okrServices.Dashboard(empId, quarter, year, header, loginUser);
                    if (payloadOutputSave.Entity != null)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("empId", "There is no Okr for the particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadOutputSave);


            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "Dashboard", "Dashboard", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("AutoSubmit")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AutoSubmit()
        {
            var payloadOutputGet = new PayloadCustom<bool>();
            try
            {
                payloadOutputGet.Entity = _okrServices.AutoSubmit();

                if (payloadOutputGet.Entity == true)
                {
                    payloadOutputGet.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputGet.MessageList.Add("status", "Okr for this quarter auto submitted successfully");
                    payloadOutputGet.IsSuccess = true;
                    payloadOutputGet.Status = Response.StatusCode;
                }
                else
                {


                    payloadOutputGet.IsSuccess = false;
                    payloadOutputGet.Status = (int)HttpStatusCode.BadRequest;
                }

                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "DeleteComments", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }
        }

        [Route("MyOkrTeam")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult MyOkrTeam(int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<MyOkrTeamResponse>();
            try
            {

                if (ModelState.IsValid)
                {
                    payloadOutputSave.EntityList = _okrServices.GetMyOkrTeam(quarter, year, header, loginUser);
                    if (payloadOutputSave.EntityList != null)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("UserId", "There is no user");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadOutputSave);


            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrController", "MyOkrTeam", "MyOkrTeam", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("CalculateScore")]
        [HttpPost]
        [EnableCors("AllowOrigin")]
        public IActionResult CalculateScore(int quarter, int year)
        {
            var payloadOutputSave = new PayloadCustom<int>();
            try
            {
                payloadOutputSave.Entity = _okrServices.CalculateAvgScoreAtAutoSubmit(quarter, year);
                if(payloadOutputSave.Entity == 1)
                {
                    payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                    payloadOutputSave.IsSuccess = true;
                    payloadOutputSave.Status = Response.StatusCode;
                }
                return Ok(payloadOutputSave);
            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrController", "CalculateScore", "CalculateScore", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }

        [Route("GetResetLog")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult GetResetLog()
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputGet = new PayloadCustom<OkrResetLog>();
            try
            {
                payloadOutputGet.EntityList = _okrServices.GetResetLog();
                return Ok(payloadOutputGet);
            }
            catch (Exception e)
            {
                payloadOutputGet.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputGet.IsSuccess = false;
                _okrServices.SaveLog("OkrController", "GetResetLog", "OkrService", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputGet);
            }

        }

        [Route("ScoreCard")]
        [HttpGet]
        [EnableCors("AllowOrigin")]
        public IActionResult ScoreCard(long empId, int quarter, int year)
        {
            var header = ControlerHeader;
            var loginUser = _okrServices.GetIdentity(header);
            if (loginUser == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }
            var payloadOutputSave = new PayloadCustom<ScoreCardResponse>();
            try
            {
                if (empId == 0)
                {
                    ModelState.AddModelError("empId", "EmployeeId cant be blank.");
                }

                if (quarter == 0)
                {
                    ModelState.AddModelError("quarter", "EmployeeId cant be blank.");
                }

                if (year == 0)
                {
                    ModelState.AddModelError("year", "EmployeeId cant be blank.");
                }

                if (ModelState.IsValid)
                {
                    payloadOutputSave.Entity = _okrServices.ScoreCard(empId, quarter, year, header, loginUser);
                    if (payloadOutputSave.Entity.EmployeeId != null)
                    {
                        payloadOutputSave.MessageType = Common.MessageType.Success.ToString();
                        payloadOutputSave.IsSuccess = true;
                        payloadOutputSave.Status = Response.StatusCode;
                    }
                    else
                    {

                        payloadOutputSave.MessageList.Add("empId", "There is no Okr for the particular quarter and year.");
                        payloadOutputSave.IsSuccess = false;
                        payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    var errors = new Dictionary<string, string>();

                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {

                            payloadOutputSave.MessageList.Add(state.Key, error.ErrorMessage);
                        }
                    }

                    payloadOutputSave.IsSuccess = false;
                    payloadOutputSave.Status = (int)HttpStatusCode.BadRequest;
                }
                return Ok(payloadOutputSave);


            }
            catch (Exception e)
            {
                payloadOutputSave.MessageType = Common.MessageType.Warning.ToString();
                payloadOutputSave.IsSuccess = false;
                _okrServices.SaveLog("OkrDataProvider", "MyOkr", "MyOkr", e + "InnerException:" + e.InnerException);
                return NotFound(payloadOutputSave);
            }
        }
    }
}
