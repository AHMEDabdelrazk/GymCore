using AutoMapper;
using GymCore.API.DTOs.Plan;
using GymCore.API.Models.Entities;

namespace GymCore.API.Mappings;

public class PlanProfile : Profile
{
    public PlanProfile()
    {
        CreateMap<MembershipPlan, PlanDto>();

        CreateMap<CreatePlanDto, MembershipPlan>();

        CreateMap<UpdatePlanDto, MembershipPlan>();
    }
}