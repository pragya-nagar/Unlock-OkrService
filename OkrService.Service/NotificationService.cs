using OKRService.Common;
using OKRService.EF;
using OKRService.Service.Contracts;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OKRService.Service
{
    [ExcludeFromCodeCoverage]
    public class NotificationService : BaseService, INotificationService
    {
        private readonly IRepositoryAsync<GoalObjective> goalObjectiveRepo;
        private readonly IRepositoryAsync<GoalKey> goalKeyRepo;
        private readonly ICommonService commonService;
        private readonly IKeyVaultService keyVaultService;

        public NotificationService(IServicesAggregator servicesAggregateService, ICommonService commonServices, IKeyVaultService keyVault) : base(servicesAggregateService)
        {
            goalObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<GoalObjective>();
            goalKeyRepo = UnitOfWorkAsync.RepositoryAsync<GoalKey>();
            commonService = commonServices;
            keyVaultService = keyVault;
        }

        /// <summary>
        /// when obj goes to Amber or Red
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="loginUser"></param>
        /// <param name="GoalKeyId"></param>
        /// <param name="GoalObjectiveId"></param>
        /// <param name="GoalObjectiveProgress"></param>
        /// <param name="GoalKeyProgress"></param>
        /// <returns></returns>
        public async Task KrUpdateNotifications(string jwtToken, long loginUser, long GoalKeyId, long GoalObjectiveId, int GoalObjectiveProgress, int GoalKeyProgress)
        {
            var okr = GetGoalObjective(GoalObjectiveId);
            var okrAlign = GetImportedObjective(okr.ImportedId);

            if (okrAlign != null)
            {
                if (GoalObjectiveProgress == (int)ProgressMaster.Lagging)
                {
                    var AmberNotifications = Constants.AmberMessage;
                    List<long> to = new List<long>
                    {
                        loginUser,
                        okrAlign.EmployeeId
                    };
                    NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                    {
                        By = loginUser,
                        NotificationToList = to,
                        Url = "",
                        NotificationText = AmberNotifications.Replace("<OKR>", okr.ObjectiveName),
                        AppId = Constants.AppID,
                        NotificationType = (int)NotificationType.AmberMessage,
                        MessageType = (int)MessageTypeForNotifications.AlertMessages,
                        JwtToken = jwtToken
                    };
                    await commonService.NotificationsAsync(notificationsCommonRequest);
                }

                if (GoalObjectiveProgress == (int)ProgressMaster.AtRisk)
                {
                    var RedNotifications = Constants.RedMessage;
                    List<long> to = new List<long>
                    {
                        loginUser,
                        okrAlign.EmployeeId
                    };
                    NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                    {
                        By = loginUser,
                        NotificationToList = to,
                        Url = "",
                        NotificationText = RedNotifications.Replace("<OKR>", okr.ObjectiveName),
                        AppId = Constants.AppID,
                        NotificationType = (int)NotificationType.RedMessage,
                        MessageType = (int)MessageTypeForNotifications.AlertMessages,
                        JwtToken = jwtToken
                    };
                    await commonService.NotificationsAsync(notificationsCommonRequests);
                }
            }
        }

        /// <summary>
        /// Obj Contributors
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="loginUser"></param>
        /// <param name="contriEmployeeId"></param>
        /// <param name="objId"></param>
        /// <returns></returns>
        public async Task ObjContributorsNotifications(string jwtToken, long loginUser, long contriEmployeeId, long objId, long contribObjId)
        {
            var objName = GetGoalObjective(objId).ObjectiveName;
            var notification = Constants.ObjContributors;
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == contriEmployeeId)?.FirstName;
            var firstnameLoginUser = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser)?.FirstName;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == contriEmployeeId)?.EmailId;
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();

            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.ASO.ToString(), jwtToken);
            string body = template.Body;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + contriEmployeeId;
            }
            body = body.Replace("<user>", firstname).Replace("topBar", blobCdnUrl + Constants.TopBar)
                       .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("<supportEmailId>", Constants.SupportEmailId)
                       .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("<Frontend>", loginUrl + Constants.ModifiedLogin).Replace("<goToGoalsUrl>", loginUrl + "?redirectUrl=" + "Contributor/" + contribObjId + "&empId=" + contriEmployeeId)
                       .Replace("OKRfocus", objName).Replace("<manager>", firstnameLoginUser).Replace("<Requestor>", firstnameLoginUser).Replace("goToGoals", blobCdnUrl + Constants.GoToGoalImage)
                       .Replace("year", Convert.ToString(DateTime.Now.Year))
                       .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                       .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                       .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = loginUser,
                To = contriEmployeeId,
                Url = "Contributors/" + contribObjId,
                NotificationText = notification.Replace("<OKR>", objName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);

            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                To = loginUser,
                By = loginUser,
                Url = "",
                NotificationText = Constants.AssignContriAlert.Replace("<user>", firstname).Replace("<OKR>", objName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);
        }

        /// <summary>
        /// Align to objectives and import previous OKR
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="loginUser"></param>
        /// <param name="importedType"></param>
        /// <param name="importedId"></param>
        /// <returns></returns>
        public async Task AlignObjNotifications(string jwtToken, long loginUser, int importedType, long importedId)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var EmployeeName = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser).FirstName;
            var align = GetImportedObjective(importedId);
            if (align != null)
            {
                var alignEmployee = align.EmployeeId;
                var alignEmployeeName = allEmployee.FirstOrDefault(x => x.EmployeeId == alignEmployee).FirstName;
                var notification = Constants.ObjAlign;

                NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                {
                    By = loginUser,
                    To = loginUser,
                    Url = "",
                    NotificationText = notification.Replace("<contributor>", align.ObjectiveName).Replace("<user receiving contribution>", alignEmployeeName),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequest);

                NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                {
                    By = loginUser,
                    To = alignEmployee,
                    Url = "",
                    NotificationText = notification.Replace("<contributor>", align.ObjectiveName).Replace("<user receiving contribution>", EmployeeName),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequests);
            }
        }

        /// <summary>
        /// when clicking on Lock Goals
        /// </summary>
        /// <param name="empId"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>

        public async Task LockGoalsNotifications(long empId, string jwtToken)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var reportingTo = allEmployee.FirstOrDefault(x => x.EmployeeId == empId).ReportingTo;
            var EmployeeName = allEmployee.FirstOrDefault(x => x.EmployeeId == empId).FirstName;
            var notification = Constants.LockGoalsForUser;

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = empId,
                To = empId,
                Url = "",
                NotificationText = notification.Replace("<user>", EmployeeName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.lockMyGoals,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);

            var notificationstomanager = Constants.LockGoalsForManager;
            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                By = empId,
                To = Convert.ToInt64(reportingTo),
                Url = "",
                NotificationText = notificationstomanager.Replace("<user>", EmployeeName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.lockMyGoals,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);
        }

        public async Task DeleteOkrNotifications(List<long> OkrContributors, int goalType, long GoalId, long empId, string jwtToken)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var EmployeeName = allEmployee.FirstOrDefault(x => x.EmployeeId == empId).FirstName;

            var OkrDeleted = GetObjective(GoalId);
            var parentEmpId = GetObjectiveParent(GoalId);

            if (parentEmpId > 0)
            {
                OkrContributors.Add(parentEmpId);
            }

            var notificationManager = Constants.DeleteOkrManager;

            OkrContributors.Remove(empId);

            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                By = empId,
                NotificationToList = OkrContributors,
                Url = "",
                NotificationText = notificationManager.Replace("<user>", EmployeeName).Replace("<OKR>", OkrDeleted.ObjectiveName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.DeleteOkr,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);

            var notification = Constants.DeleteOkrUser;

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = empId,
                To = empId,
                Url = "",
                NotificationText = notification.Replace("<user>", EmployeeName).Replace("<OKR>", OkrDeleted.ObjectiveName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.DeleteOkr,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);
        }

        public async Task DeleteKrNotifications(int Count, List<DeleteKrResponse> deleteKrResponses, List<KrContributors> krContributors, int goalType, long GoalId, long empId, string jwtToken)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;

            var EmployeeName = allEmployee.FirstOrDefault(x => x.EmployeeId == empId).FirstName;

            var KrDeleted = GetDeletedKey(GoalId);
            var objname = GetObjective(KrDeleted.GoalObjectiveId);
            var parentEmpId = GetKeyParent(GoalId);

            string notifications = Constants.DeleteKrUser;
            string notificationsManager = Constants.DeleteKrManager;

            if (parentEmpId > 0)
            {
                if (Count == 1)
                {
                    NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                    {
                        By = empId,
                        To = (long)parentEmpId,
                        Url = "",
                        NotificationText = notificationsManager.Replace("<user>", EmployeeName).Replace("<KR>", KrDeleted.KeyDescription).Replace("<OKR>", objname.ObjectiveName),
                        AppId = Constants.AppID,
                        NotificationType = (int)NotificationType.DeleteKr,
                        MessageType = (int)MessageTypeForNotifications.AlertMessages,
                        JwtToken = jwtToken
                    };
                    await commonService.NotificationsAsync(notificationsCommonRequests);
                }
                else
                {
                    NotificationsCommonRequest notificationCommonRequests = new NotificationsCommonRequest
                    {
                        By = empId,
                        To = (long)parentEmpId,
                        Url = "",
                        NotificationText = notifications.Replace("<user>", EmployeeName).Replace("<KR>", KrDeleted.KeyDescription),
                        AppId = Constants.AppID,
                        NotificationType = (int)NotificationType.DeleteKr,
                        MessageType = (int)MessageTypeForNotifications.AlertMessages,
                        JwtToken = jwtToken
                    };
                    await commonService.NotificationsAsync(notificationCommonRequests);
                }
            }
            else
            {
                NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                {
                    By = empId,
                    To = empId,
                    Url = "",
                    NotificationText = notifications.Replace("<user>", EmployeeName).Replace("<KR>", KrDeleted.KeyDescription),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.DeleteKr,
                    MessageType = (int)MessageTypeForNotifications.AlertMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequests);
            }

            if (krContributors.Count > 0)
            {
                foreach (var item in krContributors)
                {
                    if (item.EmployeeId != empId)
                    {
                        var result = GetDeletedKey(item.GoalId);
                        //var KRCount = deleteKrResponses.Count(x => x.GoalId == result.GoalObjectiveId);
                        var KRCount = goalKeyRepo.GetQueryable().Where(x => x.GoalKeyId == result.GoalObjectiveId && !x.IsActive).ToList().Count();
                        var OkrDeleted = GetObjective(result.GoalObjectiveId);
                        if (KRCount == 1)
                        {
                            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                            {
                                By = empId,
                                To = item.EmployeeId,
                                Url = "",
                                NotificationText = notificationsManager.Replace("<user>", EmployeeName).Replace("<KR>", KrDeleted.KeyDescription).Replace("<OKR>", OkrDeleted.ObjectiveName),
                                AppId = Constants.AppID,
                                NotificationType = (int)NotificationType.DeleteKr,
                                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                                JwtToken = jwtToken
                            };
                            await commonService.NotificationsAsync(notificationsCommonRequests);
                        }
                        else
                        {
                            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                            {
                                By = empId,
                                To = item.EmployeeId,
                                Url = "",
                                NotificationText = notifications.Replace("<user>", EmployeeName).Replace("<KR>", KrDeleted.KeyDescription),
                                AppId = Constants.AppID,
                                NotificationType = (int)NotificationType.DeleteKr,
                                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                                JwtToken = jwtToken
                            };
                            await commonService.NotificationsAsync(notificationsCommonRequest);
                        }
                    }
                }
            }
        }

        public async Task BulkUnlockApproveNotificationsAndEmails(EmployeeResult users, UnLockRequest item, string jwtToken)
        {
            var user = users.Results.FirstOrDefault(x => x.EmployeeId == item.EmployeeId);
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.RA.ToString(), jwtToken);
            string body = template.Body;
            body = body.Replace("<user>", user.FirstName);
            MailRequest mailRequest = new MailRequest();
            if (user.EmailId != null && template.Subject != "")
            {
                mailRequest.MailTo = user.EmailId;
                mailRequest.Subject = template.Subject;
                mailRequest.Body = body;
                /// await _commonServices.SentMailAsync(mailRequest, jwtToken);
            }

            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                To = item.EmployeeId,
                By = item.EmployeeId,
                Url = "",
                NotificationText = Constants.ResetOkrMessage.Replace("<user>", user.FirstName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ResetOkr,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);

            if (!string.IsNullOrWhiteSpace(user.ReportingTo))
            {
                NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                {
                    To = Convert.ToInt64(user.ReportingTo),
                    By = Convert.ToInt64(user.ReportingTo),
                    Url = "",
                    NotificationText = Constants.ResetOkrMessage.Replace("<user>", user.FirstName),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.ResetOkr,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequest);
            }
        }

        public async Task DeleteContributorsByChangingType(long loginempId, string jwtToken, string objective, long ContriEmp)
        {
            var notificationManager = Constants.DeleteOkrManager;
            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                By = loginempId,
                To = ContriEmp,
                Url = "",
                NotificationText = notificationManager.Replace("<OKR>", objective),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.DeleteOkr,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);
        }

        public GoalObjective GetGoalObjective(long GoalObjectiveId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == GoalObjectiveId && x.IsActive);
        }

        public GoalKey GetGoalKey(long goalKeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);
        }

        public GoalObjective GetImportedObjective(long importedId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == importedId);
        }



        public GoalObjective GetObjective(long goalId)
        {
            return goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalId);
        }

        public long GetObjectiveParent(long goalId)
        {
            var result = new GoalObjective();
            long parentEmpId = 0;
            result = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalId && x.ImportedId > 0);
            if (result != null)
            {
                var parentObj = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == result.ImportedId && x.ImportedId > 0);
                if (parentObj != null)
                {
                    parentEmpId = parentObj.EmployeeId;
                }
            }

            return parentEmpId;
        }

        public GoalKey GetDeletedKey(long GoalKeyId)
        {
            return goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == GoalKeyId);
        }

        public long? GetKeyParent(long goalkeyId)
        {
            long? parentEmpId = 0;
            var goalObjId = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalkeyId)?.GoalObjectiveId;
            if (goalObjId != null)
            {
                var parentKey = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == goalObjId);
                if (parentKey != null)
                {
                    parentEmpId = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == parentKey.ImportedId)?.EmployeeId;
                }
            }

            return parentEmpId;
        }

        public async Task KeyContributorsNotifications(string jwtToken, long loginUser, long ContriEmployeeId, long keyId, long ContribKeyId, GoalKey goalKey)
        {
            var keyDetails = GetGoalKey(keyId);

            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == ContriEmployeeId).FirstName;
            var firstnameLoginUser = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser).FirstName;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == ContriEmployeeId).EmailId;
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";

            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var url = settings?.FrontEndUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.AK.ToString(), jwtToken);
            string msg;

            var metric = keyDetails.MetricId == (int)Metrics.Percentage ? Constants.Percentage : keyDetails.MetricId == (int)Metrics.Currency ? Constants.Currency : keyDetails.MetricId == (int)Metrics.Numbers ? Constants.Numbers : " ";

            string body = template.Body;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + ContriEmployeeId;
            }

            var keyURL = goalKey.GoalObjectiveId == 0 ? url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + ContriEmployeeId : url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId + "&empId=" + ContriEmployeeId;

            body = body.Replace("Contri", firstname).Replace("topBar", blobCdnUrl + Constants.TopBar)
                       .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("supportEmailId", Constants.SupportEmailId)
                       .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("<Frontend>", loginUrl + Constants.ModifiedLogin).Replace("<goToGoalsUrl>", loginUrl + "?redirectUrl=" + "Contributor/" + ContribKeyId + "&empId=" + ContriEmployeeId)
                       .Replace("OKRfocus", keyDetails.KeyDescription).Replace("user", firstnameLoginUser).Replace("KeyDescription", goalKey.KeyDescription).Replace("goToGoals", blobCdnUrl + Constants.GoToGoalImage)
                       .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("assignment", blobCdnUrl + Constants.OkrAssignment).Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                       .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("dot", blobCdnUrl + Constants.dotImage)
                       .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("handshake", blobCdnUrl + Constants.Hand)
                       .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("cant", Constants.cantkeyword).Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("footer", blobCdnUrl + Constants.FooterLogo)
                       .Replace("tinggg", keyURL).Replace("<GoTo>", url + "?redirectUrl=" + "KrAssignment" + "/" + goalKey.AssignmentTypeId + "/" + ContribKeyId + "/" + goalKey.GoalObjectiveId + "/1" + "&empId=" + ContriEmployeeId)
                       .Replace("<abcd>", url + "?redirectUrl=" + "KrAssignment" + "/" + goalKey.AssignmentTypeId + "/" + ContribKeyId + "/" + goalKey.GoalObjectiveId + "/2" + "&empId=" + ContriEmployeeId).Replace("<bdbdbdb>", url + "?redirectUrl=unlock-me&empId=" + ContriEmployeeId);

            if (keyDetails.MetricId == (int)Metrics.Boolean || keyDetails.MetricId == (int)Metrics.NoUnits)
            {
                msg = Constants.NoUnitAlignmentofKr.Replace("<contributorname>", firstname).Replace("<KR>", goalKey.KeyDescription).Replace("<assignername>", firstnameLoginUser);
            }
            else
            {
                msg = Constants.AlignmentofKr.Replace("<contributorname>", firstname).Replace("<KR>", goalKey.KeyDescription).Replace("<MV>", Convert.ToString(goalKey.TargetValue)).Replace("<assignername>", firstnameLoginUser).Replace("<MN>", metric);
            }

            var notificationUrl =  "KrAssignment/" + goalKey.AssignmentTypeId + "/" + ContribKeyId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.KrStatusId;

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = loginUser,
                To = ContriEmployeeId,
                Url = notificationUrl,
                NotificationText = msg,
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);


            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }
        }

        public async Task AligningParentObjective(string jwtToken, long loginUser, AddContributorRequest addContributorRequest, GoalKey goalKey)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser)?.FirstName;
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var goal = GetGoalKey(addContributorRequest.GoalKeyId);
            var sourceName = allEmployee.FirstOrDefault(x => x.EmployeeId == goal.EmployeeId)?.FirstName;
            string msg;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == goal.EmployeeId)?.EmailId;
            var url = settings?.FrontEndUrl;
            var metric = goalKey.MetricId == (int)Metrics.Percentage ? Constants.Percentage : goalKey.MetricId == (int)Metrics.Currency ? Constants.Currency : goalKey.MetricId == (int)Metrics.Numbers ? Constants.Numbers : " ";

            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();

            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = url + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.AKR.ToString(), jwtToken);
            string body = template.Body;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + goal.EmployeeId;
            }
            body = body.Replace("user", firstname).Replace("topBar", blobCdnUrl + Constants.TopBar)
              .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("supportEmailId", Constants.SupportEmailId)
              .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("footer", blobCdnUrl + Constants.FooterLogo)
              .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
              .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("dot", blobCdnUrl + Constants.dotImage)
              .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("Source", sourceName).Replace("KeyDescription", addContributorRequest.KeyDescription)
              .Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("<abcdefg>", url + "?redirectUrl=unlock-me&empId=" + goal.EmployeeId);

            if (goalKey.MetricId == (int)Metrics.Boolean || goalKey.MetricId == (int)Metrics.NoUnits)
            {
                msg = Constants.NoUnitBoolMsg.Replace("<Contributorname>", firstname).Replace("<KR>", addContributorRequest.KeyDescription).Replace("<Sourcename>", sourceName);
            }
            else
            {
                msg = Constants.ParentObjectiveMsg.Replace("<Contributorname>", firstname).Replace("<KR>", addContributorRequest.KeyDescription).Replace("<MV>", Convert.ToString(addContributorRequest.TargetValue)).Replace("<Sourcename>", sourceName).Replace("<M>", metric);
            }

            if (addContributorRequest.AssignmentTypeId == (int)AssignmentType.WithParentObjective && goalKey.GoalObjectiveId > 0)
            {
                var krObjective = GetGoalObjective(addContributorRequest.GoalObjectiveId);
                body = body.Replace("tinggg", url + "?redirectUrl=" + "Feedback" + "/3/" + goalKey.GoalObjectiveId + "/" + goalKey.AssignmentTypeId + "/" + loginUser + "&empId=" + goal.EmployeeId);
                NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                {
                    To = krObjective.EmployeeId,
                    By = loginUser,
                    Url = "Feedback" + "/3/" + goalKey.GoalObjectiveId + "/" + goalKey.AssignmentTypeId + "/" + loginUser,
                    NotificationText = msg,
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequests);
            }
     
            else if (goalKey.GoalObjectiveId == 0)
            {
                body = body.Replace("tinggg", url + "?redirectUrl=" + "Feedback" + "/3/" + goalKey.GoalKeyId + "/" + goalKey.AssignmentTypeId + "/" + loginUser + "&empId=" + goal.EmployeeId);
                NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                {
                    To = (long)goal.EmployeeId,
                    By = loginUser,
                    Url = "Feedback" + "/3/" + goalKey.GoalKeyId + "/" + goalKey.AssignmentTypeId + "/" + loginUser,
                    NotificationText = msg,
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequest);
            }

            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

        }

        public async Task AcceptsOkr(string jwtToken, long loginUser, ContributorKeyResultRequest contributorKeyResultRequest)
        {
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser).FirstName;
            var goalKey = GetGoalKey(contributorKeyResultRequest.ImportedId);
            var sourceName = allEmployee.FirstOrDefault(x => x.EmployeeId == goalKey.EmployeeId)?.FirstName;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == goalKey.EmployeeId)?.EmailId;
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var url = settings?.FrontEndUrl;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.KA.ToString(), jwtToken);
            string body = template.Body;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + goalKey.EmployeeId;
            }


            var updatedBody = string.Empty;
            var currency = string.Empty;

            currency = goalKey.CurrencyId == 1 ? "$" : goalKey.CurrencyId == 2 ? "€" : goalKey.CurrencyId == 3 ? "₹" : goalKey.CurrencyId == 4 ? "¥" : goalKey.CurrencyId == 5 ? "£" : " ";

            var metric = goalKey.MetricId == (int)Metrics.Percentage ? Constants.Percentage : goalKey.MetricId == (int)Metrics.Currency ? Constants.Currency : goalKey.MetricId == (int)Metrics.Numbers ? Constants.Numbers : " ";

            var keyUrl = goalKey.GoalObjectiveId != 0 ? url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId + "&empId=" + goalKey.EmployeeId : url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + goalKey.EmployeeId;

            var notificationsUrl = goalKey.GoalObjectiveId != 0 ? "KRAcceptDecline/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId : "KRAcceptDecline/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId;

            //if (goalkey.MetricId == (int)Metrics.Boolean)
            //{
            //    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + " \"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + contributorKeyResultRequest.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by <strong>" + firstname + "</strong> with <strong>Yes</strong>" + "." + "</td> ";
            //}
            if (goalKey.MetricId == (int)Metrics.NoUnits || goalKey.MetricId == (int)Metrics.Boolean)
            {
                updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + contributorKeyResultRequest.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by  <strong>" + firstname + "</strong>" + "." + "</td> ";
            }


            if (goalKey.TargetValue != contributorKeyResultRequest.TargetValue)
            {

                if (goalKey.MetricId == (int)Metrics.Currency)
                {

                    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + contributorKeyResultRequest.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by the <strong>" + firstname + "</strong>  with  changes to the " + metric + " metrics with target  <strong> " + currency + Decimal.Truncate(contributorKeyResultRequest.TargetValue) + "</strong>" + "." + "</td> ";
                }
                else if (goalKey.MetricId == (int)Metrics.Percentage || goalKey.MetricId == (int)Metrics.Numbers)
                {
                    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + contributorKeyResultRequest.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by the <strong>" + firstname + "</strong> with  changes to the " + metric + " metrics with target <strong> " + Decimal.Truncate(contributorKeyResultRequest.TargetValue) + "</strong>" + "." + "</td> ";
                }


                NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
                {
                    To = (long)goalKey.EmployeeId,
                    By = loginUser,
                    Url = notificationsUrl,
                    NotificationText = Constants.AcceptwithModifications.Replace("<firstname>", firstname).Replace("<KR>", contributorKeyResultRequest.KeyDescription),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequests);

            }
            else
            {

                if (goalKey.MetricId == (int)Metrics.Currency)
                {

                    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + contributorKeyResultRequest.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by <strong>" + firstname + "</strong>  with no changes to the " + metric + " metrics with target  <strong> " + currency + Decimal.Truncate(goalKey.TargetValue) + "</strong>" + "." + "</td> ";
                }
                else if (goalKey.MetricId == (int)Metrics.Percentage || goalKey.MetricId == (int)Metrics.Numbers)
                {
                    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + goalKey.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> accepted by <strong>" + firstname + "</strong> with no changes to the " + metric + " metrics with target <strong> " + Decimal.Truncate(goalKey.TargetValue) + "</strong>" + "." + "</td> ";
                }

                NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
                {
                    To = (long)goalKey.EmployeeId,
                    By = loginUser,
                    Url = notificationsUrl,
                    NotificationText = Constants.AcceptwithoutModifications.Replace("<firstname>", firstname).Replace("<user>", firstname).Replace("<KR>",contributorKeyResultRequest.KeyDescription),
                    AppId = Constants.AppID,
                    NotificationType = (int)NotificationType.AlignObjectives,
                    MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                    JwtToken = jwtToken
                };
                await commonService.NotificationsAsync(notificationsCommonRequest);

            }


            body = body.Replace("name", sourceName).Replace("topBar", blobCdnUrl + Constants.TopBar)
         .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("supportEmailId", Constants.SupportEmailId)
         .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("footer", blobCdnUrl + Constants.FooterLogo)
        .Replace("dot", blobCdnUrl + Constants.dotImage)
         .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("Source", sourceName).Replace("KeyDescription", contributorKeyResultRequest.KeyDescription).
          Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("hand-shake", blobCdnUrl + Constants.Hand)
          .Replace("tinggg", url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + goalKey.EmployeeId)
          .Replace("<abcdefg>", url + "?redirectUrl=unlock-me&empId=" + goalKey.EmployeeId).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("srcTwitter", blobCdnUrl + Constants.Twitter)
           .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin).Replace("lk", linkedInUrl).Replace("changesinokr", blobCdnUrl + Constants.ChangesinOkr)
            .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("ijk", instagramUrl).Replace("message", blobCdnUrl + Constants.Accept)
             .Replace("<Gist>", updatedBody);


            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

        }

        public async Task DeclineKr(string jwtToken, long loginUser, ContributorKeyResultRequest contributorKeyResultRequest)
        {
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser).FirstName;
            var goalKey = GetGoalKey(contributorKeyResultRequest.ImportedId);
            var sourceName = allEmployee.FirstOrDefault(x => x.EmployeeId == goalKey.EmployeeId)?.FirstName;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == goalKey.EmployeeId)?.EmailId;
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var url = settings?.FrontEndUrl;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + goalKey.EmployeeId;
            }

            var updatedBody = string.Empty;
            var currency = string.Empty;

            var keyUrl = goalKey.GoalObjectiveId != 0 ? url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId + "&empId=" + goalKey.EmployeeId : url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + goalKey.EmployeeId;
            var notificationsUrl = goalKey.GoalObjectiveId != 0 ? "KRAcceptDecline/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId : "KRAcceptDecline/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId;

            //if (goalkey.MetricId == (int)Metrics.Boolean)
            //{
            //    updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + " \"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + goalkey.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> declined by <strong>" + firstname + "</strong> with <strong>No</strong>" + "." + "</td> ";
            //}
            if (goalKey.MetricId == (int)Metrics.NoUnits || goalKey.MetricId == (int)Metrics.Boolean)
            {
                updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + goalKey.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> declined by <strong>" + firstname + "</strong>" + "." + "</td> ";
            }
            else if (goalKey.MetricId == (int)Metrics.Currency)
            {
                currency = goalKey.CurrencyId == 1 ? "$" : goalKey.CurrencyId == 2 ? "€" : goalKey.CurrencyId == 3 ? "₹" : goalKey.CurrencyId == 4 ? "¥" : goalKey.CurrencyId == 5 ? "£" : " ";
                updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + goalKey.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> declined by <strong>" + firstname + "</strong> with target <strong> " + currency + Decimal.Truncate(goalKey.TargetValue) + "</strong>" + "." + "</td> ";
            }
            else if (goalKey.MetricId == (int)Metrics.Percentage || goalKey.MetricId == (int)Metrics.Numbers)
            {
                updatedBody = "<td align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style =\"font-size:16px;line-height:21px;color:#292929;font-family: Calibri,Arial;text-align: center;\"><a href =\""+ keyUrl + "\"  traget =\"_blank\"  style =\"font-size:16px;font-family: Calibri,Arial;font-weight:bold;color: #39A3FA;text-decoration: none;\">“ " + goalKey.KeyDescription + " ”</a></td></tr><tr><td align =\"center\" cellpadding=\"0\" cellspacing=\"0\"style =\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;text-align: center;\"> declined by <strong>" + firstname + "</strong> with target <strong> " + Decimal.Truncate(goalKey.TargetValue) + "</strong>" + "." + "</td> ";
            }

            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.KD.ToString(), jwtToken);
            string body = template.Body;
            body = body.Replace("name", firstname).Replace("topBar", blobCdnUrl + Constants.TopBar)
            .Replace("logo", blobCdnUrl + Constants.LogoImages)
            .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("footer", blobCdnUrl + Constants.FooterLogo)
           .Replace("dot", blobCdnUrl + Constants.dotImage)
           .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("Source", sourceName).Replace("KeyDescription", contributorKeyResultRequest.KeyDescription).
            Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("hand-shake", blobCdnUrl + Constants.Hand).Replace("message", contributorKeyResultRequest.KrAssigneeMessage)
             .Replace("tinggg", url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + goalKey.EmployeeId)
            .Replace("<abcdefg>", url + "?redirectUrl=unlock-me&empId=" + goalKey.EmployeeId).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("srcTwitter", blobCdnUrl + Constants.Twitter)
            .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin).Replace("lk", linkedInUrl).Replace("changesinokr", blobCdnUrl + Constants.ChangesinOkr)
             .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("ijk", instagramUrl).Replace("keyDescription", goalKey.KeyDescription)
             .Replace("<Gist>", updatedBody).Replace("mnc", blobCdnUrl + Constants.Decline);

            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }


            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                To = (long)goalKey.EmployeeId,
                By = loginUser,
                Url = notificationsUrl,
                NotificationText = Constants.DeclineKR.Replace("<firstname>", firstname).Replace("<message>", contributorKeyResultRequest.KrAssigneeMessage).Replace("<KR>",contributorKeyResultRequest.KeyDescription),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.AlignObjectives,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);

        }

        public async Task DraftOkrNotifications(string jwtToken, UserIdentity loginUser, GoalObjective goalObjective)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.DO.ToString(), jwtToken);
            var url = settings?.FrontEndUrl;
            string body = template.Body;
            string subject = template.Subject;

            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + loginUser.EmployeeId;
            }

            body = body.Replace("topBar", blobCdnUrl + Constants.TopBar).Replace("<URL>", loginUrl)
                .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("loggedInButton", blobCdnUrl + Constants.LoginButtonImage)
                .Replace("name", loginUser.FirstName).Replace("draftImage", blobCdnUrl + Constants.DraftImage)
                .Replace("KeyDescription", goalObjective.ObjectiveName).Replace("<Button>", loginUrl).Replace("supportEmailId", Constants.SupportEmailId)
                .Replace("footer", blobCdnUrl + Constants.FooterLogo).Replace("privacy", loginUrl + "/" + Constants.Privacy)
                .Replace("dot", blobCdnUrl + Constants.dotImage).Replace("terming", url + Constants.Terms).Replace("contact", url + Constants.Terms)
                .Replace("technicalSupport", loginUrl + "/" + Constants.Terms).Replace("year", Convert.ToString(DateTime.Now.Year))
                .Replace("<GoalUrl>", url + "?redirectUrl=KRAcceptDecline/1/" + goalObjective.GoalObjectiveId + "&empId=" + loginUser.EmployeeId)
                .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl); 

            subject = subject.Replace("<username>", loginUser.FirstName);


            if (loginUser.EmailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = loginUser.EmailId,
                    Subject = subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                To = loginUser.EmployeeId,
                By = loginUser.EmployeeId,
                Url = "",
                NotificationText = Constants.DraftOkrAlert.Replace("<OKR name>", goalObjective.ObjectiveName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.DraftOkr,
                MessageType = (int)MessageTypeForNotifications.AlertMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);
        }

        public async Task NudgeTeamNotifications(string jwtToken, UserIdentity loginUser, long teamEmployeeId)
        {
            NotificationsCommonRequest notificationsCommonRequests = new NotificationsCommonRequest
            {
                To = teamEmployeeId,
                By = loginUser.EmployeeId,
                Url = "",
                NotificationText = Constants.NudgeTeamMsg.Replace("<Nugder name>", loginUser.FirstName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequests);
        }

        public async Task TeamKeyContributorsNotifications(string jwtToken, long loginUser, long contribEmployeeId, long keyId, long contribKeyId, GoalKey goalKey)
        {
            var keyDetails = GetGoalKey(keyId);
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";

            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == contribEmployeeId).FirstName;
            var firstNameLoginUser = allEmployee.FirstOrDefault(x => x.EmployeeId == loginUser).FirstName;
            var msg = string.Empty;
            var metric = keyDetails.MetricId == (int)Metrics.Percentage ? Constants.Percentage : keyDetails.MetricId == (int)Metrics.Currency ? Constants.Currency : keyDetails.MetricId == (int)Metrics.Numbers ? Constants.Numbers : " ";
            var EmailId = allEmployee.FirstOrDefault(x => x.EmployeeId == contribEmployeeId).EmailId;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            var template = await commonService.GetMailerTemplateAsync(TemplateCodes.AK.ToString(), jwtToken);

            string body = template.Body;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + contribEmployeeId;
            }

            var url = settings?.FrontEndUrl;
         

            if (keyDetails.MetricId == (int)Metrics.Boolean || keyDetails.MetricId == (int)Metrics.NoUnits)
            {
                msg = Constants.NoUnitAlignmentofKr.Replace("<contributorname>", firstname).Replace("<KR>", goalKey.KeyDescription).Replace("<assignername>", firstNameLoginUser);
            }
            else
            {
                msg = Constants.AlignmentofKr.Replace("<contributorname>", firstname).Replace("<KR>", goalKey.KeyDescription).Replace("<MV>", Convert.ToString(goalKey.TargetValue)).Replace("<assignername>", firstNameLoginUser).Replace("<MN>", metric);
            }

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = loginUser,
                To = contribEmployeeId,
                Url = "KrAssignment/" + goalKey.AssignmentTypeId + "/" + contribKeyId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.KrStatusId,
                NotificationText = msg,
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);


            var keyURL = goalKey.GoalObjectiveId == 0 ? url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalKeyId + "/" + goalKey.GoalObjectiveId + "&empId=" + contribEmployeeId : url + "?redirectUrl=" + "KRAcceptDecline" + "/" + goalKey.AssignmentTypeId + "/" + goalKey.GoalObjectiveId + "/" + goalKey.GoalKeyId + "&empId=" + contribEmployeeId;

            body = body.Replace("Contri", firstname).Replace("topBar", blobCdnUrl + Constants.TopBar)
                       .Replace("logo", blobCdnUrl + Constants.LogoImages).Replace("login", blobCdnUrl + Constants.LoginButtonImage).Replace("supportEmailId", Constants.SupportEmailId)
                       .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("<Frontend>", loginUrl + Constants.ModifiedLogin).Replace("<goToGoalsUrl>", blobCdnUrl + "?redirectUrl=" + "Contributor/" + contribKeyId + "&empId=" + contribEmployeeId)
                       .Replace("OKRfocus", keyDetails.KeyDescription).Replace("user", firstNameLoginUser).Replace("KeyDescription", goalKey.KeyDescription).Replace("goToGoals", blobCdnUrl + Constants.GoToGoalImage)
                       .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("assignment", blobCdnUrl + Constants.OkrAssignment).Replace("dot", blobCdnUrl + Constants.dotImage)
                       .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("cant", Constants.cantkeyword).Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("footer", blobCdnUrl + Constants.FooterLogo)
                       .Replace("tinggg", keyURL).Replace("<GoTo>", url + "?redirectUrl=" + "KrAssignment" + "/" + goalKey.AssignmentTypeId + "/" + contribKeyId + "/" + goalKey.GoalObjectiveId + "/1" + "&empId=" + contribEmployeeId)
                       .Replace("<abcd>", url + "?redirectUrl=" + "KrAssignment" + "/" + goalKey.AssignmentTypeId + "/" + contribKeyId + "/" + goalKey.GoalObjectiveId + "/2" + "&empId=" + contribEmployeeId).Replace("<bdbdbdb>", url + "?redirectUrl=unlock-me&empId=" + contribEmployeeId)
                       .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin).Replace("handshake", blobCdnUrl + Constants.Hand)
                       .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook).Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

            if (EmailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = EmailId,
                    Subject = template.Subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

        }

        public async Task UpdateContributorsOkrNotifications(GoalObjective goalObjective, UserIdentity userIdentity, string jwtToken)
        {
            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = userIdentity.EmployeeId,
                To = goalObjective.EmployeeId,
                Url = "",
                NotificationText = Constants.UpdateContributorOkrMsg.Replace("<ownername>", userIdentity.FirstName).Replace("<OKR>", goalObjective.ObjectiveName),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);
        }

        public async Task UpdateContributorsKeyNotifications(GoalKey goalKey, UserIdentity userIdentity, string jwtToken)
        {
            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = userIdentity.EmployeeId,
                To = Convert.ToInt64(goalKey.EmployeeId),
                Url = "",
                NotificationText = Constants.UpdateContributorOkrMsg.Replace("<ownername>", userIdentity.FirstName).Replace("<OKR>", goalKey.KeyDescription),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);
        }


        public async Task UpdateProgress(long? senderEmpid, GoalKey contributors, string jwtToken, long goalKeyId, decimal currentValue, int year)

        {
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var blobCdnUrl = keyVault.BlobCdnCommonUrl ?? "";
            var loginUrl = settings?.FrontEndUrl + Configuration.GetSection("OkrFrontendURL:SecretLogin").Value;
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
     
            var url = settings?.FrontEndUrl;
            var firstNameLoginUser = string.Empty;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?redirectUrl=unlock-me&empId=" + contributors.EmployeeId;

            }
           var keyDetails = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == goalKeyId && x.IsActive);

            firstNameLoginUser = allEmployee.FirstOrDefault(x => x.EmployeeId == senderEmpid).FirstName;
            var firstname = allEmployee.FirstOrDefault(x => x.EmployeeId == contributors.EmployeeId).FirstName;
            var emailId = allEmployee.FirstOrDefault(x => x.EmployeeId == contributors.EmployeeId).EmailId;
            string finalCurrentvalue = "";
            var template = new MailerTemplate();
            if ((contributors.MetricId == (int)MetricType.NoUnits) || (contributors.MetricId == (int)MetricType.Boolean))
            {
                template = await commonService.GetMailerTemplateAsync(TemplateCodes.KP.ToString(), jwtToken);
            }
            else
            {
                template = await commonService.GetMailerTemplateAsync(TemplateCodes.KP2.ToString(), jwtToken);
                if (contributors.MetricId == (int)MetricType.Currency)
                {
                    finalCurrentvalue = currentValue.ToString() + Constants.DollarSymbol;
                }
                if (contributors.MetricId == (int)MetricType.Numbers)
                {
                    finalCurrentvalue = currentValue.ToString() + Constants.numberText;
                }
                if (contributors.MetricId == (int)MetricType.Percentage)
                {
                    finalCurrentvalue = currentValue.ToString() + Constants.percentageText;
                }
            }

            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var InstagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            string body = template.Body;
            string subject = template.Subject.Replace("<Contributor name>", firstNameLoginUser);


            var keyUrl = contributors.GoalObjectiveId != 0 ? url + "?redirectUrl=" + "KRAcceptDecline" + "/" + contributors.AssignmentTypeId + "/" + contributors.GoalObjectiveId + "/" + contributors.GoalKeyId + "&empId=" + contributors.EmployeeId + "&cycleId=" + contributors.CycleId + "&year=" + year : url + "?redirectUrl=" + "KRAcceptDecline" + "/" + contributors.AssignmentTypeId + "/" + contributors.GoalKeyId + "/" + contributors.GoalObjectiveId + "&empId=" + contributors.EmployeeId + "&cycleId=" + contributors.CycleId + "&year=" + year; 
            var notificationsURL = contributors.GoalObjectiveId != 0 ? "KRAcceptDecline" + "/" + contributors.AssignmentTypeId + "/" + contributors.GoalObjectiveId + "/" + contributors.GoalKeyId : "KRAcceptDecline" + "/" + contributors.AssignmentTypeId + "/" + contributors.GoalKeyId + "/" + contributors.GoalObjectiveId;

            if (keyDetails.Score > 100)
            {
                keyDetails.Score = 100;
            }
            if (keyDetails.Score < 0)
            {
                keyDetails.Score = 0;
            }

            body = body.Replace("topBar", blobCdnUrl + Constants.TopBar)
               .Replace("logo", blobCdnUrl + Constants.LogoImages)
               .Replace("tick", blobCdnUrl + Constants.TickImages).Replace("<URL>", loginUrl).Replace("footer", blobCdnUrl + Constants.FooterLogo)
               .Replace("dot", blobCdnUrl + Constants.dotImage)
                   .Replace("year", Convert.ToString(DateTime.Now.Year)).Replace("keyDescription", contributors.KeyDescription).
               Replace("privacy", url + Constants.Privacy).Replace("terming", url + Constants.Terms).Replace("hand-shake", blobCdnUrl + Constants.Hand)
                .Replace("tinggg", keyUrl)
               .Replace("<abcdefg>", keyUrl)
                .Replace("srcInstagram", blobCdnUrl + Constants.Instagram).Replace("srcLinkedin", blobCdnUrl + Constants.Linkedin)
                 .Replace("srcTwitter", blobCdnUrl + Constants.Twitter).Replace("srcFacebook", blobCdnUrl + Constants.Facebook)
                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("changesinokr", blobCdnUrl + Constants.ChangesinOkr)
                 .Replace("ijk", InstagramUrl).Replace("source", firstNameLoginUser).Replace("name", firstname).Replace("ProgressPercentage", Decimal.Truncate(keyDetails.Score).ToString() + "%").Replace("ProgressValue", finalCurrentvalue);

            if (contributors.AssignmentTypeId == 2)
            {
                var objName = GetGoalObjective(contributors.GoalObjectiveId).ObjectiveName;
                string finalobjName = "linked to " + objName.Trim();
                body = body.Replace("KRtitle", contributors.KeyDescription.Trim()).Replace("ParentObjective", finalobjName.Trim());
            }
            else
            {
                body = body.Replace("KRtitle", contributors.KeyDescription.Trim()).Replace("ParentObjective", "");
            }



            if (emailId != null && template.Subject != "")
            {
                MailRequest mailRequest = new MailRequest
                {
                    MailTo = emailId,
                    Subject = subject,
                    Body = body
                };
                await commonService.SentMailAsync(mailRequest, jwtToken);
            }

            NotificationsCommonRequest notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = (long)senderEmpid,
                To = Convert.ToInt64(contributors.EmployeeId),
                Url = notificationsURL,
                NotificationText = Constants.ProgressUpdate.Replace("<contributor>", firstNameLoginUser).Replace("<KR>", contributors.KeyDescription),
                AppId = Constants.AppID,
                NotificationType = (int)NotificationType.ObjContributors,
                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);
        }

        public async Task VirtualLinkingNotifications(long empId, UserIdentity userIdentity, string jwtToken)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var leaderDetails = allEmployee.FirstOrDefault(x => x.EmployeeId == empId);
            var leaderFirstName = leaderDetails == null ? "N" : leaderDetails.FirstName;
            var notificationsCommonRequest = new NotificationsCommonRequest
            {
                By = userIdentity.EmployeeId,
                To = empId,
                Url = "",
                NotificationText = Constants.VirtualLinkingMsg.Replace("<LeaderName>", leaderFirstName)
                    .Replace("<UserName>", userIdentity.FirstName),
                AppId = Constants.AppID,
                NotificationType = (int) NotificationType.ObjContributors,
                MessageType = (int) MessageTypeForNotifications.NotificationsMessages,
                JwtToken = jwtToken
            };
            await commonService.NotificationsAsync(notificationsCommonRequest);
        }

        public async Task UpdateDueDateNotifications(List<DueDateResponse> dueDateResponse, int goaltype, string jwtToken, long loginEmpId)
        {
            var allEmployee = commonService.GetAllUserFromUsers(jwtToken).Results;
            var leaderDetails = allEmployee.FirstOrDefault(x => x.EmployeeId == loginEmpId); 

            if (dueDateResponse.Count > 0)
            {
                foreach (var item in dueDateResponse)
                {
                    string notificationsText = string.Empty;
                    string url = string.Empty;
                    if (item.EmployeeId != loginEmpId)
                    {
                        if(item.OkrId > 0)
                        {
                            var objName = goalObjectiveRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == item.OkrId && x.IsActive);
                            if(objName != null)
                            {
                                var keyName = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalObjectiveId == objName.GoalObjectiveId && x.IsActive);
                                notificationsText = Constants.UpdateGoalObjective.Replace("<OKR>", objName.ObjectiveName).Replace("<Owner>", leaderDetails.FirstName);
                                if(keyName != null)
                                {
                                    url = "hightlightOKR/" + item.OkrId + "/" + keyName.GoalKeyId + "/1";
                                }
                               

                            }
                        }
                        else
                        {
                            var keyName = goalKeyRepo.GetQueryable().FirstOrDefault(x => x.GoalKeyId == item.KrId.FirstOrDefault() && x.IsActive);
                            if (keyName.GoalObjectiveId > 0)
                            {
                                url = "hightlightOKR/" + keyName.GoalObjectiveId + "/" + keyName.GoalKeyId + "/1";
                            }
                            else
                            {
                                url = "hightlightOKR/" + item.KrId.FirstOrDefault() + "/2";
                            }
                            notificationsText = Constants.UpdateGoalObjective.Replace("<OKR>", keyName.KeyDescription).Replace("<Owner>", leaderDetails.FirstName);

                        }
                        if(url != "")
                        {
                            var notificationsCommonRequest = new NotificationsCommonRequest
                            {
                                By = loginEmpId,
                                To = item.EmployeeId,
                                Url = url,
                                NotificationText = notificationsText,
                                AppId = Constants.AppID,
                                NotificationType = (int)NotificationType.ObjContributors,
                                MessageType = (int)MessageTypeForNotifications.NotificationsMessages,
                                JwtToken = jwtToken
                            };
                            await commonService.NotificationsAsync(notificationsCommonRequest);
                        }
                      
                    }
                  
                }
            }
        }
    }
}
