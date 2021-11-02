using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System;
using System.Reflection;
using Xunit;

namespace OKRService.UnitTests.ViewModel
{
    public class ViewModelTestCases
    {
        [Fact]
        public void AlignKeyResponseModel()
        {
            AlignKeyResponse model = new AlignKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignmentMapResponseModel()
        {
            AlignmentMapResponse model = new AlignmentMapResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void GoalMapAlignmentModel()
        {
            GoalMapAlignment model = new GoalMapAlignment();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignOkrResponseModel()
        {
            AlignOkrResponse model = new AlignOkrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignParentRequestModel()
        {
            AlignParentRequest model = new AlignParentRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignResponseModel()
        {
            AlignResponse model = new AlignResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AvgOkrScoreResponseModel()
        {
            AvgOkrScoreResponse model = new AvgOkrScoreResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ContributorsDotResponseModel()
        {
            ContributorsDotResponse model = new ContributorsDotResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ContributorsResponseModel()
        {
            ContributorsResponse model = new ContributorsResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ReportContributorsWithScoreModel()
        {
            ReportContributorsWithScore model = new ReportContributorsWithScore();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void CycleLockDateModel()
        {
            CycleLockDate model = new CycleLockDate();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void DashboardKeyResponseModel()
        {
            DashboardKeyResponse model = new DashboardKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void DashboardOkrResponseModel()
        {
            DashboardOkrResponse model = new DashboardOkrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void DeleteKrResponseModel()
        {
            DeleteKrResponse model = new DeleteKrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void FeedbackResponseModel()
        {
            FeedbackResponse model = new FeedbackResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void GoalUnlockDateModel()
        {
            GoalUnlockDate model = new GoalUnlockDate();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void EmployeeResultModel()
        {
            EmployeeResult model = new EmployeeResult();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KeyContributorsResponseModel()
        {
            KeyContributorsResponse model = new KeyContributorsResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KeyScoreUpdateModel()
        {
            KeyScoreUpdate model = new KeyScoreUpdate();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KrContributorsModel()
        {
            KrContributors model = new KrContributors();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MailerTemplateModel()
        {
            MailerTemplate model = new MailerTemplate();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MailRequestModel()
        {
            MailRequest model = new MailRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalDetailResponseModel()
        {
            MyGoalDetailResponse model = new MyGoalDetailResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void GoalKeyDetailsModel()
        {
            GoalKeyDetails model = new GoalKeyDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalKeyResponseModel()
        {
            MyGoalKeyResponse model = new MyGoalKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalOkrResponseModel()
        {
            MyGoalOkrResponse model = new MyGoalOkrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalResponseModel()
        {
            MyGoalResponse model = new MyGoalResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignmentResponseModel()
        {
            AlignmentResponse model = new AlignmentResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalsPdfResponseModel()
        {
            MyGoalsPdfResponse model = new MyGoalsPdfResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PdfGoalKeyResponseModel()
        {
            PdfGoalKeyResponse model = new PdfGoalKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PdfOkrResponseModel()
        {
            PdfOkrResponse model = new PdfOkrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PdfUniqueContributorsResponseModel()
        {
            PdfUniqueContributorsResponse model = new PdfUniqueContributorsResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalsRequestModel()
        {
            MyGoalsRequest model = new MyGoalsRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ContributorsResponseForPdfModel()
        {
            ContributorPdfResponse model = new ContributorPdfResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MyGoalsDetailsModel()
        {
            MyGoalsDetails model = new MyGoalsDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void NotificationsCommonRequestModel()
        {
            NotificationsCommonRequest model = new NotificationsCommonRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void NotificationsRequestModel()
        {
            NotificationsRequest model = new NotificationsRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ObjectiveContributorsModel()
        {
            ObjectiveContributors model = new ObjectiveContributors();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void OrganizationCycleDetailsModel()
        {
            OrganisationCycleDetails model = new OrganisationCycleDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void CycleDetailsModel()
        {
            CycleDetails model = new CycleDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void QuarterDetailsModel()
        {
            QuarterDetails model = new QuarterDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PassportEmployeeResponseModel()
        {
            PassportEmployeeResponse model = new PassportEmployeeResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ContactDetailsModel()
        {
            ContactDetails model = new ContactDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PeopleAlignmentResponseModel()
        {
            PeopleAlignmentResponse model = new PeopleAlignmentResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PeopleKeyResponseModel()
        {
            PeopleKeyResponse model = new PeopleKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PeopleMapResponseModel()
        {
            PeopleMapResponse model = new PeopleMapResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PeopleOkrResponseModel()
        {
            PeopleOkrResponse model = new PeopleOkrResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PeopleResponseModel()
        {
            PeopleResponse model = new PeopleResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ProgressDetailModel()
        {
            ProgressDetail model = new ProgressDetail();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ProgressReportModel()
        {
            ProgressReport model = new ProgressReport();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ProgressReportResponseModel()
        {
            ProgressReportResponse model = new ProgressReportResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void QuarterReportResponseModel()
        {
            QuarterReportResponse model = new QuarterReportResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ReportMostLeastObjectiveModel()
        {
            ReportMostLeastObjective model = new ReportMostLeastObjective();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ReportMostLeastObjectiveKeyResultModel()
        {
            ReportMostLeastObjectiveKeyResult model = new ReportMostLeastObjectiveKeyResult();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void RoleDetailsModel()
        {
            RoleDetails model = new RoleDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void StatusReportResponseModel()
        {
            StatusReportResponse model = new StatusReportResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UnLockRequestModel()
        {
            UnLockRequest model = new UnLockRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UserGoalKeyResponseModel()
        {
            UserGoalKeyResponse model = new UserGoalKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UserIdentityModel()
        {
            UserIdentity model = new UserIdentity();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UserResponseModel()
        {
            UserResponse model = new UserResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void WeeklyReportResponseModel()
        {
            WeeklyReportResponse model = new WeeklyReportResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KrGrowthCycleResponseModel()
        {
            KrGrowthCycleResponse model = new KrGrowthCycleResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ModelStateError()
        {
            ModelStateError model = new ModelStateError();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void AssignmentTypeResponse()
        {

            AssignmentTypeResponse model = new AssignmentTypeResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void MetricDataMasterResponse()
        {

            MetricDataMasterResponse model = new MetricDataMasterResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]

        public void MetricMasterResponse()
        {

            MetricMasterResponse model = new MetricMasterResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }



        [Fact]
        public void OrganizationCycleResponseModel()
        {
            OrganisationCycleResponse model = new OrganisationCycleResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KrValueUpdateModel()
        {
            KrValueUpdate model = new KrValueUpdate();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateGoalDescriptionRequestModel()
        {
            UpdateGoalDescriptionRequest model = new UpdateGoalDescriptionRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AddContributorRequestModel()
        {
            AddContributorRequest model = new AddContributorRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void NudgeTeamRequestModel()
        {
            NudgeTeamRequest model = new NudgeTeamRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void AllOkrViewResponseModel()
        {
            AllOkrViewResponse model = new AllOkrViewResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);

        }
        [Fact]
        public void TeamOkrResponseModel()
        {
            TeamOkrResponse model = new TeamOkrResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void OkrViewResponseModel()
        {
            OkrViewResponse model = new OkrViewResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);

        }
        [Fact]
        public void OkrStatusMasterDetailsModel()
        {
            OkrStatusMasterDetails model = new OkrStatusMasterDetails();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void OkrViewKeyResultsModel()
        {
            OkrViewKeyResults model = new OkrViewKeyResults();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void LeaderDetailsResponseModel()
        {
            LeaderDetailsResponse model = new LeaderDetailsResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void OkrViewContributorsModel()
        {
            OkrViewContributors model = new OkrViewContributors();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);

        }

        [Fact]
        public void ActiveOrganisationsResponseModel()

        {
            ActiveOrganisationsResponse model = new ActiveOrganisationsResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]
        public void TeamHeadDetailsModel()
        {
            TeamHeadDetails model = new TeamHeadDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void TeamOkrCardResponseModel()
        {
            TeamOkrCardResponse model = new TeamOkrCardResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateTeamSequenceRequestModel()
        {
            UpdateTeamSequenceRequest model = new UpdateTeamSequenceRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]
        public void KrCalculationResponseModel()
        {
            KrCalculationResponse model = new KrCalculationResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void TeamDetailsModel()
        {
            TeamDetails model = new TeamDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void NotificationsDetailsModel()
        {
            NotificationsDetails model = new NotificationsDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]
        public void AllTeamOkrViewModel()
        {
            AllTeamOkrViewResponse model = new AllTeamOkrViewResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void ContributorKeyResultRequestModel()
        {
            ContributorKeyResultRequest model = new ContributorKeyResultRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void LeaderDetailsModel()
        {
            LeaderDetailsAlignmentResponse model = new LeaderDetailsAlignmentResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void KeyDetailsResponseModel()
        {
            KeyDetailsResponse model = new KeyDetailsResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void PeopleViewResponseModel()
        {
            PeopleViewResponse model = new PeopleViewResponse();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]

        public void ContributorDetailsModel()
        {
            ContributorDetails model = new ContributorDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]
        public void PeopleViewRequestModel()
        {
            PeopleViewRequest model = new PeopleViewRequest();

            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }


        [Fact]
        public void KrStatusContributorResponseModel()
        {
            KrStatusContributorResponse model = new KrStatusContributorResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void TeamOkrRequestModel()
        {
            TeamOkrRequest model = new TeamOkrRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void CycleLockDetailsModel()
        {
            CycleLockDetails model = new CycleLockDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateGoalRequestModel()
        {
            UpdateGoalRequest model = new UpdateGoalRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateSequenceRequestModel()
        {
            UpdateSequenceRequest model = new UpdateSequenceRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AlignStatusResponseModel()
        {
            AlignStatusResponse model = new AlignStatusResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void KrContributorResponseModel()
        {
            KrContributorResponse model = new KrContributorResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateNotificationUrlRequestModel()
        {
            UpdateNotificationURLRequest model = new UpdateNotificationURLRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void LinkedObjectiveResponseModel()
        {
            LinkedObjectiveResponse model = new LinkedObjectiveResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void DirectReportsResponseModel()
        {
            DirectReportsResponse model = new DirectReportsResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void LastSevenDaysProgressResponseModel()
        {
            LastSevenDaysProgressResponse model = new LastSevenDaysProgressResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ContributorsLastSevenDaysProgressResponseModel()
        {
            ContributorsLastSevenDaysProgressResponse model = new ContributorsLastSevenDaysProgressResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void LastSevenDaysStatusCardProgressModel()
        {
            LastSevenDaysStatusCardProgress model = new LastSevenDaysStatusCardProgress();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ScoringKeyModel()
        {
            ScoringKey model = new ScoringKey();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AllOkrDashboardResponseModel()
        {
            AllOkrDashboardResponse model = new AllOkrDashboardResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void DeltaResponseModel()
        {
            DeltaResponse model = new DeltaResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AllOkrDashboardProgressDetailModel()
        {
            AllOkrDashboardProgressDetail model = new AllOkrDashboardProgressDetail();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AllOkrDashboardOkrKRResponseModel()
        {
            AllOkrDashboardOkrKRResponse model = new AllOkrDashboardOkrKRResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AllOkrDashboardKeyResponseModel()
        {
            AllOkrDashboardKeyResponse model = new AllOkrDashboardKeyResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateTeamLeaderOkrRequestModel()
        {
            UpdateTeamLeaderOkrRequest model = new UpdateTeamLeaderOkrRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void TeamPerformanceResponseModel()
        {
            TeamPerformanceResponse model = new TeamPerformanceResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);

        }

        #region Private Methods

        private T GetModelTestData<T>(T newModel)
        {
            Type type = newModel.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var propTypeInfo = type.GetProperty(prop.Name.Trim());
                if (propTypeInfo.CanRead)
                    prop.GetValue(newModel);
            }

            return newModel;
        }

        private T SetModelTestData<T>(T newModel)
        {
            Type type = newModel.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var propTypeInfo = type.GetProperty(prop.Name.Trim());
                var propType = prop.GetType();

                if (propTypeInfo.CanWrite)
                {
                    if (prop.PropertyType.Name == "String")
                    {
                        prop.SetValue(newModel, String.Empty);
                    }
                    else if (propType.IsValueType)
                    {
                        prop.SetValue(newModel, Activator.CreateInstance(propType));
                    }
                    else
                    {
                        prop.SetValue(newModel, null);
                    }
                }
            }

            return newModel;
        }

        #endregion
    }
}

