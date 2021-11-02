using System;
using AutoMapper;
using OKRService.EF;
using OKRService.ViewModel.Request;
using OKRService.ViewModel.Response;
using System.Diagnostics.CodeAnalysis;

namespace OKRService.Service.AutoMapper
{
    [ExcludeFromCodeCoverage]
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GoalKey, GoalKeyDetails>();
            CreateMap<MyGoalsRequest, GoalObjective>();
            CreateMap<GoalObjective, GoalObjectiveResponse>();
            CreateMap<ContributorDetailRequest, GoalKey>();
            CreateMap<GoalKey, KrCalculationResponse>();


            CreateMap<GoalObjective, OkrViewResponse>()
                .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalObjectiveId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.ObjectiveName))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.EndDate))
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.IsPrivate, o => o.MapFrom(s => s.IsPrivate))
                .ForMember(d => d.IsSourceExist, o => o.MapFrom(s => s.ImportedId != 0))
                .ForMember(d => d.Progress, o => o.MapFrom(s => s.Progress))
                .ForMember(d => d.Score, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
                .ForMember(d => d.ObjectiveTypeId, o => o.MapFrom(s => s.GoalTypeId))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
                .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
                .ForMember(d => d.Owner, o => o.MapFrom(s => s.Owner))
                .ForMember(d => d.ObjectiveDescription, o => o.MapFrom(s => s.ObjectiveDescription))
                 .ForMember(d => d.LinkedObjectiveId, o => o.MapFrom(s => s.LinkedObjectiveId));

            CreateMap<GoalKey, OkrViewResponse>()
                .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.KrStatusId, o => o.MapFrom(s => s.KrStatusId))
                .ForMember(d => d.Progress, o => o.MapFrom(s => s.Progress))
                .ForMember(d => d.Score, o => o.MapFrom(s => Math.Round(s.Score)))
                .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
                .ForMember(d => d.KrStartValue, o => o.MapFrom(s => s.StartValue))
                .ForMember(d => d.KrTargetValue, o => o.MapFrom(s => s.TargetValue))
                .ForMember(d => d.KrCurrentValue, o => o.MapFrom(s => s.CurrentValue))
                ///.ForMember(d => d.ParentId, o => o.MapFrom(s => s.ImportedId))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
                .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
                .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.CurrencyId))
                .ForMember(d => d.MetricId, o => o.MapFrom(s => s.MetricId))
                .ForMember(d => d.AssignmentTypeId, o => o.MapFrom(s => s.AssignmentTypeId))
                 .ForMember(d => d.KeyNotes, o => o.MapFrom(s => s.KeyNotes));

            CreateMap<GoalKey, OkrViewKeyResults>()
                .ForMember(d => d.KrId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.KrName, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.KrCurrentValue, o => o.MapFrom(s => s.CurrentValue))
                .ForMember(d => d.KrContributorValue, o => o.MapFrom(s => s.ContributorValue))
                .ForMember(d => d.KrProgress, o => o.MapFrom(s => s.Progress))
                .ForMember(d => d.KrScore, o => o.MapFrom(s => Math.Round(s.Score)))
                .ForMember(d => d.KrStartValue, o => o.MapFrom(s => s.StartValue))
                .ForMember(d => d.KrTargetValue, o => o.MapFrom(s => s.TargetValue))
                 .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
                .ForMember(d => d.KrDueDate, o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.KrStatusId, o => o.MapFrom(s => s.KrStatusId))
                .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.CurrencyId))
                .ForMember(d => d.MetricId, o => o.MapFrom(s => s.MetricId))
                .ForMember(d => d.AssignmentTypeId, o => o.MapFrom(s => s.AssignmentTypeId))
                  .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                  .ForMember(d => d.KeyNotes, o => o.MapFrom(s => s.KeyNotes));


            CreateMap<UserResponse, OkrViewContributors>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.ImagePath.Trim()));
            CreateMap<GoalKey, PeopleViewKeyResults>()
                .ForMember(d => d.KrId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.KrName, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.KrScore, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
                .ForMember(d => d.CreatedOn, o => o.MapFrom(s => s.CreatedOn));
            CreateMap<GoalObjective, PeopleViewObjectives>()
                .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalObjectiveId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.ObjectiveName))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.EndDate))
                .ForMember(d => d.Score, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
                .ForMember(d => d.ObjectiveTypeId, o => o.MapFrom(s => s.GoalTypeId))
                .ForMember(d => d.CreatedOn, o => o.MapFrom(s => s.CreatedOn))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId));
            CreateMap<GoalKey, PeopleViewObjectives>()
                .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.Score, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId));
            CreateMap<UserResponse, PeopleViewContributors>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.ImagePath.Trim()));
            CreateMap<UserResponse, PeopleViewResponse>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.OrganisationID))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.ImagePath.Trim()));

            CreateMap<LeaderDetailsResponse, LeaderDetailsAlignmentResponse>()
                 .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName));

            CreateMap<GoalObjective, AllTeamOkrViewResponse>()
               .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalObjectiveId))
               .ForMember(d => d.Name, o => o.MapFrom(s => s.ObjectiveName))
               .ForMember(d => d.DueDate, o => o.MapFrom(s => s.EndDate))
               .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
               .ForMember(d => d.IsPrivate, o => o.MapFrom(s => s.IsPrivate))
               .ForMember(d => d.IsSourceExist, o => o.MapFrom(s => s.ImportedId != 0))
               .ForMember(d => d.Progress, o => o.MapFrom(s => s.Progress))
               .ForMember(d => d.Score, o => o.MapFrom(s => s.Score))
               .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
               .ForMember(d => d.ObjectiveTypeId, o => o.MapFrom(s => s.GoalTypeId))
               .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
               .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
               .ForMember(d => d.Owner, o => o.MapFrom(s => s.Owner));

            CreateMap<GoalKey, AllTeamOkrViewResponse>()
               .ForMember(d => d.ObjectiveId, o => o.MapFrom(s => s.GoalKeyId))
               .ForMember(d => d.Name, o => o.MapFrom(s => s.KeyDescription))
               .ForMember(d => d.DueDate, o => o.MapFrom(s => s.DueDate))
               .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
               .ForMember(d => d.KrStatusId, o => o.MapFrom(s => s.KrStatusId))
               .ForMember(d => d.Progress, o => o.MapFrom(s => s.Progress))
               .ForMember(d => d.Score, o => o.MapFrom(s => s.Score))
               .ForMember(d => d.ObjectiveStatusId, o => o.MapFrom(s => s.GoalStatusId))
               .ForMember(d => d.KrStartValue, o => o.MapFrom(s => s.StartValue))
               .ForMember(d => d.KrTargetValue, o => o.MapFrom(s => s.TargetValue))
               .ForMember(d => d.KrCurrentValue, o => o.MapFrom(s => s.CurrentValue))
               .ForMember(d => d.ParentId, o => o.MapFrom(s => s.ImportedId))
               .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
               .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
               .ForMember(d => d.CurrencyId, o => o.MapFrom(s => s.CurrencyId))
               .ForMember(d => d.MetricId, o => o.MapFrom(s => s.MetricId))
               .ForMember(d => d.AssignmentTypeId, o => o.MapFrom(s => s.AssignmentTypeId));

            CreateMap<DirectReportsDetails, DirectReportsResponse>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation))
                .ForMember(d => d.OrganisationId, o => o.MapFrom(s => s.OrganisationId))
                .ForMember(d => d.ImagePath, o => o.MapFrom(s => s.ImagePath));

            CreateMap<UserResponse, DirectReportsResponse>()
                .ForMember(d => d.EmployeeId, o => o.MapFrom(s => s.EmployeeId))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation))
                .ForMember(d => d.OrganisationId, o => o.MapFrom(s => s.OrganisationID))
                .ForMember(d => d.ImagePath, o => o.MapFrom(s => s.ImagePath));

            CreateMap<GoalKey, DirectReportsKeyResult>()
                .ForMember(d => d.KrId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.KrName, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.DueDate, o => o.MapFrom(s => s.DueDate))
                .ForMember(d => d.KrScore, o => o.MapFrom(s => s.Score))
                .ForMember(d => d.TeamId, o => o.MapFrom(s => s.TeamId))
                .ForMember(d => d.CreatedOn, o => o.MapFrom(s => s.CreatedOn));

            CreateMap<GoalObjective, DirectReportsObjectives>();
            CreateMap<GoalKey, DirectReportsObjectives>()
                .ForMember(d => d.GoalObjectiveId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.ObjectiveName, o => o.MapFrom(s => s.KeyDescription))
                .ForMember(d => d.EndDate, o => o.MapFrom(s => s.DueDate));


            CreateMap<GoalKey, KrCalculationAlignmentMapResponse>()
                .ForMember(d => d.KrId, o => o.MapFrom(s => s.GoalKeyId))
                .ForMember(d => d.KrScore, o => o.MapFrom(s => Math.Round(s.Score)))
                .ForMember(d => d.KrCurrentValue, o => o.MapFrom(s => s.CurrentValue));

            CreateMap<UpdateDueDateRequest, GoalObjective>();
            CreateMap<AllLevelObjectiveResponse, GoalObjective>();
        }
    }
}
