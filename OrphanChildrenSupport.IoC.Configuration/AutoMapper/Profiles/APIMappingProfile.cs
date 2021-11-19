using AutoMapper;
using OrphanChildrenSupport.DataContracts;
using OrphanChildrenSupport.DataContracts.Resources;
using OrphanChildrenSupport.Services.Models;

namespace OrphanChildrenSupport.IoC.Configuration.AutoMapper.Profiles
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            CreateMap<PersonalProfile, PersonalProfileResource>().ReverseMap();
            CreateMap(typeof(QueryResult<>), typeof(QueryResultResource<>));
        }
    }
}
