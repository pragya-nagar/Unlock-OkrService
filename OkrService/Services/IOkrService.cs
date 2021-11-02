using OkrService.DataContract;
using System.Collections.Generic;
using System.Net;
using OkrService.Models;

namespace OkrService.Services
{
    public interface IOkrService
    {
        List<PreviousOkrResponse> GetOkrDetails(long empId, long supId, int qtr, int year);
        bool GetHeaderValue(string header);
        UserDetail GetLoginUser();
        List<PassportEmployeeResponse> GetAllUserFromPassport();
        List<UserManagementResponse> GetAllUserFromUserManagement(string jwtToken);
        HttpWebRequest GetHttpWebRequest(string restUrl, string method, Dictionary<string, string> parameters, Dictionary<string, string> headers = null, string jwtToken = null);
        void SaveLog(string pageName, string functionName, string applicationName, string errorDetail);
        List<ProgressionMaster> SaveProgression(List<ProgressionRequest> progressionRequests, Identity loginUser);
        List<ProgressionMaster> GetProgression(int year, int period);
        List<SaveOkrResponce> SaveOkr(List<OkrRequest> okrRequest, string jwtToken);
        List<OkrRequest> MyOkr(long userId, int quarter, int year, string jwtToken);
        List<StatusMaster> GetStatusMasters();
        string ResetOkr(long employeeId, int year, int quarter,long okrId, long loginEmp, long resetType);
        List<QuarterSummaryResponse> QuarterSummary(int quarter, int year, string jwtToken);
        List<OkrStatusResponse> OkrStatusReport(int quarter, int year, string jwtToken);
        List<OkrProgressResponse> OkrProgressReport(int quarter, int year, string jwtToken);
        ImportOkr ImportOkr( long reportingId, int year, int qtr , string jwtToken);
        List<OkrSettingsForProgression> OkrSettingsForProgressions();
        List<OkrRequest> MyScoreOkr(long userId, int quarter, int year, string jwtToken);     
        CommentResponce SaveComments(CommentRequest commentRequest, string jwtToken);
        string ReadComments(long employeeId, int type,long typeId, string jwtToken);
        ImportEmployeeOkr ImportEmployeeOkr(long EmployeeId, int Year, int qtr, string jwtToken);
        List<CommentResponce> GetComments(int type, long typeId, string jwtToken);
        Identity GetIdentity(string jwtToken);
        List<OkrRequest> OkrByOkrId(long OkrId, string jwtToken, Identity identity);
        string DeleteOkr(long okrId, long keyId, string jwtToken);
        string DeleteComments(long CommentsId);
        void SaveNotification(NotificationsResponse notificationsResponse);
        Dashboard Dashboard(long userId, int quarter, int year, string jwtToken, Identity identity);
        bool AutoSubmit();
        List<MyOkrTeamResponse> GetMyOkrTeam(int quarter, int year, string jwtToken, Identity identity);
        int CalculateAvgScoreAtAutoSubmit(int quarter, int year);
        List<OkrResetLog> GetResetLog();

        ScoreCardResponse ScoreCard(long userId, int quarter, int year, string jwtToken, Identity loginUser);

    }
}
