using OkrService.DataContract;
using OkrService.DataProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using OkrService.Models;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;

namespace OkrService.Services
{
    public class OkrServices : IOkrService
    {
        private readonly IOkrDataProvider _OkrDataProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        private string sessionId;
        private string appId;
        private readonly IConfiguration _iconfiguration;
        public OkrServices(IOkrDataProvider okrDataProvider, IMemoryCache memoryCache, IMapper mapper, IConfiguration iconfiguration)
        {
            _OkrDataProvider = okrDataProvider;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _iconfiguration = iconfiguration;
        }
        public List<PreviousOkrResponse> GetOkrDetails(long empId, long supId, int qtr, int year)
        {
            List<PreviousOkrResponse> result = new List<PreviousOkrResponse>();
            try
            {
                //var allUser = GetAllUser();


                //var data = _OkrDataProvider.GetOkrDetails(empId, supId, qtr, year);
                //if (data != null && allUser != null)
                //{
                //    result = (from u in allUser
                //              join d in data on u.EmployeeId equals d.createdById
                //              select new PreviousOkrResponse
                //              {
                //                  Quarter = d.Quarter,
                //                  Year = d.Year,
                //                  OkrId = d.OkrId,
                //                  OkrName = d.OkrName,
                //                  KeyId = d.KeyId,
                //                  KeyName = d.KeyName,
                //                  Score = d.Score,
                //                  createdById = d.createdById,
                //                  createdByName = u.FirstName + " " + u.LastName,
                //                  ImportFrom = d.ImportFrom
                //              }).ToList();
                //}

                result = _OkrDataProvider.GetOkrDetails(empId, supId, qtr, year);


            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetOkrDetails", "OkrService", ex.ToString());
            }

            return result;
        }
        public bool GetHeaderValue(string header)
        {

            bool isValidToken = false;
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(header);
                string actualHeader = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                //string actualHeader = WebUtility.UrlDecode(header);
                string[] tokens = actualHeader.Split(':');

                if (tokens.Length > 0)
                {
                    if (tokens[2] == "F8B09621-FA5C-4712-9CF5-A3535F9E8945")
                    {
                        isValidToken = true;
                        sessionId = tokens[0];
                        appId = tokens[1];
                    }
                    else
                    {
                        isValidToken = false;
                    }
                }
                else
                {
                    isValidToken = false;
                }
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetHeaderValue", "OkrService", ex.ToString());
            }
            return isValidToken;
        }
        public UserDetail GetLoginUser()
        {
            UserDetail loginUserDetail = new UserDetail();

            try
            {
                string privatekey = "D240B4F5-C317-47AA-A4E0-66B0BB5AA055";
                string ssoBaseUrl = "http://passportdev.compunnel.com/api/";
                string issueUrl = ssoBaseUrl + "api/User/ApplicationUserAuthentication";
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("sessionid", sessionId);
                header.Add("appid", appId);
                header.Add("privatekey", privatekey);
                HttpWebRequest ssoRequest = GetHttpWebRequest(issueUrl, "GET", null, header);
                using (HttpWebResponse response = (HttpWebResponse)ssoRequest.GetResponse())
                {

                    Stream dataStream = response.GetResponseStream();
                    if (dataStream != null)
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        var result = reader.ReadToEnd();

                        var ssoUsers = JsonConvert.DeserializeObject<List<PassportEmployeeResponse>>(result);
                        if (ssoUsers != null)
                        {
                            loginUserDetail = (from s in ssoUsers
                                               where s != null
                                               select new UserDetail()
                                               {
                                                   UserId = s.EmployeeId.ToString(),
                                                   MailId = s.MailId.ToString(),
                                                   SupervisorId = s.ReportingTo.ToString(),
                                                   SupervisorName = s.ReportingToName.ToString(),
                                                   Lob = s.LOBName.ToString(),
                                                   GroupName = s.DivisionName.ToString(),
                                                   RoleName = s.CompetencyName.ToString(),
                                                   Designation = s.DesignationName.ToString(),
                                                   FirstName = s.FirstName.ToString(),
                                                   LastName = s.LastName.ToString(),
                                                   ContractFrom = s.DateOfJoining,
                                                   UserName = s.EmployeeId.ToString(),
                                                   Grade = s.GradeID.ToString(),
                                                   Division = s.FunctionName.ToString(),
                                                   Team = s.CompetencyName.ToString(),
                                                   Business = s.DivisionName.ToString(),
                                                   ImageUrl =
                                                       !String.IsNullOrEmpty(s.Image.ToString())
                                                           ? s.Image.ToString()
                                                           : ""

                                               }).FirstOrDefault();
                        }
                        else
                        {
                            loginUserDetail = null;
                        }
                    }
                    else
                    {
                        loginUserDetail = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetLoginUser", "OkrService", ex.ToString());
            }
            return loginUserDetail;
        }
        public List<PassportEmployeeResponse> GetAllUserFromPassport()
        {
            string cacheKey = "PassportUserDetails".ToLower();
            List<PassportEmployeeResponse> loginUserDetail = new List<PassportEmployeeResponse>();
            try
            {
                //if (!_memoryCache.TryGetValue(cacheKey, out loginUserDetail))
                //{
                    string PassportUrl = _iconfiguration.GetValue<string>("Passport:URL");
                    PassportUrl = PassportUrl + "User?userType=Employee";
                    HttpWebRequest passportRequest = GetHttpWebRequest(PassportUrl, "GET", null, null);
                    using (HttpWebResponse response = (HttpWebResponse)passportRequest.GetResponse())
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        var result = reader.ReadToEnd();
                        var passportUsers = JsonConvert.DeserializeObject<PayloadCustomPassport<PassportEmployeeResponse>>(result);
                        loginUserDetail = passportUsers.EntityList;
                       
                    }
                //    _memoryCache.Set(cacheKey, loginUserDetail);
                //}
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetLoginUser", "OkrService", ex.ToString());
            }
            return loginUserDetail;
        }
        public void SaveNotification(NotificationsResponse notificationsResponse)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri(_iconfiguration.GetValue<string>("Notifications:URL"));
                var response = client.PostAsJsonAsync("api/OkrNotifications/InsertNotificationsDetails", notificationsResponse).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.Write("Success");
                }
                else
                    Console.Write("Error");
            }
        }
        public List<UserManagementResponse> GetAllUserFromUserManagement(string jwtToken)
        {
            string cacheKey = "UserDetails".ToLower();
            List<UserManagementResponse> loginUserDetail = new List<UserManagementResponse>();
            try
            {
                //if (!_memoryCache.TryGetValue(cacheKey, out loginUserDetail))
                //{
                    HttpWebRequest passportRequest = GetHttpWebRequest(_iconfiguration.GetValue<string>("UserManagement:UserURL"), "GET", null, null, jwtToken);
                    using (HttpWebResponse response = (HttpWebResponse)passportRequest.GetResponse())
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        var result = reader.ReadToEnd();

                        var passportUsers = JsonConvert.DeserializeObject<PayloadCustomGenric<UserManagementResponse>>(result);
                        loginUserDetail = passportUsers.entityList;
                    }
                //    _memoryCache.Set(cacheKey, loginUserDetail);
                //}
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetAllUserFromUserManagement", "OkrService", ex.ToString());
            }
            return loginUserDetail;
        }
        public HttpWebRequest GetHttpWebRequest(string restUrl, string method, Dictionary<string, string> parameters, Dictionary<string, string> headers = null, string jwtToken = null)
        {

            string header = "";
            HttpWebRequest request = null;
            try
            {
                if (method.ToUpper() == "POST" && jwtToken != "")
                {
                    request = WebRequest.Create(restUrl) as HttpWebRequest;
                    request.Method = method;
                    request.Accept = "application/json";
                    request.ContentType = "application/json";
                    if (jwtToken != null)
                    {
                        request.Headers.Add("Authorization", "Bearer " + jwtToken);
                    }
                    if (parameters == null)
                    {
                        request.ContentLength = 0;
                    }
                }
                if (method.ToUpper() == "GET" && parameters == null && jwtToken == "")
                {
                    request = WebRequest.Create(restUrl) as HttpWebRequest;
                    if (headers != null)
                    {
                        foreach (string key in headers.Keys)
                        {
                            header += HttpUtility.UrlEncode(headers[key]) + ":";
                        }
                        header = header.TrimEnd(':');
                        byte[] bytes = Encoding.UTF8.GetBytes(header);
                        request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(bytes));
                    }
                    request.Method = method;
                    request.Accept = "application/json";
                    request.ContentType = "application/json";
                    request.ContentLength = 0;

                }
                if (method.ToUpper() == "GET" && jwtToken != "")
                {
                    request = WebRequest.Create(restUrl) as HttpWebRequest;
                    if (jwtToken != null)
                    {
                        request.Headers.Add("Authorization", "Bearer " + jwtToken);
                    }
                    request.Method = method;
                    request.Accept = "application/json";
                    request.ContentType = "application/json";
                    request.ContentLength = 0;

                }
                return request;
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetLoginUser", "OkrService", ex.ToString());
            }

            return request;
        }

        public void SaveLog(string pageName, string functionName, string applicationName, string errorDetail)
        {
            _OkrDataProvider.SaveLog(pageName, functionName, applicationName, errorDetail);
        }
        public List<ProgressionMaster> SaveProgression(List<ProgressionRequest> progressionRequests, Identity loginUser)
        {
            List<ProgressionMaster> progressionRequest = new List<ProgressionMaster>();
            try
            {
                foreach (var data in progressionRequests)
                {
                    ProgressionMaster progressionMaster = new ProgressionMaster();
                    if (data.Id > 0)
                    {
                        progressionMaster = _OkrDataProvider.Progressionbyid(data.Id);
                        progressionMaster.ProgressName = data.ProgressName;
                        progressionMaster.ProgressPercentage = 1;
                        progressionMaster.LastChangedBy = loginUser.EmployeeId;
                        progressionMaster.LastChangedOn = DateTime.UtcNow;
                        _OkrDataProvider.UpdateProgression(progressionMaster);
                    }
                    else
                    {
                        progressionMaster.Year = data.Year;
                        progressionMaster.Period = data.Period;
                        progressionMaster.ProgressName = data.ProgressName;
                        progressionMaster.CreatedBy = loginUser.EmployeeId;
                        progressionMaster.ProgressValue = 1;
                        progressionMaster.ProgressPercentage = 1;
                        _OkrDataProvider.SaveProgression(progressionMaster);

                    }
                }

                var resultObj = progressionRequests.FirstOrDefault();
                _OkrDataProvider.UpdateProgressPercentage(resultObj.Year, resultObj.Period);
                progressionRequest = GetProgression(resultObj.Year, resultObj.Period);
            }
            catch (Exception e)
            {
                SaveLog("OkrServices", "SaveProgression", "OkrService", e.ToString());
            }

            return progressionRequest;
        }
        public List<ProgressionMaster> GetProgression(int year, int period)
        {
            List<ProgressionMaster> progression = new List<ProgressionMaster>();

            try
            {
                progression = _OkrDataProvider.GetProgression(year, period).ToList();
            }
            catch (Exception e)
            {
                SaveLog("OkrServices", "GetProgression", "OkrService", e.ToString());
            }

            return progression;
        }
        public List<SaveOkrResponce> SaveOkr(List<OkrRequest> okrRequestList, string jwtToken)
        {
            List<SaveOkrResponce> SaveOkrResponceList = new List<SaveOkrResponce>();
            var userList = GetAllUserFromUserManagement(jwtToken);
            try
            {
                foreach (var item in okrRequestList)
                {
                    Okr okr = new Okr();
                    SaveOkrResponce saveOkrResponce = new SaveOkrResponce();
                    var progression = GetProgression(item.Year, item.Quarter);
                    long minProgression = progression.OrderBy(x => x.ProgressValue).FirstOrDefault().Id;
                    if (item.OkrId == 0)
                    {
                        okr.Year = item.Year;
                        okr.Quarter = item.Quarter;
                        okr.EmployeeId = item.EmployeeId;
                        okr.OkrObjective = item.OkrObjective;
                        okr.Type = item.Type;
                        okr.Status = item.Status;
                        okr.IsImported = item.IsImported;
                        okr.ImportedId = item.ImportedId;
                        okr.Owner = item.Owner;
                        okr.OkrDescription = item.OkrDescription;
                        _OkrDataProvider.SaveOkrObjective(okr);

                        foreach (var data in item.OkrDetailsList)
                        {
                            OkrDetails okrDetails = new OkrDetails();
                            okrDetails.OkrId = okr.OkrId;
                            okrDetails.KeyDesc = data.KeyDesc;
                            okrDetails.Status = data.Status;
                            okrDetails.Score = minProgression;
                            okrDetails.IsNotify = data.IsNotify;
                            okrDetails.DueDate = data.DueDate;
                            _OkrDataProvider.SaveOkrKey(okrDetails);

                        }

                        if (item.ImportedId != 0 && item.Status == 1)//Notification will create for owner of Objective import
                        {
                            OkrDetails prevKey = new OkrDetails();
                            Okr prevOkr = new Okr();
                            if (item.ImportedType == 2)
                            {
                                prevKey = _OkrDataProvider.GetOkrKeyById(Convert.ToInt64(item.ImportedId));
                                long prevOkrId = prevKey == null ? 0 : prevKey.OkrId;
                                prevOkr = _OkrDataProvider.GetOkrObjectiveById(prevOkrId);
                            }
                            else
                            {
                                prevOkr = _OkrDataProvider.GetOkrObjectiveById(Convert.ToInt64(item.ImportedId));
                            }

                            if (prevOkr.EmployeeId == item.EmployeeId)///Imported Own OKR
                            {
                                string okrName = item.ImportedType == 2 ? prevKey.KeyDesc : prevOkr.OkrObjective;
                                string createrName = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;

                                ///For Subordinate at import time
                                List<long> toSubordinate = new List<long>();
                                string owername = userList.FirstOrDefault(x => x.employeeId == item.Owner) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.Owner).firstName;
                                NotificationsResponse notificationsRequestSubordinate = new NotificationsResponse();
                                //  string notificationTextForSubordinate = "Great work [#(Name)#], [#(MName)#] will surely appreciate you adopting his [#(OkrName)#] OKR!";
                                string notificationTextForSubordinate = _iconfiguration.GetValue<string>("Notifications:ImportedFromBacklog");
                                toSubordinate.Add(item.EmployeeId);
                                notificationsRequestSubordinate.to = toSubordinate;
                                notificationsRequestSubordinate.by = item.Owner;
                                notificationsRequestSubordinate.appId = 3;
                                notificationsRequestSubordinate.notificationType = 2;
                                notificationsRequestSubordinate.messageType = 2;
                                notificationsRequestSubordinate.url = "";
                                notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(Name)#]", createrName);
                                // notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(MName)#]", owername);
                                notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(OkrName)#]", prevKey.KeyDesc);
                                notificationsRequestSubordinate.text = notificationTextForSubordinate;
                                SaveNotification(notificationsRequestSubordinate);
                            }
                            else if (prevOkr.EmployeeId != item.EmployeeId)///Imported other OKR
                            {
                                List<long> toImportedEmployee = new List<long>();
                                string createrName = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;
                                NotificationsResponse notificationsRequest = new NotificationsResponse();
                                // string notificationText = "It's a great day. [#(Name)#] has picked up one of your OKRs '[#(Key)#]', you should expect some serious help there! ";
                                string notificationText = _iconfiguration.GetValue<string>("Notifications:MessageForOwnerAtImportTime");
                                toImportedEmployee.Add(item.Owner);
                                notificationsRequest.to = toImportedEmployee;
                                notificationsRequest.by = item.EmployeeId;
                                notificationsRequest.appId = 3;
                                notificationsRequest.notificationType = 2;
                                notificationsRequest.messageType = 1;
                                notificationsRequest.url = "okr-detail/" + okr.OkrId;
                                notificationText = notificationText.Replace("[#(Name)#]", createrName);
                                notificationText = notificationText.Replace("[#(Key)#]", item.OkrObjective);
                                notificationsRequest.text = notificationText;
                                SaveNotification(notificationsRequest);

                                ///For Subordinate at import time
                                List<long> toSubordinate = new List<long>();
                                string owername = userList.FirstOrDefault(x => x.employeeId == item.Owner) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.Owner).firstName;
                                NotificationsResponse notificationsRequestSubordinate = new NotificationsResponse();
                                //  string notificationTextForSubordinate = "Great work [#(Name)#], [#(MName)#] will surely appreciate you adopting his [#(OkrName)#] OKR!";
                                string notificationTextForSubordinate = _iconfiguration.GetValue<string>("Notifications:MessageForEmployee");
                                toSubordinate.Add(item.EmployeeId);
                                notificationsRequestSubordinate.to = toSubordinate;
                                notificationsRequestSubordinate.by = item.Owner;
                                notificationsRequestSubordinate.appId = 3;
                                notificationsRequestSubordinate.notificationType = 2;
                                notificationsRequestSubordinate.messageType = 1;
                                notificationsRequestSubordinate.url = "okr-detail/" + okr.OkrId;
                                notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(Name)#]", createrName);
                                notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(MName)#]", owername);
                                notificationTextForSubordinate = notificationTextForSubordinate.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationsRequestSubordinate.text = notificationTextForSubordinate;
                                SaveNotification(notificationsRequestSubordinate);
                            }

                        }

                    }
                    else if (item.OkrId != 0)
                    {
                        okr = _OkrDataProvider.GetOkrObjectiveById(item.OkrId);
                        okr.OkrObjective = item.OkrObjective;
                        okr.Status = item.Status;
                        okr.UpdatedOn = DateTime.UtcNow;
                        okr.Owner = item.Owner;
                        okr.Type = item.Type;
                        okr.OkrDescription = item.OkrDescription;
                        _OkrDataProvider.UpdateOkrObjective(okr);
                        foreach (var data in item.OkrDetailsList)
                        {
                            OkrDetails okrDetails = new OkrDetails();
                            if (data.KeyId == 0)
                            {
                                okrDetails.OkrId = okr.OkrId;
                                okrDetails.Status = data.Status;
                                okrDetails.KeyDesc = data.KeyDesc;
                                okrDetails.Score = data.Score;
                                okrDetails.IsNotify = data.IsNotify;
                                okrDetails.DueDate = data.DueDate;
                                _OkrDataProvider.SaveOkrKey(okrDetails);
                            }
                            else
                            {
                                okrDetails = _OkrDataProvider.GetOkrKeyById(data.KeyId);
                                okrDetails.KeyDesc = data.KeyDesc;
                                okrDetails.Status = data.Status;
                                okrDetails.UpdatedOn = DateTime.UtcNow;
                                okrDetails.Score = data.Score;
                                okrDetails.IsNotify = data.IsNotify;
                                okrDetails.DueDate = data.DueDate;
                                _OkrDataProvider.UpdateOkrKey(okrDetails);
                            }

                            //if (item.Status == 3)//Notification will create at the time of submit
                            //{
                            //    var Mdetails = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId);
                            //    long ReportingToId = Mdetails == null ? 0 : Mdetails.reportingTo;
                            //    string managerName = userList.FirstOrDefault(x => x.employeeId == ReportingToId) == null ? ""
                            //        : userList.FirstOrDefault(x => x.employeeId == ReportingToId).firstName;
                            //    string createrNameAtTimeOwn = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                            //            : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;
                            //    if (item.EmployeeId == item.Owner)
                            //    {
                            //        List<long> toSubmitByOwn = new List<long>();

                            //        NotificationsResponse notificationsRequestSubmitedByOwn = new NotificationsResponse();
                            //        //string notificationTextForOwn = "Hi <b>[#(Name)#]</b>! Objective <b>[#(Okr)#]</b> has been successfully submitted. You can track it by navigating to your Dashboard.";
                            //        string notificationTextForOwn = _iconfiguration.GetValue<string>("Notifications:CreaterAndOwnerSameAtTheTimeOfSubmit");
                            //        toSubmitByOwn.Add(item.Owner);
                            //        notificationsRequestSubmitedByOwn.to = toSubmitByOwn;
                            //        notificationsRequestSubmitedByOwn.by = item.EmployeeId;
                            //        notificationsRequestSubmitedByOwn.appId = 3;
                            //        notificationsRequestSubmitedByOwn.notificationType = 1;
                            //        notificationsRequestSubmitedByOwn.messageType = 2;
                            //        notificationsRequestSubmitedByOwn.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextForOwn = notificationTextForOwn.Replace("[#(Name)#]", createrNameAtTimeOwn);
                            //        notificationTextForOwn = notificationTextForOwn.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationsRequestSubmitedByOwn.text = notificationTextForOwn;
                            //        SaveNotification(notificationsRequestSubmitedByOwn);

                            //        ///Notification will deliver to supervisor 
                            //        List<long> toManager = new List<long>();
                            //        NotificationsResponse notificationsRequestForSupervisor = new NotificationsResponse();
                            //        // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                            //        string notificationTextForSupervisor = _iconfiguration.GetValue<string>("Notifications:NotifyToReportingTo");
                            //        toManager.Add(ReportingToId);
                            //        notificationsRequestForSupervisor.to = toManager;
                            //        notificationsRequestForSupervisor.by = item.EmployeeId;
                            //        notificationsRequestForSupervisor.appId = 3;
                            //        notificationsRequestForSupervisor.notificationType = 1;
                            //        notificationsRequestForSupervisor.messageType = 2;
                            //        notificationsRequestForSupervisor.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(MName)#]", managerName);
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Name)#]", createrNameAtTimeOwn);
                            //        notificationsRequestForSupervisor.text = notificationTextForSupervisor;
                            //        SaveNotification(notificationsRequestForSupervisor);
                            //    }
                            //    else if (item.Owner == ReportingToId)
                            //    {
                            //        List<long> toSubmitCreater = new List<long>();
                            //        NotificationsResponse notificationsRequestToCreater = new NotificationsResponse();
                            //        // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                            //        string notificationTextSubmitToCreater = _iconfiguration.GetValue<string>("Notifications:Submit");
                            //        toSubmitCreater.Add(item.EmployeeId);
                            //        notificationsRequestToCreater.to = toSubmitCreater;
                            //        notificationsRequestToCreater.by = item.EmployeeId;
                            //        notificationsRequestToCreater.appId = 3;
                            //        notificationsRequestToCreater.notificationType = 1;
                            //        notificationsRequestToCreater.messageType = 2;
                            //        notificationsRequestToCreater.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextSubmitToCreater = notificationTextSubmitToCreater.Replace("[#(Name)#]", createrNameAtTimeOwn);
                            //        notificationTextSubmitToCreater = notificationTextSubmitToCreater.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationsRequestToCreater.text = notificationTextSubmitToCreater;
                            //        SaveNotification(notificationsRequestToCreater);

                            //        List<long> toSubmitToPmWhoIsOwner = new List<long>();
                            //        NotificationsResponse notificationsRequestToPmWhoIsOwner = new NotificationsResponse();
                            //        // string notificationTextSubmitToOwner = "Hi [#(OwnerName)#], [#(Okr)#] has been successfully submitted by [#(Creater)#]. You can review them by navigating to to your Team Management";
                            //        string notificationTextSubmitToPmWhoIsOwner = _iconfiguration.GetValue<string>("Notifications:SubmitSupervisor");
                            //        toSubmitToPmWhoIsOwner.Add(item.Owner);
                            //        notificationsRequestToPmWhoIsOwner.to = toSubmitToPmWhoIsOwner;
                            //        notificationsRequestToPmWhoIsOwner.by = item.EmployeeId;
                            //        notificationsRequestToPmWhoIsOwner.appId = 3;
                            //        notificationsRequestToPmWhoIsOwner.notificationType = 1;
                            //        notificationsRequestToPmWhoIsOwner.messageType = 2;
                            //        notificationsRequestToPmWhoIsOwner.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(OwnerName)#]", managerName);
                            //        notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(Creater)#]", createrNameAtTimeOwn);
                            //        notificationsRequestToPmWhoIsOwner.text = notificationTextSubmitToPmWhoIsOwner;
                            //        SaveNotification(notificationsRequestToPmWhoIsOwner);
                            //    }
                            //    else if (item.Owner != ReportingToId)
                            //    {
                            //        List<long> toSubmit = new List<long>();
                            //        string createrName = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                            //            : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;
                            //        NotificationsResponse notificationsRequest = new NotificationsResponse();
                            //        // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                            //        string notificationTextSubmit = _iconfiguration.GetValue<string>("Notifications:Submit");
                            //        toSubmit.Add(item.EmployeeId);
                            //        notificationsRequest.to = toSubmit;
                            //        notificationsRequest.by = item.EmployeeId;
                            //        notificationsRequest.appId = 3;
                            //        notificationsRequest.notificationType = 1;
                            //        notificationsRequest.messageType = 2;
                            //        notificationsRequest.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextSubmit = notificationTextSubmit.Replace("[#(Name)#]", createrName);
                            //        notificationTextSubmit = notificationTextSubmit.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationsRequest.text = notificationTextSubmit;
                            //        SaveNotification(notificationsRequest);

                            //        ///Message for supervisor at the time of submittion of OKR
                            //        List<long> toSubmitToSupervisor = new List<long>();
                            //        string ownerName = userList.FirstOrDefault(x => x.employeeId == item.Owner) == null ? ""
                            //            : userList.FirstOrDefault(x => x.employeeId == item.Owner).firstName;
                            //        NotificationsResponse notificationsRequestSubmit = new NotificationsResponse();
                            //        // string notificationTextSubmitToOwner = "Hi [#(OwnerName)#], [#(Okr)#] has been successfully submitted by [#(Creater)#]. You can review them by navigating to to your Team Management";
                            //        string notificationTextSubmitToOwner = _iconfiguration.GetValue<string>("Notifications:SubmitSupervisor");
                            //        toSubmitToSupervisor.Add(item.Owner);
                            //        notificationsRequestSubmit.to = toSubmitToSupervisor;
                            //        notificationsRequestSubmit.by = item.EmployeeId;
                            //        notificationsRequestSubmit.appId = 3;
                            //        notificationsRequestSubmit.notificationType = 1;
                            //        notificationsRequestSubmit.messageType = 2;
                            //        notificationsRequestSubmit.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(OwnerName)#]", ownerName);
                            //        notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(Okr)#]", createrName);
                            //        notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(Creater)#]", item.OkrObjective);
                            //        notificationsRequestSubmit.text = notificationTextSubmitToOwner;
                            //        SaveNotification(notificationsRequestSubmit);

                            //        List<long> toManager = new List<long>();
                            //        NotificationsResponse notificationsRequestForSupervisor = new NotificationsResponse();
                            //        // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                            //        string notificationTextForSupervisor = _iconfiguration.GetValue<string>("Notifications:NotifyToReportingTo");
                            //        toManager.Add(ReportingToId);
                            //        notificationsRequestForSupervisor.to = toManager;
                            //        notificationsRequestForSupervisor.by = item.EmployeeId;
                            //        notificationsRequestForSupervisor.appId = 3;
                            //        notificationsRequestForSupervisor.notificationType = 1;
                            //        notificationsRequestForSupervisor.messageType = 2;
                            //        notificationsRequestForSupervisor.url = "okr-detail/" + okr.OkrId;
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(MName)#]", managerName);
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Okr)#]", item.OkrObjective);
                            //        notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Name)#]", createrNameAtTimeOwn);
                            //        notificationsRequestForSupervisor.text = notificationTextForSupervisor;
                            //        SaveNotification(notificationsRequestForSupervisor);
                            //    }


                            //}


                        }

                        if (item.Status == 3)//Notification will create at the time of submit
                        {
                            var Mdetails = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId);
                            long ReportingToId = Mdetails == null ? 0 : Mdetails.reportingTo;
                            string managerName = userList.FirstOrDefault(x => x.employeeId == ReportingToId) == null ? ""
                                : userList.FirstOrDefault(x => x.employeeId == ReportingToId).firstName;
                            string createrNameAtTimeOwn = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;
                            if (item.EmployeeId == item.Owner)
                            {
                                List<long> toSubmitByOwn = new List<long>();

                                NotificationsResponse notificationsRequestSubmitedByOwn = new NotificationsResponse();
                                //string notificationTextForOwn = "Hi <b>[#(Name)#]</b>! Objective <b>[#(Okr)#]</b> has been successfully submitted. You can track it by navigating to your Dashboard.";
                                string notificationTextForOwn = _iconfiguration.GetValue<string>("Notifications:CreaterAndOwnerSameAtTheTimeOfSubmit");
                                toSubmitByOwn.Add(item.Owner);
                                notificationsRequestSubmitedByOwn.to = toSubmitByOwn;
                                notificationsRequestSubmitedByOwn.by = item.EmployeeId;
                                notificationsRequestSubmitedByOwn.appId = 3;
                                notificationsRequestSubmitedByOwn.notificationType = 1;
                                notificationsRequestSubmitedByOwn.messageType = 2;
                                notificationsRequestSubmitedByOwn.url = "okr-detail/" + okr.OkrId;
                                notificationTextForOwn = notificationTextForOwn.Replace("[#(Name)#]", createrNameAtTimeOwn);
                                notificationTextForOwn = notificationTextForOwn.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationsRequestSubmitedByOwn.text = notificationTextForOwn;
                                SaveNotification(notificationsRequestSubmitedByOwn);

                                ///Notification will deliver to supervisor 
                                List<long> toManager = new List<long>();
                                NotificationsResponse notificationsRequestForSupervisor = new NotificationsResponse();
                                // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                                string notificationTextForSupervisor = _iconfiguration.GetValue<string>("Notifications:NotifyToReportingTo");
                                toManager.Add(ReportingToId);
                                notificationsRequestForSupervisor.to = toManager;
                                notificationsRequestForSupervisor.by = item.EmployeeId;
                                notificationsRequestForSupervisor.appId = 3;
                                notificationsRequestForSupervisor.notificationType = 1;
                                notificationsRequestForSupervisor.messageType = 2;
                                notificationsRequestForSupervisor.url = "okr-detail/" + okr.OkrId;
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(MName)#]", managerName);
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Name)#]", createrNameAtTimeOwn);
                                notificationsRequestForSupervisor.text = notificationTextForSupervisor;
                                SaveNotification(notificationsRequestForSupervisor);
                            }
                            else if (item.Owner == ReportingToId)
                            {
                                List<long> toSubmitCreater = new List<long>();
                                NotificationsResponse notificationsRequestToCreater = new NotificationsResponse();
                                // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                                string notificationTextSubmitToCreater = _iconfiguration.GetValue<string>("Notifications:Submit");
                                toSubmitCreater.Add(item.EmployeeId);
                                notificationsRequestToCreater.to = toSubmitCreater;
                                notificationsRequestToCreater.by = item.EmployeeId;
                                notificationsRequestToCreater.appId = 3;
                                notificationsRequestToCreater.notificationType = 1;
                                notificationsRequestToCreater.messageType = 2;
                                notificationsRequestToCreater.url = "okr-detail/" + okr.OkrId;
                                notificationTextSubmitToCreater = notificationTextSubmitToCreater.Replace("[#(Name)#]", createrNameAtTimeOwn);
                                notificationTextSubmitToCreater = notificationTextSubmitToCreater.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationsRequestToCreater.text = notificationTextSubmitToCreater;
                                SaveNotification(notificationsRequestToCreater);

                                List<long> toSubmitToPmWhoIsOwner = new List<long>();
                                NotificationsResponse notificationsRequestToPmWhoIsOwner = new NotificationsResponse();
                                // string notificationTextSubmitToOwner = "Hi [#(OwnerName)#], [#(Okr)#] has been successfully submitted by [#(Creater)#]. You can review them by navigating to to your Team Management";
                                string notificationTextSubmitToPmWhoIsOwner = _iconfiguration.GetValue<string>("Notifications:SubmitSupervisor");
                                toSubmitToPmWhoIsOwner.Add(item.Owner);
                                notificationsRequestToPmWhoIsOwner.to = toSubmitToPmWhoIsOwner;
                                notificationsRequestToPmWhoIsOwner.by = item.EmployeeId;
                                notificationsRequestToPmWhoIsOwner.appId = 3;
                                notificationsRequestToPmWhoIsOwner.notificationType = 1;
                                notificationsRequestToPmWhoIsOwner.messageType = 2;
                                notificationsRequestToPmWhoIsOwner.url = "okr-detail/" + okr.OkrId;
                                notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(OwnerName)#]", managerName);
                                notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationTextSubmitToPmWhoIsOwner = notificationTextSubmitToPmWhoIsOwner.Replace("[#(Creater)#]", createrNameAtTimeOwn);
                                notificationsRequestToPmWhoIsOwner.text = notificationTextSubmitToPmWhoIsOwner;
                                SaveNotification(notificationsRequestToPmWhoIsOwner);
                            }
                            else if (item.Owner != ReportingToId)
                            {
                                List<long> toSubmit = new List<long>();
                                string createrName = userList.FirstOrDefault(x => x.employeeId == item.EmployeeId) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.EmployeeId).firstName;
                                NotificationsResponse notificationsRequest = new NotificationsResponse();
                                // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                                string notificationTextSubmit = _iconfiguration.GetValue<string>("Notifications:Submit");
                                toSubmit.Add(item.EmployeeId);
                                notificationsRequest.to = toSubmit;
                                notificationsRequest.by = item.EmployeeId;
                                notificationsRequest.appId = 3;
                                notificationsRequest.notificationType = 1;
                                notificationsRequest.messageType = 2;
                                notificationsRequest.url = "okr-detail/" + okr.OkrId;
                                notificationTextSubmit = notificationTextSubmit.Replace("[#(Name)#]", createrName);
                                notificationTextSubmit = notificationTextSubmit.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationsRequest.text = notificationTextSubmit;
                                SaveNotification(notificationsRequest);

                                ///Message for supervisor at the time of submittion of OKR
                                List<long> toSubmitToSupervisor = new List<long>();
                                string ownerName = userList.FirstOrDefault(x => x.employeeId == item.Owner) == null ? ""
                                    : userList.FirstOrDefault(x => x.employeeId == item.Owner).firstName;
                                NotificationsResponse notificationsRequestSubmit = new NotificationsResponse();
                                // string notificationTextSubmitToOwner = "Hi [#(OwnerName)#], [#(Okr)#] has been successfully submitted by [#(Creater)#]. You can review them by navigating to to your Team Management";
                                string notificationTextSubmitToOwner = _iconfiguration.GetValue<string>("Notifications:SubmitSupervisor");
                                toSubmitToSupervisor.Add(item.Owner);
                                notificationsRequestSubmit.to = toSubmitToSupervisor;
                                notificationsRequestSubmit.by = item.EmployeeId;
                                notificationsRequestSubmit.appId = 3;
                                notificationsRequestSubmit.notificationType = 1;
                                notificationsRequestSubmit.messageType = 2;
                                notificationsRequestSubmit.url = "okr-detail/" + okr.OkrId;
                                notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(OwnerName)#]", ownerName);
                                notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(Okr)#]", createrName);
                                notificationTextSubmitToOwner = notificationTextSubmitToOwner.Replace("[#(Creater)#]", item.OkrObjective);
                                notificationsRequestSubmit.text = notificationTextSubmitToOwner;
                                SaveNotification(notificationsRequestSubmit);

                                List<long> toManager = new List<long>();
                                NotificationsResponse notificationsRequestForSupervisor = new NotificationsResponse();
                                // string notificationTextSubmit = "Awesome work [#(Name)#], your OKRs have all been submitted. The team is all aligned.";
                                string notificationTextForSupervisor = _iconfiguration.GetValue<string>("Notifications:NotifyToReportingTo");
                                toManager.Add(ReportingToId);
                                notificationsRequestForSupervisor.to = toManager;
                                notificationsRequestForSupervisor.by = item.EmployeeId;
                                notificationsRequestForSupervisor.appId = 3;
                                notificationsRequestForSupervisor.notificationType = 1;
                                notificationsRequestForSupervisor.messageType = 2;
                                notificationsRequestForSupervisor.url = "okr-detail/" + okr.OkrId;
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(MName)#]", managerName);
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Okr)#]", item.OkrObjective);
                                notificationTextForSupervisor = notificationTextForSupervisor.Replace("[#(Name)#]", createrNameAtTimeOwn);
                                notificationsRequestForSupervisor.text = notificationTextForSupervisor;
                                SaveNotification(notificationsRequestForSupervisor);
                            }


                        }

                    }
                    saveOkrResponce.okrId = okr.OkrId;
                    SaveOkrResponceList.Add(saveOkrResponce);
                }
                var resultObj = okrRequestList.FirstOrDefault();
                _OkrDataProvider.CalculateScore(resultObj.Year, resultObj.Quarter, resultObj.EmployeeId);


            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "GetStatusMasters", "OkrService", ex.ToString());
            }
            return SaveOkrResponceList;
        }
        public List<OkrRequest> MyOkr(long userId, int quarter, int year, string jwtToken)
        {
            List<OkrRequest> okrRequests = new List<OkrRequest>();
            List<Okr> okrs = new List<Okr>();
            List<ProgressionMaster> progressionMasters = new List<ProgressionMaster>();
            List<OkrObjectiveScoreResponse> okrObjectiveScoreResponses = new List<OkrObjectiveScoreResponse>();

            okrs = _OkrDataProvider.GetOkrObjByYear(userId, quarter, year);
            okrObjectiveScoreResponses = _OkrDataProvider.GetObjectiveScore(year, quarter, userId);
            progressionMasters = _OkrDataProvider.GetProgression(year, quarter);
            var userList = GetAllUserFromUserManagement(jwtToken);

            if (okrs != null)
            {
                foreach (var item in okrs)
                {
                    OkrRequest okrRequest = new OkrRequest();
                    okrRequest.Year = item.Year;
                    okrRequest.Quarter = item.Quarter;
                    okrRequest.EmployeeId = item.EmployeeId;
                    okrRequest.OkrId = item.OkrId;
                    okrRequest.OkrObjective = item.OkrObjective;
                    okrRequest.Type = item.Type;
                    okrRequest.IsImported = item.IsImported;
                    okrRequest.ImportedId = item.ImportedId;
                    okrRequest.Status = item.Status;
                    okrRequest.Owner = item.Owner;
                    var ownerUser = userList.FirstOrDefault(x => x.employeeId == item.Owner);
                    okrRequest.OwnerName = ownerUser == null ? " " : ownerUser.firstName + " " + ownerUser.lastName;
                    okrRequest.OkrDescription = item.OkrDescription;
                    okrRequest.CreatedOn = item.CreatedOn;
                    okrRequest.UpdatedOn = item.UpdatedOn == null ? item.CreatedOn : item.UpdatedOn;
                    var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                    List<OkrDetailReq> okrDetails = new List<OkrDetailReq>();
                    foreach (var data in okrDetail)
                    {
                        OkrDetailReq okrRecord = new OkrDetailReq();
                        okrRecord.KeyId = data.KeyId;
                        okrRecord.KeyDesc = data.KeyDesc;
                        okrRecord.Status = data.Status;
                        okrRecord.Score = data.Score;
                        okrRecord.IsNotify = data.IsNotify;
                        okrRecord.DueDate = data.DueDate;
                        okrRecord.CreatedOn = data.CreatedOn;
                        okrRecord.UpdatedOn = data.UpdatedOn == null ? data.CreatedOn : data.UpdatedOn;
                        var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                        okrRecord.ProgressionPercentage = record == null ? 0.00M : record.ProgressPercentage;
                        okrRecord.ProgressionName = record == null ? "" : record.ProgressName;
                        okrDetails.Add(okrRecord);

                    }
                    okrRequest.ProgressionScore
                         = (okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId)) == null
                         ? "0.00" : okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId).Score.ToString("0.00");


                    okrRequest.OkrDetailsList = okrDetails;
                    okrRequests.Add(okrRequest);

                }

            }

            return okrRequests;

        }
        public List<StatusMaster> GetStatusMasters()
        {
            List<StatusMaster> statusMasters = new List<StatusMaster>();
            try
            {
                statusMasters = _OkrDataProvider.GetStatusMasters();
            }
            catch (Exception e)
            {
                SaveLog("OkrServices", "GetStatusMasters", "OkrService", e.ToString());
            }

            return statusMasters;
        }
        public string ResetOkr(long employeeId, int year, int quarter, long okrId, long loginEmp, long resetType)
        {
            string reset = string.Empty;
            try
            {

                reset = _OkrDataProvider.ResetOkr(employeeId, year, quarter, okrId, loginEmp, resetType);
            }
            catch (Exception e)
            {
                SaveLog("OkrServices", "ResetOkr", "OkrService", e.ToString());
            }

            return reset;
        }
        public List<QuarterSummaryResponse> QuarterSummary(int quarter, int year, string jwtToken)
        {
            List<QuarterSummaryResponse> quarterSummaryResponses = new List<QuarterSummaryResponse>();
            bool isExternalApi = true;
            try
            {
                var qtrOkr = _OkrDataProvider.QuarterSummary(quarter, year);
                if (isExternalApi)
                {
                    var userDetailPassport = GetAllUserFromPassport();
                    quarterSummaryResponses = (from q in qtrOkr
                                               join u in userDetailPassport on q.EmployeeId equals u.EmployeeId
                                               select new QuarterSummaryResponse()
                                               {
                                                   EmployeeId = u.EmployeeId,
                                                   EmployeeName = u.FirstName + " " + u.LastName,
                                                   Designation = u.DesignationName,
                                                   ReportingTo = u.ReportingToName,
                                                   Group = u.DivisionName,
                                                   Year = q.Year,
                                                   Quarter = q.Quarter,
                                                   OKRType = q.OKRType,
                                                   OKRObjective = q.OKRObjective,
                                                   KeyResult = q.KeyResult,
                                                   Score = q.Score
                                               }).ToList();
                }
                else
                {
                    var userDetailPassport = GetAllUserFromUserManagement(jwtToken);
                    quarterSummaryResponses = (from q in qtrOkr
                                               join u in userDetailPassport on q.EmployeeId equals u.employeeId
                                               select new QuarterSummaryResponse()
                                               {
                                                   EmployeeId = u.employeeId,
                                                   EmployeeName = u.firstName + " " + u.lastName,
                                                   Designation = "",
                                                   ReportingTo = "",
                                                   Group = "",
                                                   Year = q.Year,
                                                   Quarter = q.Quarter,
                                                   OKRType = q.OKRType,
                                                   OKRObjective = q.OKRObjective,
                                                   KeyResult = q.KeyResult,
                                                   Score = q.Score
                                               }).ToList();
                }
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "QuarterSummary", "OkrService", ex.ToString());
            }
            return quarterSummaryResponses;

        }
        public List<OkrStatusResponse> OkrStatusReport(int quarter, int year, string jwtToken)
        {
            List<OkrStatusResponse> OkrStatusResponses = new List<OkrStatusResponse>();
            bool isExternalApi = true;
            try
            {
                var qtrOkr = _OkrDataProvider.OkrStatusReport(quarter, year);
                if (isExternalApi)
                {
                    var userDetailPassport = GetAllUserFromPassport();
                    foreach (var u in userDetailPassport)
                    {
                        var userOkr = qtrOkr.Where(x => x.EmployeeId == u.EmployeeId).ToList();
                        string status = "NotStarted";
                        status = userOkr.Count() == 0 ? "NotStarted" : userOkr.Count(x => x.Status == "1") > 0 ? "Created" : "Submmitted";
                        OkrStatusResponse okrStatusResponse = new OkrStatusResponse();
                        okrStatusResponse.EmployeeId = u.EmployeeId;
                        okrStatusResponse.EmployeeName = u.FirstName + " " + u.LastName;
                        okrStatusResponse.Business = u.FunctionName;
                        okrStatusResponse.LobName = u.LOBName;
                        okrStatusResponse.Division = u.DivisionName;
                        okrStatusResponse.RoleName = u.CompetencyName;
                        okrStatusResponse.ContractFrom = u.DateOfJoining;
                        okrStatusResponse.MailId = u.MailId;
                        okrStatusResponse.SupervisorId = u.ReportingToName;
                        okrStatusResponse.Status = status;

                        OkrStatusResponses.Add(okrStatusResponse);
                    }

                }
                else
                {
                    var userDetailPassport = GetAllUserFromUserManagement(jwtToken);
                    OkrStatusResponses = (from q in qtrOkr
                                          join u in userDetailPassport on q.EmployeeId equals u.employeeId
                                          select new OkrStatusResponse()
                                          {
                                              EmployeeId = u.employeeId,
                                              EmployeeName = u.firstName + " " + u.lastName,
                                              Business = "",
                                              LobName = "",
                                              Division = "",
                                              RoleName = "",
                                              ContractFrom = "",
                                              MailId = "",
                                              SupervisorId = q.SupervisorId,
                                              Status = q.Status,
                                          }).ToList();
                }
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "OkrStatusReport", "OkrService", ex.ToString());
            }
            return OkrStatusResponses;

        }
        public List<OkrProgressResponse> OkrProgressReport(int quarter, int year, string jwtToken)
        {
            List<OkrProgressResponse> OkrProgressResponses = new List<OkrProgressResponse>();
            bool isExternalApi = true;
            try
            {
                var qtrOkr = _OkrDataProvider.OkrProgressReport(quarter, year);
                if (isExternalApi)
                {
                    var userDetailPassport = GetAllUserFromPassport().Where(x => x.IsActive = true);
                    foreach (var u in userDetailPassport)
                    {
                        var userOkr = qtrOkr.FirstOrDefault(x => x.EmployeeId == u.EmployeeId);
                        OkrProgressResponse okrProgressResponse = new OkrProgressResponse();
                        okrProgressResponse.EmployeeId = u.EmployeeId;
                        okrProgressResponse.EmployeeName = u.FirstName + " " + u.LastName;
                        okrProgressResponse.AverageScore = userOkr == null ? "0.00" : userOkr.AverageScore;
                        okrProgressResponse.AvgAspirationalScore = userOkr == null ? "0.00" : userOkr.AvgAspirationalScore;
                        okrProgressResponse.AvgOperationalScore = userOkr == null ? "0.00" : userOkr.AvgOperationalScore;
                        okrProgressResponse.LobName = u.LOBName;
                        OkrProgressResponses.Add(okrProgressResponse);
                    }

                    //    OkrProgressResponses = (from q in qtrOkr
                    //                            join u in userDetailPassport on q.EmployeeId equals u.EmployeeId
                    //                            select new OkrProgressResponse()
                    //                            {
                    //                                EmployeeId = u.EmployeeId,
                    //                                EmployeeName = u.FirstName + " " + u.LastName,
                    //                                AverageScore = q.AverageScore,
                    //                                AvgAspirationalScore = q.AvgAspirationalScore,
                    //                                AvgOperationalScore = q.AvgOperationalScore,
                    //                                LobName = u.LOBName,
                    //                            }).ToList();
                }
                else
                {
                    var userDetailPassport = GetAllUserFromUserManagement(jwtToken);
                    OkrProgressResponses = (from q in qtrOkr
                                            join u in userDetailPassport on q.EmployeeId equals u.employeeId
                                            select new OkrProgressResponse()
                                            {
                                                EmployeeId = u.employeeId,
                                                EmployeeName = u.firstName + " " + u.lastName,
                                                AverageScore = q.AverageScore,
                                                AvgAspirationalScore = q.AvgAspirationalScore,
                                                AvgOperationalScore = q.AvgOperationalScore,
                                                LobName = "",
                                            }).ToList();
                }


            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "OkrProgressReport", "OkrService", ex.ToString());
            }
            return OkrProgressResponses;
        }
        public ImportOkr ImportOkr(long reportingId, int year, int qtr, string jwtToken)
        {
            ImportOkr importOkr = new ImportOkr();
            try
            {
                importOkr.ReportingOkr = MyOkr(reportingId, qtr, year, jwtToken);
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "ImportOkr", "OkrService", ex.ToString());
            }

            return importOkr;

        }
        public ImportEmployeeOkr ImportEmployeeOkr(long EmployeeId, int Year, int qtr, string jwtToken)
        {
            ImportEmployeeOkr importEmployeeOkr = new ImportEmployeeOkr();
            List<ImportPeriod> importPeriods = new List<ImportPeriod>();
            ImportPeriod importPeriod1 = new ImportPeriod();
            ImportPeriod importPeriod2 = new ImportPeriod();
            ImportPeriod importPeriod3 = new ImportPeriod();
            int prevQtrOld1 = 0;
            int prevQtrOld2 = 0;
            int prevQtrOld3 = 0;
            int prevYearOld = 0;
            int prevYearNew = 0;
            try
            {
                if (qtr == 1)
                {
                    prevQtrOld1 = 4;
                    prevQtrOld2 = 3;
                    prevQtrOld3 = 2;
                    prevYearOld = Year - 1;

                    importPeriod1.Quarter = prevQtrOld1;
                    importPeriod1.Year = prevYearOld;
                    importPeriod2.Quarter = prevQtrOld2;
                    importPeriod2.Year = prevYearOld;
                    importPeriod3.Quarter = prevQtrOld3;
                    importPeriod3.Year = prevYearOld;

                    importPeriods.Add(importPeriod1);
                    importPeriods.Add(importPeriod2);
                    importPeriods.Add(importPeriod3);
                    importEmployeeOkr.ImportPeriodList = importPeriods;
                    importEmployeeOkr.ImportPRV1 = MyScoreOkr(EmployeeId, prevQtrOld1, prevYearOld, jwtToken);
                    importEmployeeOkr.ImportPRV2 = MyScoreOkr(EmployeeId, prevQtrOld2, prevYearOld, jwtToken);
                    importEmployeeOkr.ImportPRV3 = MyScoreOkr(EmployeeId, prevQtrOld3, prevYearOld, jwtToken);
                }
                if (qtr == 2)
                {
                    prevQtrOld1 = 4;
                    prevQtrOld2 = 3;
                    prevYearOld = Year - 1;
                    prevQtrOld3 = 1;
                    prevYearNew = Year;

                    importPeriod1.Quarter = prevQtrOld3;
                    importPeriod1.Year = prevYearNew;
                    importPeriod2.Quarter = prevQtrOld1;
                    importPeriod2.Year = prevYearOld;
                    importPeriod3.Quarter = prevQtrOld2;
                    importPeriod3.Year = prevYearOld;

                    importPeriods.Add(importPeriod1);
                    importPeriods.Add(importPeriod2);
                    importPeriods.Add(importPeriod3);

                    importEmployeeOkr.ImportPeriodList = importPeriods;
                    importEmployeeOkr.ImportPRV1 = MyScoreOkr(EmployeeId, prevQtrOld3, prevYearNew, jwtToken);
                    importEmployeeOkr.ImportPRV2 = MyScoreOkr(EmployeeId, prevQtrOld1, prevYearOld, jwtToken);
                    importEmployeeOkr.ImportPRV3 = MyScoreOkr(EmployeeId, prevQtrOld2, prevYearOld, jwtToken);

                }
                if (qtr == 3)
                {
                    prevQtrOld1 = 4;
                    prevQtrOld2 = 2;
                    prevQtrOld3 = 1;
                    prevYearOld = Year - 1;
                    prevYearNew = Year;

                    importPeriod1.Quarter = prevQtrOld3;
                    importPeriod1.Year = prevYearNew;
                    importPeriod2.Quarter = prevQtrOld2;
                    importPeriod2.Year = prevYearNew;
                    importPeriod3.Quarter = prevQtrOld1;
                    importPeriod3.Year = prevYearOld;

                    importPeriods.Add(importPeriod1);
                    importPeriods.Add(importPeriod2);
                    importPeriods.Add(importPeriod3);

                    importEmployeeOkr.ImportPeriodList = importPeriods;
                    importEmployeeOkr.ImportPRV1 = MyScoreOkr(EmployeeId, prevQtrOld3, prevYearNew, jwtToken);
                    importEmployeeOkr.ImportPRV2 = MyScoreOkr(EmployeeId, prevQtrOld2, prevYearNew, jwtToken);
                    importEmployeeOkr.ImportPRV3 = MyScoreOkr(EmployeeId, prevQtrOld1, prevYearOld, jwtToken);
                }
                if (qtr == 4)
                {
                    prevQtrOld1 = 1;
                    prevQtrOld2 = 2;
                    prevQtrOld3 = 3;
                    prevYearOld = Year;


                    importPeriod1.Quarter = prevQtrOld1;
                    importPeriod1.Year = prevYearOld;
                    importPeriod2.Quarter = prevQtrOld2;
                    importPeriod2.Year = prevYearOld;
                    importPeriod3.Quarter = prevQtrOld3;
                    importPeriod3.Year = prevYearOld;

                    importPeriods.Add(importPeriod1);
                    importPeriods.Add(importPeriod2);
                    importPeriods.Add(importPeriod3);

                    importEmployeeOkr.ImportPeriodList = importPeriods;
                    importEmployeeOkr.ImportPRV1 = MyScoreOkr(EmployeeId, prevQtrOld1, prevYearOld, jwtToken);
                    importEmployeeOkr.ImportPRV2 = MyScoreOkr(EmployeeId, prevQtrOld2, prevYearOld, jwtToken);
                    importEmployeeOkr.ImportPRV3 = MyScoreOkr(EmployeeId, prevQtrOld3, prevYearOld, jwtToken);
                }



            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "ImportEmployeeOkr", "OkrService", ex.ToString());
            }

            return importEmployeeOkr;


        }
        public List<OkrSettingsForProgression> OkrSettingsForProgressions()
        {
            List<OkrSettingsForProgression> okrSettingsForProgressions = new List<OkrSettingsForProgression>();
            try
            {
                okrSettingsForProgressions = _OkrDataProvider.OkrSettingsForProgressions();
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "OkrSettingsForProgressions", "OkrService", ex.ToString());
            }
            return okrSettingsForProgressions;
        }
        public List<OkrRequest> MyScoreOkr(long userId, int quarter, int year, string jwtToken)
        {
            List<OkrRequest> okrRequests = new List<OkrRequest>();
            List<Okr> okrs = new List<Okr>();
            List<ProgressionMaster> progressionMasters = new List<ProgressionMaster>();
            var userList = GetAllUserFromUserManagement(jwtToken);

            try
            {
                okrs = _OkrDataProvider.GetOkrObjByYear(userId, quarter, year);
                progressionMasters = _OkrDataProvider.GetProgression(year, quarter);
                decimal meritScore = _OkrDataProvider.ConstantValue();
                if (okrs != null)
                {
                    foreach (var item in okrs)
                    {
                        OkrRequest okrRequest = new OkrRequest();
                        okrRequest.Year = item.Year;
                        okrRequest.Quarter = item.Quarter;
                        okrRequest.EmployeeId = item.EmployeeId;
                        okrRequest.OkrId = item.OkrId;
                        okrRequest.OkrObjective = item.OkrObjective;
                        okrRequest.Type = item.Type;
                        okrRequest.IsImported = item.IsImported;
                        okrRequest.ImportedId = item.ImportedId;
                        okrRequest.Status = item.Status;
                        okrRequest.Owner = item.Owner;
                        var ownerUser = userList.FirstOrDefault(x => x.employeeId == item.Owner);
                        okrRequest.OwnerName = ownerUser == null ? " " : ownerUser.firstName + " " + ownerUser.lastName;
                        // okrRequest.IsNotify = item.IsNotify;
                        //okrRequest.NotificationDate = item.NotificationDate;
                        okrRequest.OkrDescription = item.OkrDescription;
                        okrRequest.CreatedOn = item.CreatedOn;
                        okrRequest.UpdatedOn = item.UpdatedOn == null ? item.CreatedOn : item.UpdatedOn;
                        var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                        List<OkrDetailReq> okrDetails = new List<OkrDetailReq>();
                        foreach (var data in okrDetail)
                        {

                            OkrDetailReq okrRecord = new OkrDetailReq();
                            var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                            okrRecord.ProgressionPercentage = record == null ? 0.00M : record.ProgressPercentage;

                            if (okrRecord.ProgressionPercentage <= meritScore)
                            {
                                okrRecord.KeyId = data.KeyId;
                                okrRecord.KeyDesc = data.KeyDesc;
                                okrRecord.Status = data.Status;
                                okrRecord.Score = data.Score;
                                okrRecord.CreatedOn = data.CreatedOn;
                                okrRecord.UpdatedOn = data.UpdatedOn == null ? data.CreatedOn : data.UpdatedOn;
                                okrRecord.ProgressionName = record == null ? "" : record.ProgressName;
                                okrDetails.Add(okrRecord);
                            }

                        }
                        if (okrDetails.Count > 0)
                        {

                            okrRequest.OkrDetailsList = okrDetails;
                            okrRequests.Add(okrRequest);
                        }



                    }
                }
            }
            catch (Exception ex)
            {

            }
            return okrRequests;

        }
        public CommentResponce SaveComments(CommentRequest commentRequest, string jwtToken)
        {
            Comments comments = new Comments();
            CommentResponce commentResponce = new CommentResponce();
            NotificationsResponse notificationsResponse = new NotificationsResponse();
            List<long> to = new List<long>();
            var userList = GetAllUserFromUserManagement(jwtToken);
            OkrDetails keyDetail = new OkrDetails();
            Okr okr = new Okr();
            string goalType = "OKR";
            try
            {
                if (commentRequest.CommentId > 0)
                {
                    comments.Comment = commentRequest.Comment;
                    comments.Status = commentRequest.Status;
                    comments.UpdatedBy = commentRequest.CreatedBy;
                    comments.UpdatedOn = DateTime.UtcNow;
                    comments = _OkrDataProvider.UpdateComments(comments);
                }
                else
                {
                    comments.Comment = commentRequest.Comment;
                    comments.Type = commentRequest.Type;
                    comments.TypeId = commentRequest.TypeId;
                    comments.CreatedBy = commentRequest.CreatedBy;
                    comments = _OkrDataProvider.SaveComments(comments);


                }

                commentResponce = _mapper.Map<CommentResponce>(comments);

                var commentedUser = userList.FirstOrDefault(x => x.employeeId == comments.CreatedBy);
                //long reportingto = commentedUser == null ? 0 : commentedUser.reportingTo;
                //string reportingName = commentedUser == null ? " " : commentedUser.reportingName;

                commentResponce.CreatedByName = commentedUser == null ? " " : commentedUser.firstName + " " + commentedUser.lastName;
                var commentedDetails = _OkrDataProvider.GetCommentedDetails(commentRequest.Type, commentRequest.TypeId);

                if (commentedDetails.CreatedEmployee == commentRequest.CreatedBy)///Notification will be created for owner to
                {
                    var ownerDetails = userList.FirstOrDefault(x => x.employeeId == commentedDetails.Owner);
                    string ownerName = ownerDetails == null ? "" : ownerDetails.firstName;
                    to.Add(Convert.ToInt64(commentedDetails.Owner));
                    notificationsResponse.by = commentRequest.CreatedBy;
                    goalType = commentRequest.Type == 1 ? "OKR" : "KEY";
                    notificationsResponse.text = " Hi <b>" + ownerName + "</b>, " + commentedUser == null ? " " : " <b>" + commentedUser.firstName + " </b> has commented on your " + goalType + " <b>'" + commentedDetails.Goal + "'</b>. You may want to review and respond to it. ";
                }
                else ///Notification will be created for okr creater
                {
                    var okrCreatedUser = userList.FirstOrDefault(x => x.employeeId == commentedDetails.CreatedEmployee);
                    to.Add(Convert.ToInt64(commentedDetails.CreatedEmployee));
                    notificationsResponse.by = commentRequest.CreatedBy;
                    notificationsResponse.text = " Hi <b>" + okrCreatedUser.firstName + "</b>, <b>"
                    + commentedUser.firstName + "</b>  has commented on your " + goalType + " <b>'" + commentedDetails.Goal
                    + "'</b>. You may want to review and respond to it. ";
                }

                notificationsResponse.to = to;
                notificationsResponse.url = "okr-detail/" + commentedDetails.Okrid;
                notificationsResponse.appId = 3;
                notificationsResponse.notificationType = 3;
                notificationsResponse.messageType = 1;
                SaveNotification(notificationsResponse);

            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "OkrSettingsForProgressions", "OkrService", ex.ToString());
            }
            return commentResponce;
        }
        public string ReadComments(long employeeId, int type, long typeId, string jwtToken)
        {
            string result = string.Empty;
            try
            {
                result = _OkrDataProvider.ReadComments(employeeId, type, typeId);
            }
            catch (Exception ex)
            {
                SaveLog("OkrServices", "OkrSettingsForProgressions", "OkrService", ex.ToString());
            }
            return result;
        }
        public List<CommentResponce> GetComments(int type, long typeId, string jwtToken)
        {
            List<CommentResponce> comments = new List<CommentResponce>();

            var userList = GetAllUserFromUserManagement(jwtToken);

            var commentRecord = _OkrDataProvider.GetComments(type, typeId);
            comments = (from u in userList
                        join c in commentRecord on u.employeeId equals c.CreatedBy
                        select new CommentResponce
                        {
                            CommentId = c.CommentId,
                            Comment = c.Comment,
                            Type = c.Type,
                            TypeId = c.TypeId,
                            CreatedOn = c.CreatedOn,
                            CreatedBy = c.CreatedBy,
                            CreatedByName = u.firstName + " " + u.lastName,
                            IsRead = c.IsRead,
                            Status = c.Status
                        }).OrderByDescending(c => c.CommentId).ThenByDescending(c => c.CreatedOn).ToList();

            return comments;
        }
        public Identity GetIdentity(string jwtToken)
        {
            Identity loginUserDetail = new Identity();
            if (jwtToken != "")
            {
                try
                {
                    HttpWebRequest passportRequest = GetHttpWebRequest(_iconfiguration.GetValue<string>("UserManagement:URL"), "POST", null, null, jwtToken);
                    using (HttpWebResponse response = (HttpWebResponse)passportRequest.GetResponse())
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        var result = reader.ReadToEnd();
                        var UserManagement = JsonConvert.DeserializeObject<PayloadCustom<Identity>>(result);
                        loginUserDetail = UserManagement.Entity;

                    }
                }
                catch (Exception ex)
                {
                    SaveLog("OkrServices", "GetIdentity", "OkrService", ex.ToString());
                    return null;
                }

            }
            return loginUserDetail;
        }
        public List<OkrRequest> OkrByOkrId(long OkrId, string jwtToken, Identity identity)
        {
            List<OkrRequest> okrRequests = new List<OkrRequest>();
            List<Okr> okrs = new List<Okr>();
            List<Comments> unreadObjComments = new List<Comments>();
            var userList = GetAllUserFromUserManagement(jwtToken);
            List<ProgressionMaster> progressionMasters = new List<ProgressionMaster>();
            List<OkrObjectiveScoreResponse> okrObjectiveScoreResponses = new List<OkrObjectiveScoreResponse>();
            List<Comments> unreadComments = new List<Comments>();

            okrs = _OkrDataProvider.OkrbyOkrid(OkrId);//.Where(x => x.Owner == identity.EmployeeId || x.EmployeeId == identity.EmployeeId).ToList();

            if (userList != null && okrs != null)
            {
                var userDetails = userList.FirstOrDefault(x => x.employeeId == okrs.FirstOrDefault().EmployeeId);
                if (userDetails.reportingTo == identity.EmployeeId)
                {

                }
                else if (identity.EmployeeId == okrs.FirstOrDefault().EmployeeId)
                {

                }
                else
                {
                    okrs = _OkrDataProvider.OkrbyOkrid(OkrId).Where(x => x.Owner == identity.EmployeeId || x.EmployeeId != identity.EmployeeId).ToList();
                }
            }


            if (okrs != null)
            {

                unreadComments = _OkrDataProvider.GetUnreadComments(2);
                unreadObjComments = _OkrDataProvider.GetUnreadComments(1);
                foreach (var item in okrs)
                {
                    OkrRequest okrRequest = new OkrRequest();

                    if (unreadObjComments == null)
                    {
                        okrRequest.ObjUnreadComment = 0;
                    }
                    else
                    {
                        okrRequest.ObjUnreadComment = unreadObjComments.Count(x => x.TypeId == item.OkrId && x.CreatedBy != identity.EmployeeId);
                    }


                    okrRequest.Year = item.Year;
                    okrRequest.Quarter = item.Quarter;
                    okrRequest.EmployeeId = item.EmployeeId;
                    okrRequest.OkrId = item.OkrId;
                    okrRequest.OkrObjective = item.OkrObjective;
                    okrRequest.Type = item.Type;
                    okrRequest.IsImported = item.IsImported;
                    okrRequest.ImportedId = item.ImportedId;
                    okrRequest.Status = item.Status;
                    okrRequest.Owner = item.Owner;
                    var ownerUser = userList.FirstOrDefault(x => x.employeeId == item.Owner);
                    okrRequest.OwnerName = ownerUser == null ? " " : ownerUser.firstName + " " + ownerUser.lastName;
                    okrRequest.OkrDescription = item.OkrDescription;
                    okrRequest.CreatedOn = item.CreatedOn;
                    okrRequest.UpdatedOn = item.UpdatedOn == null ? item.CreatedOn : item.UpdatedOn;
                    var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                    List<OkrDetailReq> okrDetails = new List<OkrDetailReq>();
                    foreach (var data in okrDetail)
                    {
                        OkrDetailReq okrRecord = new OkrDetailReq();
                        if (unreadComments == null)
                        {
                            okrRecord.KeyUnreadComment = 0;
                        }
                        else
                        {
                            okrRecord.KeyUnreadComment = unreadComments.Count(x => x.TypeId == data.KeyId && x.CreatedBy != identity.EmployeeId);
                        }
                        okrRecord.KeyId = data.KeyId;
                        okrRecord.KeyDesc = data.KeyDesc;
                        okrRecord.Status = data.Status;
                        okrRecord.Score = data.Score;
                        okrRecord.CreatedOn = data.CreatedOn;
                        okrRecord.UpdatedOn = data.UpdatedOn == null ? data.CreatedOn : data.UpdatedOn;
                        okrRecord.DueDate = data.DueDate;
                        okrRecord.IsNotify = data.IsNotify;
                        progressionMasters = _OkrDataProvider.GetProgression(item.Year, item.Quarter);
                        var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                        okrRecord.ProgressionPercentage = record == null ? 0.00M : record.ProgressPercentage;
                        okrRecord.ProgressionName = record == null ? "" : record.ProgressName;
                        okrRecord.ProgressionValue = record == null ? 0 : record.ProgressValue;
                        okrDetails.Add(okrRecord);

                    }
                    okrObjectiveScoreResponses = _OkrDataProvider.GetObjectiveScore(item.Year, item.Quarter, item.EmployeeId);
                    okrRequest.ProgressionScore
                         = (okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId)) == null
                         ? "0.00" : okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId).Score.ToString("0.00");


                    okrRequest.OkrDetailsList = okrDetails;
                    okrRequests.Add(okrRequest);

                }

            }

            return okrRequests;
        }
        public string DeleteOkr(long okrId, long keyId, string jwtToken)
        {
            string result = "";
            result = _OkrDataProvider.DeleteOkr(okrId, keyId);
            return result;
        }
        public string DeleteComments(long CommentsId)
        {
            string result = "";
            result = _OkrDataProvider.DeleteComments(CommentsId);
            return result;
        }
        public Dashboard Dashboard(long empId, int quarter, int year, string jwtToken, Identity identity)
        {
            Dashboard dashboard = new Dashboard();
            bool isSuperviserWantToSee = false;
            List<OkrRequest> okrRequests = new List<OkrRequest>();
            List<Okr> okrs = new List<Okr>();
            List<ProgressionMaster> progressionMasters = new List<ProgressionMaster>();
            List<OkrObjectiveScoreResponse> okrObjectiveScoreResponses = new List<OkrObjectiveScoreResponse>();
            /// This is for objective unread message. 
            List<Comments> unreadObjComments = new List<Comments>();
            List<Comments> unreadKeyComments = new List<Comments>();
            unreadObjComments = _OkrDataProvider.GetUnreadComments(1);
            unreadKeyComments = _OkrDataProvider.GetUnreadComments(2);
            okrs = _OkrDataProvider.GetOkrObjByYear(empId, quarter, year);
            okrObjectiveScoreResponses = _OkrDataProvider.GetObjectiveScore(year, quarter, empId);
            progressionMasters = _OkrDataProvider.GetProgression(year, quarter);
            var userList = GetAllUserFromUserManagement(jwtToken);
            var avgOkrScore = _OkrDataProvider.GetEmployeeAvgScore(quarter, year, empId);
            var subordinate = _OkrDataProvider.GetOkrMyTeam(quarter, year, identity.EmployeeId).Count();
            var userSubordinate = userList.Where(x => x.reportingTo == identity.EmployeeId).Count();
            if (userList != null)
            {
                var userDetails = userList.FirstOrDefault(x => x.employeeId == empId);
                if (userDetails.reportingTo == identity.EmployeeId)
                {
                    isSuperviserWantToSee = true;
                }
            }
            decimal totalScore = 0.00M;
            int totalNumberObjective = 0;
            if (okrs != null)
            {
                foreach (var item in okrs)
                {
                    OkrRequest okrRequest = new OkrRequest();
                    if (unreadObjComments == null)
                    {
                        okrRequest.ObjUnreadComment = 0;
                    }
                    else
                    {
                        okrRequest.ObjUnreadComment = unreadObjComments.Count(x => x.TypeId == item.OkrId && x.CreatedBy != identity.EmployeeId);
                    }

                    if (item.ImportedId != 0)
                    {
                        long importedOkrId = item.ImportedId == null ? 0 : Convert.ToInt64(item.ImportedId);
                        var data = _OkrDataProvider.OkrbyOkrid(importedOkrId).FirstOrDefault();
                        okrRequest.ImportedPeriod = data == null ? "Q" + quarter + "-" + year : "Q" + data.Quarter + "-" + data.Year;
                    }
                    else
                    {
                        okrRequest.ImportedPeriod = "Q" + quarter + "-" + year;
                    }

                    okrRequest.Year = item.Year;
                    okrRequest.Quarter = item.Quarter;
                    okrRequest.EmployeeId = item.EmployeeId;
                    okrRequest.OkrId = item.OkrId;
                    okrRequest.OkrObjective = item.OkrObjective;
                    okrRequest.Type = item.Type;
                    okrRequest.IsImported = item.IsImported;
                    okrRequest.ImportedId = item.ImportedId;
                    okrRequest.Status = item.Status;
                    okrRequest.Owner = item.Owner;
                    var ownerUser = userList.FirstOrDefault(x => x.employeeId == item.Owner);
                    okrRequest.OwnerName = ownerUser == null ? " " : ownerUser.firstName + " " + ownerUser.lastName;
                    okrRequest.OkrDescription = item.OkrDescription;
                    okrRequest.CreatedOn = item.CreatedOn;
                    okrRequest.UpdatedOn = item.UpdatedOn == null ? item.CreatedOn : item.UpdatedOn;

                    var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                    List<OkrDetailReq> okrDetails = new List<OkrDetailReq>();
                    foreach (var data in okrDetail)
                    {
                        OkrDetailReq okrRecord = new OkrDetailReq();
                        okrRecord.KeyId = data.KeyId;
                        okrRecord.KeyDesc = data.KeyDesc;
                        okrRecord.Status = data.Status;
                        okrRecord.Score = data.Score;
                        okrRecord.IsNotify = data.IsNotify;
                        okrRecord.DueDate = data.DueDate;
                        okrRecord.CreatedOn = data.CreatedOn;
                        okrRecord.UpdatedOn = data.UpdatedOn == null ? data.CreatedOn : data.UpdatedOn;
                        var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                        okrRecord.ProgressionPercentage = record == null ? 0.00M : record.ProgressPercentage;
                        okrRecord.ProgressionName = record == null ? "" : record.ProgressName;
                        okrRecord.ProgressionValue = record == null ? 0 : record.ProgressValue;
                        if (unreadKeyComments == null)
                        {
                            okrRequest.ObjUnreadComment = okrRequest.ObjUnreadComment + 0;
                            okrRecord.KeyUnreadComment = 0;
                        }
                        else
                        {
                            okrRequest.ObjUnreadComment = okrRequest.ObjUnreadComment + unreadKeyComments.Count(x => x.TypeId == data.KeyId && x.CreatedBy != identity.EmployeeId);
                            okrRecord.KeyUnreadComment = okrRequest.ObjUnreadComment + unreadKeyComments.Count(x => x.TypeId == data.KeyId && x.CreatedBy != identity.EmployeeId);
                        }
                        okrDetails.Add(okrRecord);

                    }
                    okrRequest.ProgressionScore
                         = (okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId)) == null
                         ? "0" : okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId).Score.ToString("0");
                    okrRequest.OkrDetailsList = okrDetails;
                    totalScore = totalScore + Convert.ToDecimal(okrRequest.ProgressionScore);
                    totalNumberObjective = totalNumberObjective + 1;
                    okrRequests.Add(okrRequest);

                }
                if (okrRequests != null && isSuperviserWantToSee == false)
                {
                    okrRequests = okrRequests.Where(x => x.Owner == identity.EmployeeId || x.EmployeeId == identity.EmployeeId).ToList();
                }
                dashboard.OkrRequests = okrRequests;
                dashboard.AverageScore = Convert.ToDecimal(avgOkrScore.AverageScore).ToString("0");//totalNumberObjective == 0 ? "0" : (totalScore / totalNumberObjective).ToString("0");
                dashboard.IsAbleToDesign = _OkrDataProvider.IsAbleToDesign(year, quarter, empId);
                if (subordinate > 0 || userSubordinate > 0)
                {
                    dashboard.IsTeamAvailable = true;
                }
                else
                {
                    dashboard.IsTeamAvailable = false;
                }
                dashboard.AverageAspScore = Convert.ToDecimal(avgOkrScore.AverageAspScore).ToString("0");
                dashboard.AverageOprScore = Convert.ToDecimal(avgOkrScore.AverageOprScore).ToString("0");
            }

            return dashboard;
        }
        public bool AutoSubmit()
        {
            var result = _OkrDataProvider.AutoSubmit();
            if (result == true)
            {
                string year = DateTime.Now.Year.ToString();
                int month = DateTime.Now.Month;
                int quarter = (Convert.ToInt32(month) + 2) / 3;

                var okr = _OkrDataProvider.GetOkr(quarter, Convert.ToInt32(year));
                var distinctEmployeeId = okr.Select(e => new { e.EmployeeId })
                                .Distinct();

                foreach (var item1 in distinctEmployeeId)
                {

                    _OkrDataProvider.CalculateScore(Convert.ToInt32(year), quarter, Convert.ToInt64(item1.EmployeeId));
                }
            }

            return result;
        }
        public List<MyOkrTeamResponse> GetMyOkrTeam(int quarter, int year, string jwtToken, Identity identity)
        {
            List<MyOkrTeamResponse> myOkrTeamResponses = new List<MyOkrTeamResponse>();
            var userDetail = GetAllUserFromUserManagement(jwtToken);
            var okrDetails = _OkrDataProvider.GetOkrMyTeam(quarter, year, identity.EmployeeId);
            var distinctOkrEmployeeId = okrDetails.Select(e => new { e.EmployeeId })
                             .Distinct();

            //var distinctSubordinateEmployeeId = userDetail.Select(e => new { e.employeeId })
            //                 .Distinct();


            var okrOwner = (from u in userDetail
                            join o in distinctOkrEmployeeId on u.employeeId equals o.EmployeeId
                            select new MyOkrTeamResponse()
                            {
                                UserId = u.userId,
                                FirstName = u.firstName,
                                LastName = u.lastName,
                                RoleId = u.roleId,
                                RoleName = u.roleName,
                                EmployeeId = u.employeeId,
                                EmailId = u.emailId,
                                Status = u.status,
                                ReportingTo = u.reportingTo,
                                ReportingName = u.reportingName
                            }).ToList().Distinct();
            myOkrTeamResponses.AddRange(okrOwner);
            var userSubordinate = userDetail.Where(x => x.reportingTo == identity.EmployeeId);
            foreach (var u in userSubordinate)
            {
                if (myOkrTeamResponses.Count(x => x.EmployeeId == u.employeeId) > 0)
                {
                    continue;
                }
                else
                {
                    MyOkrTeamResponse myOkrTeamResponse = new MyOkrTeamResponse();
                    myOkrTeamResponse.UserId = u.userId;
                    myOkrTeamResponse.FirstName = u.firstName;
                    myOkrTeamResponse.LastName = u.lastName;
                    myOkrTeamResponse.RoleId = u.roleId;
                    myOkrTeamResponse.RoleName = u.roleName;
                    myOkrTeamResponse.EmployeeId = u.employeeId;
                    myOkrTeamResponse.EmailId = u.emailId;
                    myOkrTeamResponse.Status = u.status;
                    myOkrTeamResponse.ReportingTo = u.reportingTo;
                    myOkrTeamResponse.ReportingName = u.reportingName;
                    myOkrTeamResponses.Add(myOkrTeamResponse);
                }

            }
            myOkrTeamResponses = myOkrTeamResponses.OrderBy(x => x.FirstName).ToList();
            return myOkrTeamResponses;

        }

        public int CalculateAvgScoreAtAutoSubmit(int quarter, int year)
        {
            var okr = _OkrDataProvider.GetOkr(quarter, year);
            var distinctEmployeeId = okr.Select(e => new { e.EmployeeId })
                             .Distinct();



            foreach (var item1 in distinctEmployeeId)
            {

                _OkrDataProvider.CalculateScore(year, quarter, Convert.ToInt64(item1.EmployeeId));
            }




            return 1;
        }

        public List<OkrResetLog> GetResetLog()
        {
            List<OkrResetLog> okrResetLogs = new List<OkrResetLog>();
            DateTime currentDate = DateTime.Now;
            int quarter = (currentDate.Month - 1) / 3 + 1;
            int year = currentDate.Year;
            okrResetLogs = _OkrDataProvider.GetResetLog(quarter, year, currentDate);
            return okrResetLogs;
        }

        public ScoreCardResponse ScoreCard(long empId, int quarter, int year, string jwtToken, Identity loginUser)
        {
            List<OkrObjectiveScoreResponse> okrObjectiveScoreResponses = new List<OkrObjectiveScoreResponse>();
            ScoreCardResponse scoreCardResponse = new ScoreCardResponse();
            List<ScoreCardOkr> oppScoreCardOkrs = new List<ScoreCardOkr>();
            List<ScoreCardOkr> aspOkrRequest = new List<ScoreCardOkr>();
            var okrs = _OkrDataProvider.GetOkrObjByYear(empId, quarter, year);
            var users = GetAllUserFromPassport();
            //var usersOfUserManagement = GetAllUserFromUserManagement(jwtToken).FirstOrDefault(x=>x.employeeId ==empId);
            if (okrs.Count() > 0 && users != null)
            {
                okrObjectiveScoreResponses = _OkrDataProvider.GetObjectiveScore(year, quarter, empId);
                var progressionMasters = _OkrDataProvider.GetProgression(year, quarter);
                var avgOkrScore = _OkrDataProvider.GetEmployeeAvgScore(quarter, year, empId);
               
                var scoreUser = users.FirstOrDefault(x => x.EmployeeId == empId);
               
                var aspOkr = okrs.Where(x => x.Type == 1).ToList();
                var oppOkr = okrs.Where(x => x.Type == 2).ToList();

                foreach (var item in aspOkr)
                {
                    ScoreCardOkr scoreCardOkr = new ScoreCardOkr();                    
                    scoreCardOkr.OkrObjective = item.OkrObjective;
                    scoreCardOkr.ObjAvgScore = (okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId)) == null
                         ? "0" : okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId).Score.ToString("0");

                    var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                    List<ScoreCardKey> scoreCardKeys = new List<ScoreCardKey>();
                    foreach (var data in okrDetail)
                    {
                        ScoreCardKey okrRecord = new ScoreCardKey();                        
                        okrRecord.KeyDesc = data.KeyDesc;
                        var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                        okrRecord.Score = record == null ? 0.00M : record.ProgressPercentage;
                        scoreCardKeys.Add(okrRecord);

                    }
                    scoreCardOkr.scoreCardKeys = scoreCardKeys;
                    aspOkrRequest.Add(scoreCardOkr);
                }

                foreach (var item in oppOkr)
                {
                    ScoreCardOkr scoreCardOkr = new ScoreCardOkr();                    
                    scoreCardOkr.OkrObjective = item.OkrObjective;
                    scoreCardOkr.ObjAvgScore = (okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId)) == null
                        ? "0" : okrObjectiveScoreResponses.FirstOrDefault(x => x.OkrId == item.OkrId).Score.ToString("0");

                    var okrDetail = _OkrDataProvider.GetOkrKeyByOkrId(item.OkrId);
                    List<ScoreCardKey> okrscoreCardKeys = new List<ScoreCardKey>();
                    foreach (var data in okrDetail)
                    {
                        ScoreCardKey okrRecord = new ScoreCardKey();
                        okrRecord.KeyDesc = data.KeyDesc;
                        var record = progressionMasters.FirstOrDefault(x => x.Id == Convert.ToInt64(data.Score));
                        okrRecord.Score = record == null ? 0.00M : record.ProgressPercentage;
                        okrscoreCardKeys.Add(okrRecord);

                    }
                    scoreCardOkr.scoreCardKeys = okrscoreCardKeys;
                    oppScoreCardOkrs.Add(scoreCardOkr);
                }
                scoreCardResponse.AverageAspScore = Convert.ToDecimal(avgOkrScore.AverageAspScore).ToString("0");
                scoreCardResponse.AverageOppScore = Convert.ToDecimal(avgOkrScore.AverageOprScore).ToString("0");
                scoreCardResponse.AspScoreCard = aspOkrRequest;
                scoreCardResponse.OprScoreCard = oppScoreCardOkrs;
                scoreCardResponse.Name =  loginUser == null ? "" : loginUser.FirstName.Trim() + " " + loginUser.LastName.Trim();
                scoreCardResponse.ImagePath = loginUser == null ? "" : loginUser.ImageDetail;
                scoreCardResponse.EmployeeId = scoreUser.EmployeeId;
                scoreCardResponse.Grade = scoreUser.GradeName;
                scoreCardResponse.Email = scoreUser.MailId;
                scoreCardResponse.CompetencyName = scoreUser.CompetencyName;
                scoreCardResponse.OkrPeriod = "Q" + quarter + "-" + year;
                scoreCardResponse.OrganizationName = scoreUser.OrganizationName;
                scoreCardResponse.AverageScore = Convert.ToDecimal(avgOkrScore.AverageScore).ToString("0");
            }
            return scoreCardResponse;
        }
    }

}

