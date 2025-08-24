using AutoMapper;
using TaskManager.Models.Dtos;
using TaskManager.Models.Entities;

namespace TaskManager.Mappings
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TaskEntity, TaskDTO>()
                .ForMember(dto => dto.TotalSteps, entity => entity.MapFrom(en => en.Steps.Count()))
                .ForMember(dto => dto.DoneSteps, entity => entity.MapFrom(en => en.Steps.Where(step => step.Done).Count()));
            CreateMap<TaskUpdateDTO, TaskEntity>();
        }
    }
}
